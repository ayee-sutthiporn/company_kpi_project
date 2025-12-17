using CompanyKPI_Project.Models;
using CompanyKPI_Project.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CompanyKPI_Project.Controllers
{
    public class DepartmentKpiController : Controller
    {
        private IKpiRepository _repository;

        public DepartmentKpiController()
        {
            // Simple Dependency Injection Resolution
            bool useEf = false;
            var configVal = System.Web.Configuration.WebConfigurationManager.AppSettings["UseEfRepository"];
            if (!string.IsNullOrEmpty(configVal))
            {
                bool.TryParse(configVal, out useEf);
            }

            if (useEf)
            {
                _repository = new EfKpiRepository();
            }
            else
            {
                _repository = new MockKpiRepository();
            }
        }

        // Default to Current Year and first Department if not specified
        public ActionResult Index(int? year, string dept)
        {
            var targetYear = year ?? DateTime.Now.Year;

            // Default Dept List
            var depts = new List<string> { "QA", "HR", "Production", "IT", "Sales" };
            ViewBag.Departments = depts;
            ViewBag.Years = new List<int> { 2023, 2024, 2025 };

            var targetDept = dept;
            if (string.IsNullOrEmpty(targetDept)) targetDept = "QA"; // Default

            var model = new DepartmentKpiViewModel
            {
                Year = targetYear,
                Department = targetDept,
                Months = new List<DeptMonthItem>()
            };

            // Generate 12 Months (April to March Fiscal Year)
            var startDate = new DateTime(targetYear, 4, 1);
            var uploads = _repository.GetDeptUploads(targetYear, targetDept).ToList();

            for (int i = 0; i < 12; i++)
            {
                var d = startDate.AddMonths(i);
                var upload = uploads.FirstOrDefault(u => u.Month == d.Month);

                model.Months.Add(new DeptMonthItem
                {
                    Month = d.Month,
                    MonthName = d.ToString("MMMM yyyy"),
                    Upload = upload
                });
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Upload(int year, int month, string dept, HttpPostedFileBase file)
        {
            try
            {
                if (file != null && file.ContentLength > 0)
                {
                    // Remove existing
                    _repository.ClearDeptUpload(year, month, dept);

                    var upload = new DeptKpiUpload
                    {
                        Year = year,
                        Month = month,
                        Department = dept,
                        FileName = file.FileName,
                        UploadDate = DateTime.Now,
                        UploadBy = User.Identity.IsAuthenticated ? User.Identity.Name : System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                        ContentType = file.ContentType
                    };

                    // Read File to Byte Array
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        upload.FileContent = binaryReader.ReadBytes(file.ContentLength);
                    }

                    _repository.AddDeptUpload(upload);

                    // Log Success
                    _repository.AddLog(new ApplicationLog
                    {
                        LogDate = DateTime.Now,
                        Level = "Info",
                        Source = "DeptKpiController/Upload",
                        Message = $"Uploaded Dept File: {file.FileName} for {dept} {month}/{year}",
                        User = User.Identity.IsAuthenticated ? User.Identity.Name : System.Security.Principal.WindowsIdentity.GetCurrent().Name
                    });
                }
            }
            catch (Exception ex)
            {
                // Log Error
                _repository.AddLog(new ApplicationLog
                {
                    LogDate = DateTime.Now,
                    Level = "Error",
                    Source = "DeptKpiController/Upload",
                    Message = $"Upload Failed: {ex.Message}",
                    Exception = ex.ToString(),
                    User = User.Identity.IsAuthenticated ? User.Identity.Name : System.Security.Principal.WindowsIdentity.GetCurrent().Name
                });
                TempData["Error"] = "Upload Failed: " + ex.Message;
            }
            return RedirectToAction("Index", new { year = year, dept = dept });
        }

        public ActionResult Download(int id)
        {
            var upload = _repository.GetDeptUploadById(id);
            if (upload != null && upload.FileContent != null)
            {
                return File(upload.FileContent, upload.ContentType ?? "application/octet-stream", upload.FileName);
            }
            return HttpNotFound("File not found or content is empty.");
        }

        public ActionResult Delete(int id)
        {
            try
            {
                var file = _repository.GetDeptUploadById(id);
                if (file != null)
                {
                    _repository.DeleteDeptUpload(id);

                    // Log Success
                    _repository.AddLog(new ApplicationLog
                    {
                        LogDate = DateTime.Now,
                        Level = "Warning",
                        Source = "DeptKpiController/Delete",
                        Message = $"Deleted Dept File ID: {id}, Dept: {file.Department}",
                        User = User.Identity.IsAuthenticated ? User.Identity.Name : System.Security.Principal.WindowsIdentity.GetCurrent().Name
                    });

                    return RedirectToAction("Index", new { year = file.Year, dept = file.Department });
                }
            }
            catch (Exception ex)
            {
                // Log Error
                _repository.AddLog(new ApplicationLog
                {
                    LogDate = DateTime.Now,
                    Level = "Error",
                    Source = "DeptKpiController/Delete",
                    Message = $"Delete Failed: {ex.Message}",
                    Exception = ex.ToString(),
                    User = User.Identity.IsAuthenticated ? User.Identity.Name : System.Security.Principal.WindowsIdentity.GetCurrent().Name
                });
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
