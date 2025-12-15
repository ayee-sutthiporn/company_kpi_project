namespace CompanyKPI_Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class upate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tbl_t_file_upload", "File_Type", c => c.String(maxLength: 200));
            AlterColumn("dbo.tbl_t_file_upload", "File_FileContent", c => c.String(maxLength: 255));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.tbl_t_file_upload", "File_FileContent", c => c.String(maxLength: 100));
            AlterColumn("dbo.tbl_t_file_upload", "File_Type", c => c.String(maxLength: 50));
        }
    }
}
