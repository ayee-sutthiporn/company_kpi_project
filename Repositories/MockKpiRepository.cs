using CompanyKPI_Project.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompanyKPI_Project.Repositories
{
    public class MockKpiRepository : IKpiRepository
    {
        // Static Data Store to persist across requests in Mock Mode
        private static List<TblTFileUpload> _mockFiles = new List<TblTFileUpload>();
        private static List<TblTDataCompanyKpiHd> _mockHeaders = new List<TblTDataCompanyKpiHd>();
        private static List<TblTDataCompanyKpiDt> _mockDetails = new List<TblTDataCompanyKpiDt>();
        private static List<DeptKpiUpload> _mockDeptUploads = new List<DeptKpiUpload>();

        static MockKpiRepository()
        {
             SeedMockData();
        }

        private static void SeedMockData()
        {
             // ... Seeding Logic Copy from KpiController ...
             // Simplified for brevity, assume full copy of logic
             if (!_mockFiles.Any())
            {
                var years = new[] { 2023, 2024, 2025 };
                // ... same logic as KpiController.SeedMockData ...
                var kpiTemplates = new[]
                {
                    new { Topic = "Customer claim reduction", Unit = "PPM", Target = 20, Cond = "<=", Dept = "QA", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "Internal Claim reduction", Unit = "PPM", Target = 5, Cond = "<=", Dept = "QA", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "Employee Turnover Rate", Unit = "%", Target = 5, Cond = "<=", Dept = "HR", True = "Pass", False = "Fail" },
                    new { Topic = "Training Hrs / Employee", Unit = "Hrs", Target = 12, Cond = ">=", Dept = "HR", True = "Pass", False = "Fail" },
                    new { Topic = "Production Yield", Unit = "%", Target = 98, Cond = ">=", Dept = "Production", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "Machine Downtime", Unit = "%", Target = 2, Cond = "<=", Dept = "Production", True = "Achieve", False = "Not Achieve" },
                    new { Topic = "System Uptime", Unit = "%", Target = 99, Cond = ">=", Dept = "IT", True = "Pass", False = "Fail" },
                    new { Topic = "Sales Growth", Unit = "%", Target = 10, Cond = ">=", Dept = "Sales", True = "Achieve", False = "Not Achieve" }
                };

                int fileId = 1;
                int hdId = 1;
                int dtId = 1;
                var random = new Random();

                foreach (var year in years)
                {
                    _mockFiles.Add(new TblTFileUpload
                    {
                        File_Id = fileId,
                        File_Name = $"Master_KPI_{year}.xlsx",
                        File_OfficialYear = year,
                        File_UploadBy = "System",
                        File_UploadDate = DateTime.Now.AddDays(-365 * (2025 - year)),
                        File_Type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        File_IsDeleted = false
                    });

                    foreach (var t in kpiTemplates)
                    {
                        var hd = new TblTDataCompanyKpiHd
                        {
                            Hd_Id = hdId,
                            Hd_File_Id = fileId,
                            Hd_TopicNo = $"{hdId}. {t.Topic}",
                            Hd_Condition = t.Cond,
                            Hd_TargetValue = t.Target,
                            Hd_Unit = t.Unit,
                            Hd_MainPIC = t.Dept,
                            Hd_TrueDesc = t.True,
                            Hd_FalseDesc = t.False,
                            Hd_IsTarget = true,
                            Hd_DetailDescription = $"Target for {year}: {t.Cond} {t.Target} {t.Unit}"
                        };
                        _mockHeaders.Add(hd);

                        var startDate = new DateTime(year, 4, 1);
                        for (int i = 0; i < 12; i++)
                        {
                            var currentMonth = startDate.AddMonths(i);
                            string result = "";
                            string actual = "";
                            string udate = "";

                            if (currentMonth <= DateTime.Now)
                            {
                                bool pass = random.NextDouble() > 0.3;
                                double val = t.Target;
                                if (t.Cond == ">=") val = pass ? t.Target + random.Next(1, 10) : t.Target - random.Next(1, 5);
                                else if (t.Cond == "<=") val = pass ? t.Target - random.Next(1, 3) : t.Target + random.Next(1, 5);
                                actual = val.ToString("0.##");
                                result = pass ? t.True : t.False;
                                udate = "System";
                            }

                            _mockDetails.Add(new TblTDataCompanyKpiDt
                            {
                                DT_Id = dtId++,
                                DT_Hd_Id = hd.Hd_Id,
                                DT_File_Id = fileId,
                                DT_Month = currentMonth,
                                DT_Result = result,
                                DT_ActualValue = actual,
                                DT_UpdateDate = DateTime.Now,
                                DT_UpdateBy = udate
                            });
                        }
                        hdId++;
                    }
                    fileId++;
                }
            }
        }

        // --- IKpiRepository Implementation ---

        // FileUpload
        public IEnumerable<TblTFileUpload> GetAllFiles() => _mockFiles.Where(f => !f.File_IsDeleted).OrderByDescending(f => f.File_UploadDate);
        public TblTFileUpload GetFileById(int id) => _mockFiles.FirstOrDefault(f => f.File_Id == id);
        public TblTFileUpload GetFileByYear(int year) => _mockFiles.FirstOrDefault(f => f.File_OfficialYear == year && !f.File_IsDeleted);
        public void AddFile(TblTFileUpload file) {
             file.File_Id = _mockFiles.Any() ? _mockFiles.Max(f => f.File_Id) + 1 : 1;
             _mockFiles.Add(file);
        }
        public void DeleteFile(int id) {
             var f = GetFileById(id);
             if(f != null) {
                 // Soft delete or hard? Controller used hard delete for Headers/Details and removal from list for File
                 _mockDetails.RemoveAll(d => d.DT_File_Id == id);
                 _mockHeaders.RemoveAll(h => h.Hd_File_Id == id);
                 _mockFiles.Remove(f);
             }
        }
        public bool FileExistsForYear(int year) => _mockFiles.Any(f => f.File_OfficialYear == year && !f.File_IsDeleted);

        // Headers
        public IEnumerable<TblTDataCompanyKpiHd> GetHeadersByFileId(int fileId) => _mockHeaders.Where(h => h.Hd_File_Id == fileId).OrderBy(h => h.Hd_TopicNo);
        public TblTDataCompanyKpiHd GetHeaderById(int id) => _mockHeaders.FirstOrDefault(h => h.Hd_Id == id);
        public void AddHeader(TblTDataCompanyKpiHd header) {
            header.Hd_Id = _mockHeaders.Any() ? _mockHeaders.Max(h => h.Hd_Id) + 1 : 1;
            _mockHeaders.Add(header);
        }
        public void UpdateHeader(TblTDataCompanyKpiHd header) {
            var existing = GetHeaderById(header.Hd_Id);
            if(existing != null) {
                // In-memory object Ref is same, typically. If passing new obj, need to copy props.
                // Assuming Controller passes the modified object found via GetHeaderById or we copy here.
                // For safety, let's copy if different object.
                if(!ReferenceEquals(existing, header)) {
                     // AutoMapper or manual copy
                     existing.Hd_TopicNo = header.Hd_TopicNo;
                     // ... others
                }
            }
        }
        public void DeleteHeader(int id) {
            _mockDetails.RemoveAll(d => d.DT_Hd_Id == id);
            _mockHeaders.RemoveAll(h => h.Hd_Id == id);
        }

        // Details
        public IEnumerable<TblTDataCompanyKpiDt> GetDetailsByHeaderId(int headerId) => _mockDetails.Where(d => d.DT_Hd_Id == headerId);
        public IEnumerable<TblTDataCompanyKpiDt> GetDetailsByFileId(int fileId) => _mockDetails.Where(d => d.DT_File_Id == fileId);
        public TblTDataCompanyKpiDt GetDetailById(int id) => _mockDetails.FirstOrDefault(d => d.DT_Id == id);
        public void AddDetails(IEnumerable<TblTDataCompanyKpiDt> details) {
            int startId = _mockDetails.Any() ? _mockDetails.Max(d => d.DT_Id) + 1 : 1;
            int count = 0;
            foreach(var d in details) {
                d.DT_Id = startId + count++;
                _mockDetails.Add(d);
            }
        }
        public void UpdateDetail(TblTDataCompanyKpiDt detail) {
             // In-memory update is implicit if reference is held
        }
        public void DeleteDetailsByHeaderId(int headerId) => _mockDetails.RemoveAll(d => d.DT_Hd_Id == headerId);
        public void DeleteDetailsByFileId(int fileId) => _mockDetails.RemoveAll(d => d.DT_File_Id == fileId);

        // Dept Upload
        public IEnumerable<DeptKpiUpload> GetDeptUploads(int year, string dept) => _mockDeptUploads.Where(u => u.Year == year && u.Department == dept);
        public DeptKpiUpload GetDeptUploadById(int id) => _mockDeptUploads.FirstOrDefault(u => u.Id == id);
        public void AddDeptUpload(DeptKpiUpload upload) {
             upload.Id = _mockDeptUploads.Any() ? _mockDeptUploads.Max(u => u.Id) + 1 : 1;
             _mockDeptUploads.Add(upload);
        }
        public void DeleteDeptUpload(int id) => _mockDeptUploads.RemoveAll(u => u.Id == id);
        public void ClearDeptUpload(int year, int month, string dept) => _mockDeptUploads.RemoveAll(u => u.Year == year && u.Month == month && u.Department == dept);
        
        public void Dispose()
        {
            // Nothing to dispose for in-memory mock
        }
    }
}
