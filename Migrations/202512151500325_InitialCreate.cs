namespace CompanyKPI_Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tbl_t_file_upload",
                c => new
                    {
                        File_Id = c.Int(nullable: false, identity: true),
                        File_Name = c.String(maxLength: 100),
                        File_Type = c.String(maxLength: 50),
                        File_Extension = c.String(maxLength: 50),
                        File_FileContent = c.String(maxLength: 100),
                        File_File = c.Binary(),
                        File_OfficialYear = c.Int(),
                        File_IsDeleted = c.Boolean(nullable: false),
                        File_UploadBy = c.String(maxLength: 100),
                        File_UploadDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.File_Id);
            
            CreateTable(
                "dbo.tbl_t_data_company_KPI_DT",
                c => new
                    {
                        DT_Id = c.Int(nullable: false, identity: true),
                        DT_Hd_Id = c.Int(),
                        DT_File_Id = c.Int(),
                        DT_Month = c.DateTime(),
                        DT_Result = c.String(maxLength: 100),
                        DT_UpdateBy = c.String(maxLength: 100),
                        DT_UpdateDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.DT_Id)
                .ForeignKey("dbo.tbl_t_data_company_KPI_HD", t => t.DT_Hd_Id)
                .Index(t => t.DT_Hd_Id);
            
            CreateTable(
                "dbo.tbl_t_data_company_KPI_HD",
                c => new
                    {
                        Hd_Id = c.Int(nullable: false, identity: true),
                        Hd_File_Id = c.Int(),
                        Hd_TopicNo = c.String(maxLength: 50),
                        Hd_Condition = c.String(maxLength: 50),
                        Hd_TargetValue = c.Int(),
                        Hd_IsTarget = c.Boolean(),
                        Hd_TrueDesc = c.String(maxLength: 100),
                        Hd_FalseDesc = c.String(maxLength: 100),
                        Hd_Unit = c.String(maxLength: 50),
                        Hd_MainPIC = c.String(maxLength: 100),
                        Hd_DetailDescription = c.String(maxLength: 255),
                        Hd_UpdateDate = c.DateTime(),
                        Hd_UpdateBy = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Hd_Id)
                .ForeignKey("dbo.tbl_t_file_upload", t => t.Hd_File_Id)
                .Index(t => t.Hd_File_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tbl_t_data_company_KPI_DT", "DT_Hd_Id", "dbo.tbl_t_data_company_KPI_HD");
            DropForeignKey("dbo.tbl_t_data_company_KPI_HD", "Hd_File_Id", "dbo.tbl_t_file_upload");
            DropIndex("dbo.tbl_t_data_company_KPI_HD", new[] { "Hd_File_Id" });
            DropIndex("dbo.tbl_t_data_company_KPI_DT", new[] { "DT_Hd_Id" });
            DropTable("dbo.tbl_t_data_company_KPI_HD");
            DropTable("dbo.tbl_t_data_company_KPI_DT");
            DropTable("dbo.tbl_t_file_upload");
        }
    }
}
