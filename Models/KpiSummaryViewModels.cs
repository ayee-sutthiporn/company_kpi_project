using System.Collections.Generic;

namespace CompanyKPI_Project.Models
{
    public class YearlyKpiSummaryViewModel
    {
        public int Year { get; set; }
        public int TotalKpis { get; set; }
        public int TotalMeasurements { get; set; } // Total monthly slots checked
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public double SuccessRate { get; set; }
    }
}
