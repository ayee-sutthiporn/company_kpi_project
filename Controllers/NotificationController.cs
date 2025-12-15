using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CompanyKPI_Project.Models;

namespace CompanyKPI_Project.Controllers
{
    public class NotificationController : Controller
    {
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
                var activePics = KpiController._mockHeaders
                                    .Select(h => h.Hd_MainPIC)
                                    .Distinct()
                                    .Where(p => !string.IsNullOrEmpty(p))
                                    .ToList();

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
                // Note: In Mock Data, DT_Month is usually 1st of month.
                var targetMonth = new DateTime(now.Year, now.Month, 1);
                
                // Get Details for this month that are empty
                var missingDetails = KpiController._mockDetails
                                        .Where(d => d.DT_Month?.Year == targetMonth.Year 
                                                 && d.DT_Month?.Month == targetMonth.Month 
                                                 && string.IsNullOrEmpty(d.DT_Result))
                                        .ToList();

                var deptToNotify = new HashSet<string>();

                foreach(var dt in missingDetails)
                {
                    var hd = KpiController._mockHeaders.FirstOrDefault(h => h.Hd_Id == dt.DT_Hd_Id);
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
    }
}
