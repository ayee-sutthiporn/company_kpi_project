using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace CompanyKPI_Project.Models
{
    [Table("tbl_t_dept_upload")]
    public class DeptKpiUpload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Year { get; set; } // Fiscal Year
        public int Month { get; set; } // 1-12 (Calendar Month)
        
        [StringLength(50)]
        public string Department { get; set; }
        
        [StringLength(200)]
        public string FileName { get; set; }
        
        public DateTime UploadDate { get; set; }
        
        [StringLength(100)]
        public string UploadBy { get; set; }

        public byte[] FileContent { get; set; }
        
        [StringLength(200)]
        public string ContentType { get; set; }
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
