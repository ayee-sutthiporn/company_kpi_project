namespace CompanyKPI_Project.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createTableApplicationLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.tbl_t_application_log",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LogDate = c.DateTime(nullable: false),
                        Level = c.String(maxLength: 50),
                        Message = c.String(),
                        Source = c.String(maxLength: 100),
                        User = c.String(maxLength: 100),
                        Exception = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.tbl_t_application_log");
        }
    }
}
