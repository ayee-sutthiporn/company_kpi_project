using CompanyKPI_Project.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompanyKPI_Project.Repositories;

namespace CompanyKPI_Project.Controllers
{
    public class KpiController : Controller
    {
        private IKpiRepository _repository;

        public KpiController()
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

        // Helper for QA Admin Authorization
        public static bool IsQaAdmin(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            
            // Hardcoded List of Authorized Users
            var authorizedUsers = new List<string> 
            {
                "SUTTIPORN\\YeE25",       // Current User
                "QA_Manager", 
                "Admin",
                "System"
            };

            // Simple check - Case Insensitive
            return authorizedUsers.Any(u => u.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public ActionResult Index()
        {
            var files = _repository.GetAllFiles();
            return View(files);
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file, int year)
        {
            if (file != null && file.ContentLength > 0)
            {
                // Check for duplicate FY
                if (_repository.FileExistsForYear(year))
                {
                    TempData["Error"] = $"Data for FY {year} already exists.";
                    return RedirectToAction("Index");
                }

                // Create File Record
                var fileUpload = new TblTFileUpload
                {
                    // ID handled by Repo
                    File_Name = Path.GetFileName(file.FileName),
                    File_Type = file.ContentType,
                    File_Extension = Path.GetExtension(file.FileName),
                    File_OfficialYear = year,
                    File_IsDeleted = false,
                    File_UploadDate = DateTime.Now,
                    File_UploadBy = "System" // Should be from User.Identity
                };
                _repository.AddFile(fileUpload);
                int newId = fileUpload.File_Id;

                // Parse Excel
                try
                {
                    using (var workbook = new ClosedXML.Excel.XLWorkbook(file.InputStream))
                    {
                        var worksheet = workbook.Worksheet(1); // First Sheet
                        var rows = worksheet.RowsUsed().ToList();
                        
                        if(rows.Count > 1)
                        {
                            var headerRow = rows[0];
                            var departmentMap = new Dictionary<int, string>(); // Column Index -> Dept Name
                            int topicCol = -1;
                            int conditionCol = -1;
                            int targetCol = -1;
                            int unitCol = -1;

                            // Parse Header
                            foreach(var cell in headerRow.CellsUsed())
                            {
                                var val = cell.GetValue<string>().Trim();
                                if(val.Equals("Topic", StringComparison.OrdinalIgnoreCase) || val.Equals("KPI Detail", StringComparison.OrdinalIgnoreCase))
                                {
                                    topicCol = cell.Address.ColumnNumber;
                                }
                                else if (val.Equals("Condition", StringComparison.OrdinalIgnoreCase))
                                {
                                    conditionCol = cell.Address.ColumnNumber;
                                }
                                else if (val.Equals("Target", StringComparison.OrdinalIgnoreCase))
                                {
                                    targetCol = cell.Address.ColumnNumber;
                                }
                                else if (val.Equals("Unit", StringComparison.OrdinalIgnoreCase))
                                {
                                    unitCol = cell.Address.ColumnNumber;
                                }
                                else
                                {
                                    // Assume Department Name (IT, LG, PC, etc.)
                                    departmentMap.Add(cell.Address.ColumnNumber, val);
                                }
                            }

                            if(topicCol != -1)
                            {
                                // Parse Data Rows
                                foreach(var row in rows.Skip(1))
                                {
                                    var topic = row.Cell(topicCol).GetValue<string>();
                                    if(string.IsNullOrWhiteSpace(topic)) continue;

                                    var condition = (conditionCol != -1) ? row.Cell(conditionCol).GetValue<string>() : ">=";
                                    var targetValStr = (targetCol != -1) ? row.Cell(targetCol).GetValue<string>() : "100";
                                    var unit = (unitCol != -1) ? row.Cell(unitCol).GetValue<string>() : "%";
                                    
                                    // Default fallback values
                                    if(string.IsNullOrEmpty(condition)) condition = ">=";
                                    if(string.IsNullOrEmpty(unit)) unit = "%";
                                    
                                    int targetValue = 100;
                                    int.TryParse(targetValStr, out targetValue);

                                    string mainPic = "";
                                    List<string> relatedPics = new List<string>();

                                    foreach(var kvp in departmentMap)
                                    {
                                        var val = row.Cell(kvp.Key).GetValue<string>().Trim().ToUpper();
                                        if(val == "O") mainPic = kvp.Value;
                                        else if(val == "E") relatedPics.Add(kvp.Value);
                                    }

                                    // Create Header
                                    var hd = new TblTDataCompanyKpiHd
                                    {
                                        // Hd_Id handled by Repo
                                        Hd_File_Id = newId,
                                        Hd_TopicNo = topic,
                                        Hd_Condition = condition,
                                        Hd_TargetValue = targetValue,
                                        Hd_Unit = unit,
                                        Hd_MainPIC = mainPic,
                                        Hd_RelatedPIC = string.Join(", ", relatedPics),
                                        Hd_TrueDesc = "Pass", // Default
                                        Hd_FalseDesc = "Fail", // Default
                                        Hd_IsTarget = true
                                    };
                                    _repository.AddHeader(hd);
                                    
                                    // Create Dummy Details
                                    var detailsList = new List<TblTDataCompanyKpiDt>();
                                    var startDate = new DateTime(year, 4, 1);
                                    for(int i=0; i<12; i++)
                                    {
                                        detailsList.Add(new TblTDataCompanyKpiDt
                                        {
                                            // DT_Id handled by Repo
                                            DT_Hd_Id = hd.Hd_Id,
                                            DT_File_Id = newId,
                                            DT_Month = startDate.AddMonths(i),
                                            DT_Result = ""
                                        });
                                    }
                                    _repository.AddDetails(detailsList);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle Error (Log it)
                    System.Diagnostics.Debug.WriteLine("Upload Error: " + ex.Message);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteFile(int id)
        {
            _repository.DeleteFile(id);
            return RedirectToAction("Index");
        }

        public ActionResult AdminDetail(int id)
        {
            var file = _repository.GetFileById(id);
            if (file == null) return HttpNotFound();

            ViewBag.File = file;
            ViewBag.Departments = new List<string> { "QA", "HR", "Production", "IT", "Sales" }; // Standardized List
            var headers = _repository.GetHeadersByFileId(id).ToList();
            
            // Link Details manually (Repository might return disconnected entities in Mock, or proxy in EF)
            // But for View display, we need Details populated.
            // EF w/ Lazy Loading handles it, but Mock doesn't automatically link unless we explicitly do it in GetHeaders or here.
            
            foreach(var h in headers)
            {
                h.Details = _repository.GetDetailsByHeaderId(h.Hd_Id).ToList();
            }

            return View(headers);
        }

        public ActionResult UserResult(int id)
        {
            // Direct access (fallback if needed)
            var file = _repository.GetFileById(id);
            if (file == null) return HttpNotFound();

            ViewBag.File = file;
            var headers = _repository.GetHeadersByFileId(id).ToList();
             foreach(var h in headers)
            {
                h.Details = _repository.GetDetailsByHeaderId(h.Hd_Id).ToList();
            }
            return View(headers);
        }

        public ActionResult UserDashboard(int? year, string dept)
        {
            // Default to current year
            int targetYear = year ?? DateTime.Now.Year;
            
            // Find file
            var file = _repository.GetFileByYear(targetYear);
            // Fallback logic
            if (file == null) file = _repository.GetAllFiles().FirstOrDefault();

            if (file == null) return View("UserResult", new List<TblTDataCompanyKpiHd>());

            ViewBag.File = file;
            ViewBag.CurrentYear = targetYear;

            var allHeaders = _repository.GetHeadersByFileId(file.File_Id).ToList();

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
                h.Details = _repository.GetDetailsByHeaderId(h.Hd_Id).ToList();
            }

            return View("UserResult", headers);
        }

        [HttpPost]
        public ActionResult UpdateDetail(int dtId, string resultValue, HttpPostedFileBase fileProgressive, HttpPostedFileBase fileActionPlan)
        {
            var dt = _repository.GetDetailById(dtId);
            if (dt != null)
            {
                // Validation: Prevent Future Updates
                if (dt.DT_Month > DateTime.Now)
                {
                     return Json(new { success = false, message = "Cannot update future months." });
                }

                var hd = _repository.GetHeaderById(dt.DT_Hd_Id ?? 0);
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
                dt.DT_UpdateBy = "Mock User"; // Or Identity
                
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

                _repository.UpdateDetail(dt);

                return Json(new { success = true, result = finalResult, isPass = isPass });
            }
            return Json(new { success = false, message = "Item not found" });
        }

        public ActionResult DownloadResultFile(int dtId, string type)
        {
            var dt = _repository.GetDetailById(dtId);
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
            var dt = _repository.GetDetailById(dtId);
            if (dt != null)
            {
                var uploadDir = Server.MapPath("~/App_Data/KpiResultFiles");
                string fileName = (type == "prog") ? dt.DT_ProgressiveFile : dt.DT_ActionPlanFile;
                string prefix = (type == "prog") ? "P" : "A";

                // Nullify
                if (type == "prog") dt.DT_ProgressiveFile = null;
                else dt.DT_ActionPlanFile = null;

                _repository.UpdateDetail(dt);

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
                hd = _repository.GetHeaderById(hdId.Value);
                if (hd != null)
                {
                    hd.Hd_TopicNo = topic;
                    hd.Hd_Condition = condition;
                    hd.Hd_TargetValue = target;
                    hd.Hd_Unit = unit;
                    hd.Hd_MainPIC = pic;
                    hd.Hd_TrueDesc = trueDesc;
                    hd.Hd_FalseDesc = falseDesc;
                    _repository.UpdateHeader(hd);
                }
            }
            else
            {
                // Add
                // Hd_Id handled by Repo
                hd = new TblTDataCompanyKpiHd
                {
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
                _repository.AddHeader(hd);

                // Details
                var file = _repository.GetFileById(fileId);
                var year = file?.File_OfficialYear ?? 2025;
                var startDate = new DateTime(year, 4, 1);
                
                var detailsList = new List<TblTDataCompanyKpiDt>();
                for(int i=0; i<12; i++)
                {
                    detailsList.Add(new TblTDataCompanyKpiDt
                    {
                        // DT_Id handled by Repo
                        DT_Hd_Id = hd.Hd_Id,
                        DT_File_Id = fileId,
                        DT_Month = startDate.AddMonths(i),
                        DT_Result = ""
                    });
                }
                _repository.AddDetails(detailsList);
            }
            return RedirectToAction("AdminDetail", new { id = fileId });
        }

        [HttpPost]
        public ActionResult DeleteItem(int id)
        {
            // Repo handles cascading logic now
            _repository.DeleteHeader(id);
            return Json(new { success = true });
        }

        public ActionResult Export(int id)
        {
            var file = _repository.GetFileById(id);
            if (file == null) return HttpNotFound();

            var headers = _repository.GetHeadersByFileId(id).ToList();
            var details = _repository.GetDetailsByFileId(id).ToList();

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
            var allFiles = _repository.GetAllFiles();
            var years = allFiles.Select(f => f.File_OfficialYear).Distinct().OrderByDescending(y => y).ToList();

            foreach (var year in years)
            {
                // Get all files for this year
                var fileIds = allFiles.Where(f => f.File_OfficialYear == year).Select(f => f.File_Id).ToList();
                
                // For Summary, we might need bulk retrieval. Using existing methods for now.
                // In optimization, Repo should have GetDetailsByYear or similar.
                
                var headers = new List<TblTDataCompanyKpiHd>();
                var details = new List<TblTDataCompanyKpiDt>();

                foreach(var fid in fileIds) {
                    headers.AddRange(_repository.GetHeadersByFileId(fid));
                    details.AddRange(_repository.GetDetailsByFileId(fid)); // Optimized Repo method?
                }
                var headerIds = headers.Select(h => h.Hd_Id).ToList();

                // Calculate Stats
                int totalMeasurements = 0;
                int passed = 0;
                int failed = 0;

                foreach (var dt in details)
                {
                    bool hasResult = !string.IsNullOrEmpty(dt.DT_Result);
                    // Assuming current month is inclusive for due date
                    bool isDue = dt.DT_Month <= DateTime.Now; 

                    if (isDue || hasResult)
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

                // Calculate Stats per Department
                var deptStats = new List<DepartmentStatViewModel>();
                var deptGroups = details.GroupBy(d => {
                     var h = headers.FirstOrDefault(head => head.Hd_Id == d.DT_Hd_Id);
                     return h != null ? h.Hd_MainPIC : "Unknown";
                });

                foreach(var grp in deptGroups)
                {
                    var dStats = new DepartmentStatViewModel { DepartmentName = grp.Key, Total = 0, PassedCount = 0, FailedCount = 0, PendingCount = 0 };
                    foreach(var dt in grp)
                    {
                        bool hasResult = !string.IsNullOrEmpty(dt.DT_Result);
                        bool isDue = dt.DT_Month <= DateTime.Now;

                        if(isDue || hasResult)
                        {
                            var hd = headers.FirstOrDefault(h => h.Hd_Id == dt.DT_Hd_Id);
                            if(hd != null)
                            {
                                dStats.Total++;
                                if(dt.DT_Result == hd.Hd_TrueDesc) dStats.PassedCount++;
                                else if(dt.DT_Result == hd.Hd_FalseDesc) dStats.FailedCount++;
                                else dStats.PendingCount++;
                            }
                        }
                    }
                    // Pending calculation based on total months - measured? Or just from Result?
                    // Logic above only counts measured items. Pending/Other in main logic included unmeasured?
                    // Main logic: TotalMeasurements = total measured.
                    // Pending in View = TotalMeasurements - Pass - Fail.
                    // Let's stick to "Measured" stats for now.
                    
                    dStats.SuccessRate = dStats.Total > 0 ? Math.Round(((double)dStats.PassedCount / dStats.Total) * 100, 1) : 0;
                    deptStats.Add(dStats);
                }

                summaries.Add(new YearlyKpiSummaryViewModel
                {
                    Year = year ?? 0,
                    TotalKpis = headers.Count,
                    TotalMeasurements = totalMeasurements,
                    PassedCount = passed,
                    FailedCount = failed,
                    SuccessRate = totalMeasurements > 0 ? Math.Round(((double)passed / totalMeasurements) * 100, 1) : 0,
                    DepartmentStats = deptStats.OrderBy(d => d.DepartmentName).ToList()
                });
            }

            return View(summaries);
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
