using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace CompanyKPI_Project.Models
{
    [Table("tbl_t_file_upload")]
    public class TblTFileUpload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int File_Id { get; set; }

        [StringLength(100)]
        public string File_Name { get; set; }

        [StringLength(200)]
        public string File_Type { get; set; }

        [StringLength(50)]
        public string File_Extension { get; set; }

        [StringLength(255)]
        public string File_FileContent { get; set; } // ContentType or Description

        public byte[] File_File { get; set; }

        public int? File_OfficialYear { get; set; }

        public bool File_IsDeleted { get; set; }

        [StringLength(100)]
        public string File_UploadBy { get; set; }

        public DateTime? File_UploadDate { get; set; }
    }

    [Table("tbl_t_data_company_KPI_HD")]
    public class TblTDataCompanyKpiHd
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Hd_Id { get; set; }

        [ForeignKey("FileUpload")]
        public int? Hd_File_Id { get; set; }

        [StringLength(50)]
        public string Hd_TopicNo { get; set; }

        [StringLength(50)]
        public string Hd_Condition { get; set; }

        public int? Hd_TargetValue { get; set; }

        public bool? Hd_IsTarget { get; set; }

        [StringLength(100)]
        public string Hd_TrueDesc { get; set; }

        [StringLength(100)]
        public string Hd_FalseDesc { get; set; }

        [StringLength(50)]
        public string Hd_Unit { get; set; }

        [StringLength(100)]
        public string Hd_MainPIC { get; set; }

        [StringLength(255)]
        public string Hd_RelatedPIC { get; set; }

        [StringLength(255)]
        public string Hd_DetailDescription { get; set; }

        public DateTime? Hd_UpdateDate { get; set; }

        [StringLength(100)]
        public string Hd_UpdateBy { get; set; }

        public virtual TblTFileUpload FileUpload { get; set; }
        public virtual ICollection<TblTDataCompanyKpiDt> Details { get; set; }
    }

    [Table("tbl_t_data_company_KPI_DT")]
    public class TblTDataCompanyKpiDt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DT_Id { get; set; }

        [ForeignKey("Header")]
        public int? DT_Hd_Id { get; set; }

        public int? DT_File_Id { get; set; }

        public DateTime? DT_Month { get; set; }

        [StringLength(100)]
        public string DT_Result { get; set; }

        [StringLength(100)]
        public string DT_ActualValue { get; set; }

        [StringLength(255)]
        public string DT_ProgressiveFile { get; set; }

        [StringLength(255)]
        public string DT_ActionPlanFile { get; set; }

        [StringLength(100)]
        public string DT_UpdateBy { get; set; }

        public DateTime? DT_UpdateDate { get; set; }

        public virtual TblTDataCompanyKpiHd Header { get; set; }
    }

    
}
