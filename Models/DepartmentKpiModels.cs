using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompanyKPI_Project.Models
{
    public class DeptKpiUpload
    {
        public int Id { get; set; }
        public int Year { get; set; } // Fiscal Year
        public int Month { get; set; } // 1-12 (Calendar Month)
        public string Department { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
        public string UploadBy { get; set; }
    }

    public class DepartmentKpiViewModel
    {
        public int Year { get; set; }
        public string Department { get; set; }
        public List<DeptMonthItem> Months { get; set; }
    }

    public class DeptMonthItem
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public DeptKpiUpload Upload { get; set; }
    }
}
