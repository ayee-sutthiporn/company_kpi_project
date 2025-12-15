using CompanyKPI_Project.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CompanyKPI_Project.Controllers
{
    public class KpiController : Controller
    {
        // Mock Data Store
        public static List<TblTFileUpload> _mockFiles = new List<TblTFileUpload>();
        public static List<TblTDataCompanyKpiHd> _mockHeaders = new List<TblTDataCompanyKpiHd>();
        public static List<TblTDataCompanyKpiDt> _mockDetails = new List<TblTDataCompanyKpiDt>();

        static KpiController()
        {
            // Seed Mock Data on startup
            SeedMockData();
        }

        private static void SeedMockData()
        {
            if (!_mockFiles.Any())
            {
                var years = new[] { 2023, 2024, 2025 };
                var departments = new[] { "QA", "HR", "Production", "IT", "Sales" };
                var kpiTemplates = new[]
                {
                    new { Topic = "Customer claim reduction", Unit = "PPM", Target = 20, Cond = "<=", Dept = "QA", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "Internal Claim reduction", Unit = "PPM", Target = 5, Cond = "<=", Dept = "QA", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "Employee Turnover Rate", Unit = "%", Target = 5, Cond = "<=", Dept = "HR", True = "Pass", False = "Fail" },
                    new { Topic = "Training Hrs / Employee", Unit = "Hrs", Target = 12, Cond = ">=", Dept = "HR", True = "Pass", False = "Fail" },
                    new { Topic = "Production Yield", Unit = "%", Target = 98, Cond = ">=", Dept = "Production", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "Machine Downtime", Unit = "%", Target = 2, Cond = "<=", Dept = "Production", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "System Uptime", Unit = "%", Target = 99, Cond = ">=", Dept = "IT", True = "Pass", False = "Fail" },
                    new { Topic = "Sales Growth", Unit = "%", Target = 10, Cond = ">=", Dept = "Sales", True = "Achieve", False = "Not Achieve" }
                };

                int fileId = 1;
                int hdId = 1;
                int dtId = 1;
                var random = new Random();

                foreach (var year in years)
                {
                    // Create File
                    _mockFiles.Add(new TblTFileUpload
                    {
                        File_Id = fileId,
                        File_Name = $"Master_KPI_{year}.xlsx",
                        File_OfficialYear = year,
                        File_UploadBy = "System",
                        File_UploadDate = DateTime.Now.AddDays(-365 * (2025 - year)),
                        File_Type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        File_IsDeleted = false
                    });

                    // Create Headers
                    foreach (var t in kpiTemplates)
                    {
                        var hd = new TblTDataCompanyKpiHd
                        {
                            Hd_Id = hdId,
                            Hd_File_Id = fileId,
                            Hd_TopicNo = $"{hdId}. {t.Topic}",
                            Hd_Condition = t.Cond,
                            Hd_TargetValue = t.Target,
                            Hd_Unit = t.Unit,
                            Hd_MainPIC = t.Dept,
                            Hd_TrueDesc = t.True,
                            Hd_FalseDesc = t.False,
                            Hd_IsTarget = true,
                            Hd_DetailDescription = $"Target for {year}: {t.Cond} {t.Target} {t.Unit}"
                        };
                        _mockHeaders.Add(hd);

                        // Create Details (Year Starts April)
                        var startDate = new DateTime(year, 4, 1);
                        for (int i = 0; i < 12; i++)
                        {
                            var currentMonth = startDate.AddMonths(i);
                            var isPast = currentMonth < DateTime.Now; // Check if month is in the past relative to execution time
                            
                            string result = "";
                            string actual = "";
                            string udate = "";

                            // Logic: 
                            // 2023, 2024: All filled
                            // 2025: Filled until current month (Simulated as Dec 2025 based on prompt time?) 
                            // Actually context says "current local time is 2025-12-15".
                            
                            if (currentMonth <= DateTime.Now)
                            {
                                // Generate Random Result
                                bool pass = random.NextDouble() > 0.3; // 70% Pass Rate
                                double val = t.Target;
                                
                                if (t.Cond == ">=") val = pass ? t.Target + random.Next(1, 10) : t.Target - random.Next(1, 5);
                                else if (t.Cond == "<=") val = pass ? t.Target - random.Next(1, 3) : t.Target + random.Next(1, 5);
                                
                                actual = val.ToString("0.##");
                                result = pass ? t.True : t.False;
                                udate = "System";
                            }

                            _mockDetails.Add(new TblTDataCompanyKpiDt
                            {
                                DT_Id = dtId++,
                                DT_Hd_Id = hd.Hd_Id,
                                DT_File_Id = fileId,
                                DT_Month = currentMonth,
                                DT_Result = result,
                                DT_ActualValue = actual,
                                DT_UpdateDate = DateTime.Now,
                                DT_UpdateBy = udate
                            });
                        }
                        hdId++;
                    }
                    fileId++;
                }
            }
        }

        public ActionResult Index()
        {
            var files = _mockFiles.Where(f => !f.File_IsDeleted).OrderByDescending(f => f.File_UploadDate).ToList();
            return View(files);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, int year)
        {
            if (file != null && file.ContentLength > 0)
            {
                // Mock Upload Logic
                var newId = _mockFiles.Any() ? _mockFiles.Max(f => f.File_Id) + 1 : 1;
                var fileUpload = new TblTFileUpload
                {
                    File_Id = newId,
                    File_Name = Path.GetFileName(file.FileName),
                    File_Type = file.ContentType,
                    File_Extension = Path.GetExtension(file.FileName),
                    File_OfficialYear = year,
                    File_IsDeleted = false,
                    File_UploadDate = DateTime.Now,
                    File_UploadBy = "Mock User"
                };
                _mockFiles.Add(fileUpload);

                // Create Dummy KPI for this file
                var hd = new TblTDataCompanyKpiHd
                {
                    Hd_Id = _mockHeaders.Count + 1,
                    Hd_File_Id = newId,
                    Hd_TopicNo = "1. NEW UPLOAD " + DateTime.Now.ToString("HH:mm"),
                    Hd_Condition = ">=", Hd_TargetValue = 100, Hd_Unit = "%", Hd_MainPIC = "User",
                    Hd_TrueDesc = "Pass", Hd_FalseDesc = "Fail"
                };
                _mockHeaders.Add(hd);

                var months = new DateTime(year, 4, 1);
                for (int i = 0; i < 12; i++)
                {
                    _mockDetails.Add(new TblTDataCompanyKpiDt 
                    { 
                        DT_Id = _mockDetails.Count + 1, 
                        DT_Hd_Id = hd.Hd_Id, 
                        DT_File_Id = newId, 
                        DT_Month = months.AddMonths(i), 
                        DT_Result = "" 
                    });
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult AdminDetail(int id)
        {
            var file = _mockFiles.FirstOrDefault(f => f.File_Id == id);
            if (file == null) return HttpNotFound();

            ViewBag.File = file;
            var headers = _mockHeaders.Where(h => h.Hd_File_Id == id).OrderBy(h => h.Hd_TopicNo).ToList();
            
            // Link Details manually
            foreach(var h in headers)
            {
                h.Details = _mockDetails.Where(d => d.DT_Hd_Id == h.Hd_Id).ToList();
            }

            return View(headers);
        }

        public ActionResult UserResult(int id)
        {
            // Direct access (fallback if needed)
            var file = _mockFiles.FirstOrDefault(f => f.File_Id == id);
            if (file == null) return HttpNotFound();

            ViewBag.File = file;
            var headers = _mockHeaders.Where(h => h.Hd_File_Id == id).ToList();
             foreach(var h in headers)
            {
                h.Details = _mockDetails.Where(d => d.DT_Hd_Id == h.Hd_Id).ToList();
            }
            return View(headers);
        }

        public ActionResult UserDashboard(int? year, string dept)
        {
            // Default to current year
            int targetYear = year ?? DateTime.Now.Year;
            
            // Find file
            var file = _mockFiles.FirstOrDefault(f => f.File_OfficialYear == targetYear);
            if (file == null) file = _mockFiles.OrderByDescending(f => f.File_OfficialYear).FirstOrDefault();

            if (file == null) return View("UserResult", new List<TblTDataCompanyKpiHd>());

            ViewBag.File = file;
            ViewBag.CurrentYear = targetYear;

            var allHeaders = _mockHeaders.Where(h => h.Hd_File_Id == file.File_Id).ToList();

            // Department Selection Logic
            if (string.IsNullOrEmpty(dept))
            {
                var departments = allHeaders.Select(h => h.Hd_MainPIC).Distinct()
                                            .Where(d => !string.IsNullOrEmpty(d)).OrderBy(d => d).ToList();
                ViewBag.Departments = departments;
                return View("UserDepartmentSelect");
            }

            // Filter Headers
            ViewBag.SelectedDept = dept;
            var headers = allHeaders.Where(h => h.Hd_MainPIC == dept).ToList();

             // Link Details
            foreach(var h in headers)
            {
                h.Details = _mockDetails.Where(d => d.DT_Hd_Id == h.Hd_Id).ToList();
            }

            return View("UserResult", headers);
        }

        [HttpPost]
        public ActionResult UpdateDetail(int dtId, string resultValue, HttpPostedFileBase fileProgressive, HttpPostedFileBase fileActionPlan)
        {
            var dt = _mockDetails.FirstOrDefault(d => d.DT_Id == dtId);
            if (dt != null)
            {
                // Validation: Prevent Future Updates
                if (dt.DT_Month > DateTime.Now)
                {
                     return Json(new { success = false, message = "Cannot update future months." });
                }

                var hd = _mockHeaders.FirstOrDefault(h => h.Hd_Id == dt.DT_Hd_Id);
                var finalResult = resultValue;
                bool isPass = false;

                if (hd != null && hd.Hd_TargetValue.HasValue && double.TryParse(resultValue, out double val))
                {
                    switch (hd.Hd_Condition)
                    {
                        case ">=": isPass = val >= hd.Hd_TargetValue.Value; break;
                        case "<=": isPass = val <= hd.Hd_TargetValue.Value; break;
                        case ">": isPass = val > hd.Hd_TargetValue.Value; break;
                        case "<": isPass = val < hd.Hd_TargetValue.Value; break;
                        case "=": isPass = Math.Abs(val - hd.Hd_TargetValue.Value) < 0.01; break;
                    }
                    finalResult = isPass ? hd.Hd_TrueDesc : hd.Hd_FalseDesc; 
                }
                else
                {
                    finalResult = resultValue; // Fallback
                }
                
                dt.DT_ActualValue = resultValue; // Store raw value
                dt.DT_Result = finalResult;      // Store Result Text (Achieve/Fail)
                dt.DT_UpdateDate = DateTime.Now;
                dt.DT_UpdateBy = "Mock User";
                
                // Save Files
                var uploadDir = Server.MapPath("~/App_Data/KpiResultFiles");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                if (fileProgressive != null && fileProgressive.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(fileProgressive.FileName);
                    var filePath = Path.Combine(uploadDir, $"P_{dt.DT_Id}_{fileName}");
                    fileProgressive.SaveAs(filePath);
                    dt.DT_ProgressiveFile = fileName; // Store original name for display
                }
                if (fileActionPlan != null && fileActionPlan.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(fileActionPlan.FileName);
                    var filePath = Path.Combine(uploadDir, $"A_{dt.DT_Id}_{fileName}");
                    fileActionPlan.SaveAs(filePath);
                    dt.DT_ActionPlanFile = fileName;
                }

                return Json(new { success = true, result = finalResult, isPass = isPass });
            }
            return Json(new { success = false, message = "Item not found" });
        }

        public ActionResult DownloadResultFile(int dtId, string type)
        {
            var dt = _mockDetails.FirstOrDefault(d => d.DT_Id == dtId);
            if (dt != null)
            {
                var uploadDir = Server.MapPath("~/App_Data/KpiResultFiles");
                string fileName = (type == "prog") ? dt.DT_ProgressiveFile : dt.DT_ActionPlanFile;
                string prefix = (type == "prog") ? "P" : "A";
                
                if (!string.IsNullOrEmpty(fileName))
                {
                    var filePath = Path.Combine(uploadDir, $"{prefix}_{dt.DT_Id}_{fileName}");
                    if (System.IO.File.Exists(filePath))
                    {
                        return File(filePath, "application/octet-stream", fileName);
                    }
                }
            }
            return HttpNotFound("File not found.");
        }

        [HttpPost]
        public ActionResult DeleteResultFile(int dtId, string type)
        {
            var dt = _mockDetails.FirstOrDefault(d => d.DT_Id == dtId);
            if (dt != null)
            {
                var uploadDir = Server.MapPath("~/App_Data/KpiResultFiles");
                string fileName = (type == "prog") ? dt.DT_ProgressiveFile : dt.DT_ActionPlanFile;
                string prefix = (type == "prog") ? "P" : "A";

                // Nullify in DB (Mock)
                if (type == "prog") dt.DT_ProgressiveFile = null;
                else dt.DT_ActionPlanFile = null;

                // Delete from disk (Try-Catch to avoid crash if locked)
                try {
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        var filePath = Path.Combine(uploadDir, $"{prefix}_{dt.DT_Id}_{fileName}");
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                } catch { /* Ignore file delete error (orphan file) */ }
                
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Item not found" });
        }

        [HttpPost]
        public ActionResult AddItem(int fileId, string topic, string condition, int target, string unit, string pic, string trueDesc, string falseDesc, int? hdId)
        {
            TblTDataCompanyKpiHd hd;
            
            if (hdId.HasValue && hdId.Value > 0)
            {
                // Edit
                hd = _mockHeaders.FirstOrDefault(h => h.Hd_Id == hdId.Value);
                if (hd != null)
                {
                    hd.Hd_TopicNo = topic;
                    hd.Hd_Condition = condition;
                    hd.Hd_TargetValue = target;
                    hd.Hd_Unit = unit;
                    hd.Hd_MainPIC = pic;
                    hd.Hd_TrueDesc = trueDesc;
                    hd.Hd_FalseDesc = falseDesc;
                }
            }
            else
            {
                // Add
                var newId = _mockHeaders.Any() ? _mockHeaders.Max(h => h.Hd_Id) + 1 : 1;
                hd = new TblTDataCompanyKpiHd
                {
                    Hd_Id = newId,
                    Hd_File_Id = fileId,
                    Hd_TopicNo = topic,
                    Hd_Condition = condition,
                    Hd_TargetValue = target,
                    Hd_Unit = unit,
                    Hd_MainPIC = pic,
                    Hd_TrueDesc = trueDesc,
                    Hd_FalseDesc = falseDesc,
                    Hd_IsTarget = true
                };
                _mockHeaders.Add(hd);

                // Details
                var file = _mockFiles.FirstOrDefault(f => f.File_Id == fileId);
                var year = file?.File_OfficialYear ?? 2025;
                var startDate = new DateTime(year, 4, 1);
                
                int startDtId = _mockDetails.Any() ? _mockDetails.Max(d => d.DT_Id) + 1 : 1;
                for(int i=0; i<12; i++)
                {
                    _mockDetails.Add(new TblTDataCompanyKpiDt
                    {
                        DT_Id = startDtId + i,
                        DT_Hd_Id = newId,
                        DT_File_Id = fileId,
                        DT_Month = startDate.AddMonths(i),
                        DT_Result = ""
                    });
                }
            }
            return RedirectToAction("AdminDetail", new { id = fileId });
        }

        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            var hd = _mockHeaders.FirstOrDefault(h => h.Hd_Id == id);
            if (hd != null)
            {
                // Remove Details first
                _mockDetails.RemoveAll(d => d.DT_Hd_Id == id);
                
                // Remove Header
                _mockHeaders.Remove(hd);
                
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Item not found" });
        }

        public ActionResult Export(int id)
        {
            var file = _mockFiles.FirstOrDefault(f => f.File_Id == id);
            if (file == null) return HttpNotFound();

            var headers = _mockHeaders.Where(h => h.Hd_File_Id == id).OrderBy(h => h.Hd_TopicNo).ToList();
            var details = _mockDetails.Where(d => d.DT_File_Id == id).ToList();

            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("KPI Data");
                
                // Header Row
                worksheet.Cell(1, 1).Value = "No";
                worksheet.Cell(1, 2).Value = "Topic";
                worksheet.Cell(1, 3).Value = "Department";
                worksheet.Cell(1, 4).Value = "Target";
                worksheet.Cell(1, 5).Value = "Unit";
                
                // Months Headers (Apr to Mar)
                var monthStart = 6;
                for (int i = 0; i < 12; i++)
                {
                    worksheet.Cell(1, monthStart + i).Value = new DateTime(file.File_OfficialYear ?? 0, 4, 1).AddMonths(i).ToString("MMM-yy");
                }

                // Data Rows
                int row = 2;
                foreach (var item in headers)
                {
                    worksheet.Cell(row, 1).Value = row - 1; // Simple Index
                    worksheet.Cell(row, 2).Value = item.Hd_TopicNo;
                    worksheet.Cell(row, 3).Value = item.Hd_MainPIC;
                    worksheet.Cell(row, 4).Value = $"{item.Hd_Condition} {item.Hd_TargetValue}";
                    worksheet.Cell(row, 5).Value = item.Hd_Unit;

                    // Fill Monthly Data
                    for (int i = 0; i < 12; i++)
                    {
                        var targetDate = new DateTime(file.File_OfficialYear??0, 4, 1).AddMonths(i);
                        // Approximate Match for Day/Month/Year
                        var dt = details.FirstOrDefault(d => d.DT_Hd_Id == item.Hd_Id && d.DT_Month?.Year == targetDate.Year && d.DT_Month?.Month == targetDate.Month);
                        
                        if (dt != null)
                        {
                            // Export "Actual (Result)"
                            var val = !string.IsNullOrEmpty(dt.DT_ActualValue) ? dt.DT_ActualValue : "";
                            var res = !string.IsNullOrEmpty(dt.DT_Result) ? dt.DT_Result : "";
                            
                            // Format: "15 (Achieve)" or just "15"
                            string cellValue = val;
                            if(!string.IsNullOrEmpty(res)) cellValue += $" ({res})";
                            
                            worksheet.Cell(row, monthStart + i).Value = cellValue;
                            
                            // Color Coding (Optional)
                            if (res == item.Hd_TrueDesc) worksheet.Cell(row, monthStart + i).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
                            else if (res == item.Hd_FalseDesc) worksheet.Cell(row, monthStart + i).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightPink;
                        }
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new System.IO.MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"KPI_Export_{file.File_OfficialYear}.xlsx");
                }
            }
        }

        public ActionResult Summary() { 
            var summaries = new List<YearlyKpiSummaryViewModel>();
            var years = _mockFiles.Select(f => f.File_OfficialYear).Distinct().OrderByDescending(y => y).ToList();

            foreach (var year in years)
            {
                // Get all files for this year (usually just one, but handling duplicates)
                var fileIds = _mockFiles.Where(f => f.File_OfficialYear == year).Select(f => f.File_Id).ToList();
                
                var headers = _mockHeaders.Where(h => fileIds.Contains(h.Hd_File_Id ?? 0)).ToList();
                var headerIds = headers.Select(h => h.Hd_Id).ToList();
                var details = _mockDetails.Where(d => headerIds.Contains(d.DT_Hd_Id ?? 0)).ToList();

                // Calculate Stats
                int totalMeasurements = 0;
                int passed = 0;
                int failed = 0;

                foreach (var dt in details)
                {
                    if (!string.IsNullOrEmpty(dt.DT_Result))
                    {
                        var hd = headers.FirstOrDefault(h => h.Hd_Id == dt.DT_Hd_Id);
                        if (hd != null)
                        {
                            totalMeasurements++;
                            if (dt.DT_Result == hd.Hd_TrueDesc)
                            {
                                passed++;
                            }
                            else if (dt.DT_Result == hd.Hd_FalseDesc)
                            {
                                failed++;
                            }
                        }
                    }
                }

                summaries.Add(new YearlyKpiSummaryViewModel
                {
                    Year = year ?? 0,
                    TotalKpis = headers.Count,
                    TotalMeasurements = totalMeasurements,
                    PassedCount = passed,
                    FailedCount = failed,
                    SuccessRate = totalMeasurements > 0 ? Math.Round(((double)passed / totalMeasurements) * 100, 1) : 0
                });
            }

            return View(summaries);
        }
    }
}
