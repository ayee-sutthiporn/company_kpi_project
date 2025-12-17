using CompanyKPI_Project.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace CompanyKPI_Project.Data
{
    public class CompanyKpiDbContext : DbContext
    {
        public CompanyKpiDbContext() : base("name=CompanyKpiDbContext")
        {
        }

        public DbSet<TblTFileUpload> FileUploads { get; set; }
        public DbSet<TblTDataCompanyKpiHd> KpiHeaders { get; set; }
        public DbSet<TblTDataCompanyKpiDt> KpiDetails { get; set; }
        public DbSet<DeptKpiUpload> DeptUploads { get; set; }
        public DbSet<ApplicationLog> ApplicationLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}