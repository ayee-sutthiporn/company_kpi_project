using CompanyKPI_Project.Models;
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
        // Mock Data
        public static List<DeptKpiUpload> _mockDeptUploads = new List<DeptKpiUpload>();

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
            // Fiscal Year 2025 = Apr 2025 to Mar 2026? Or Jan-Dec?
            // User context suggests "Fiscal Year" logic in previous steps (Apr start). 
            // Let's stick to Fiscal Year (Apr start).
            var startDate = new DateTime(targetYear, 4, 1);
            
            for (int i = 0; i < 12; i++)
            {
                var d = startDate.AddMonths(i);
                var upload = _mockDeptUploads.FirstOrDefault(u => u.Year == targetYear && u.Month == d.Month && u.Department == targetDept);
                
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
            if (file != null && file.ContentLength > 0)
            {
                // Remove existing
                _mockDeptUploads.RemoveAll(u => u.Year == year && u.Month == month && u.Department == dept);

                var newId = _mockDeptUploads.Any() ? _mockDeptUploads.Max(u => u.Id) + 1 : 1;
                
                // Save to Disk for Download
                var fileName = Path.GetFileName(file.FileName);
                var uploadDir = Server.MapPath("~/App_Data/DeptUploads");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);
                
                var filePath = Path.Combine(uploadDir, $"{newId}_{fileName}");
                file.SaveAs(filePath);

                _mockDeptUploads.Add(new DeptKpiUpload
                {
                    Id = newId,
                    Year = year,
                    Month = month,
                    Department = dept,
                    FileName = fileName,
                    UploadDate = DateTime.Now,
                    UploadBy = "System"
                });
            }
            return RedirectToAction("Index", new { year = year, dept = dept });
        }

        public ActionResult Download(int id)
        {
            var upload = _mockDeptUploads.FirstOrDefault(u => u.Id == id);
            if (upload != null)
            {
                var uploadDir = Server.MapPath("~/App_Data/DeptUploads");
                var filePath = Path.Combine(uploadDir, $"{upload.Id}_{upload.FileName}");
                if (System.IO.File.Exists(filePath))
                {
                    return File(filePath, "application/octet-stream", upload.FileName);
                }
            }
            return HttpNotFound("File not found on server.");
        }
        
        public ActionResult Delete(int id)
        {
            var file = _mockDeptUploads.FirstOrDefault(f => f.Id == id);
            if(file != null)
            {
                _mockDeptUploads.Remove(file);
                return RedirectToAction("Index", new { year = file.Year, dept = file.Department });
            }
            return RedirectToAction("Index");
        }
    }
}
