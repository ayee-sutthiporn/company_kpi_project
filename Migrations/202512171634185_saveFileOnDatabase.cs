namespace CompanyKPI_Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class saveFileOnDatabase : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tbl_t_dept_upload", "FileContent", c => c.Binary());
            AddColumn("dbo.tbl_t_dept_upload", "ContentType", c => c.String(maxLength: 200));
            AddColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ProgressiveFileContent", c => c.Binary());
            AddColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ActionPlanFileContent", c => c.Binary());
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ActionPlanFileContent");
            DropColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ProgressiveFileContent");
            DropColumn("dbo.tbl_t_dept_upload", "ContentType");
            DropColumn("dbo.tbl_t_dept_upload", "FileContent");
        }
    }
}
