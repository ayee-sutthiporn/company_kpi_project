namespace CompanyKPI_Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class update : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tbl_t_dept_upload",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Year = c.Int(nullable: false),
                        Month = c.Int(nullable: false),
                        Department = c.String(maxLength: 50),
                        FileName = c.String(maxLength: 200),
                        UploadDate = c.DateTime(nullable: false),
                        UploadBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ActualValue", c => c.String(maxLength: 100));
            AddColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ProgressiveFile", c => c.String(maxLength: 255));
            AddColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ActionPlanFile", c => c.String(maxLength: 255));
            AddColumn("dbo.tbl_t_data_company_KPI_HD", "Hd_RelatedPIC", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tbl_t_data_company_KPI_HD", "Hd_RelatedPIC");
            DropColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ActionPlanFile");
            DropColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ProgressiveFile");
            DropColumn("dbo.tbl_t_data_company_KPI_DT", "DT_ActualValue");
            DropTable("dbo.tbl_t_dept_upload");
        }
    }
}
