using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CompanyKPI_Project.Models;
using CompanyKPI_Project.Repositories;

namespace CompanyKPI_Project.Controllers
{
    public class NotificationController : Controller
    {
        private IKpiRepository _repository;

        public NotificationController()
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

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendNewMonthNotifications()
        {
            var logs = new List<string>();
            try
            {
                // Logic: Notify all distinct Dept Heads (PICs)
                var allFiles = _repository.GetAllFiles();
                // To get all Headers, we need to iterate files. 
                // Optimized: Repo should have GetAllActiveHeaders or similar.
                // For now, iterate recent files or just use 2025.
                
                var targetYear = DateTime.Now.Year;
                var file = _repository.GetFileByYear(targetYear);
                
                // Fallback to recent
                if(file == null) file = allFiles.FirstOrDefault();

                var activePics = new List<string>();
                if(file != null)
                {
                    activePics = _repository.GetHeadersByFileId(file.File_Id)
                                    .Select(h => h.Hd_MainPIC)
                                    .Distinct()
                                    .Where(p => !string.IsNullOrEmpty(p))
                                    .ToList();
                }

                foreach (var pic in activePics)
                {
                    // Mock Email
                    var email = $"{pic.ToLower().Replace(" ", ".")}@company.com";
                    var subject = $"[KPI] New Month Started: {DateTime.Now:MMM-yyyy}";
                    var body = $"Dear {pic},<br/>The new month {DateTime.Now:MMMM yyyy} has started. Please prepare your KPI data.";
                    
                    logs.Add($"Sent 'New Month' Email to: {email} | Subject: {subject}");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = true, logs = logs });
        }

        [HttpPost]
        public ActionResult SendReminders(bool force = false)
        {
            var logs = new List<string>();
            try
            {
                // Reminder Logic: 5 days before end of month
                // Calculate "End of Month"
                var now = DateTime.Now;
                var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var daysLeft = daysInMonth - now.Day;

                bool isTimeToSend = daysLeft <= 5;

                if (!isTimeToSend && !force)
                {
                    return Json(new { success = false, message = $"Not time yet. {daysLeft} days left in month. (Need <= 5)" });
                }

                // Find Empty Results for Current Month
                var targetMonth = new DateTime(now.Year, now.Month, 1);
                
                // Need to find which File corresponds to this year?
                // Fiscal Year logic again. If Now is Dec 2025, FY is 2025.
                // If Now is Jan 2026, FY is 2025.
                int fy = (now.Month >= 4) ? now.Year : now.Year - 1;
                
                var file = _repository.GetFileByYear(fy);
                var missingDetails = new List<TblTDataCompanyKpiDt>();
                
                if(file != null)
                {
                    // This is inefficient (Get All Details then filter). But okay for now.
                    // Repo should support GetDetailsByMonth(year, month)
                    var allDetails = _repository.GetDetailsByFileId(file.File_Id);
                    
                    missingDetails = allDetails
                                        .Where(d => d.DT_Month?.Year == targetMonth.Year 
                                                 && d.DT_Month?.Month == targetMonth.Month 
                                                 && string.IsNullOrEmpty(d.DT_Result))
                                        .ToList();
                }

                var deptToNotify = new HashSet<string>();

                foreach(var dt in missingDetails)
                {
                    var hd = _repository.GetHeaderById(dt.DT_Hd_Id ?? 0);
                    if(hd != null && !string.IsNullOrEmpty(hd.Hd_MainPIC))
                    {
                        deptToNotify.Add(hd.Hd_MainPIC);
                    }
                }

                foreach (var dept in deptToNotify)
                {
                    var email = $"{dept.ToLower().Replace(" ", ".")}@company.com";
                    var subject = $"[KPI] Reminder: Outstanding KPI Results for {now:MMM-yyyy}";
                    var body = $"Dear {dept},<br/>There are only {daysLeft} days left in the month. Please submit your KPI results.";

                    logs.Add($"Sent 'Reminder' Email to: {email} | Subject: {subject}");
                }

                if (!deptToNotify.Any())
                {
                     logs.Add("No missing KPI results found for this month.");
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

            return Json(new { success = true, logs = logs });
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
