using CompanyKPI_Project.Data;
using CompanyKPI_Project.Models;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace CompanyKPI_Project.Repositories
{
    public class EfKpiRepository : IKpiRepository
    {
        private CompanyKpiDbContext _context;

        public EfKpiRepository()
        {
            _context = new CompanyKpiDbContext();
        }

        // Files
        public IEnumerable<TblTFileUpload> GetAllFiles()
        {
            return _context.FileUploads.Where(f => !f.File_IsDeleted).OrderByDescending(f => f.File_UploadDate).ToList();
        }

        public TblTFileUpload GetFileById(int id)
        {
            return _context.FileUploads.FirstOrDefault(f => f.File_Id == id);
        }

        public TblTFileUpload GetFileByYear(int year)
        {
            return _context.FileUploads.FirstOrDefault(f => f.File_OfficialYear == year && !f.File_IsDeleted);
        }

        public void AddFile(TblTFileUpload file)
        {
            _context.FileUploads.Add(file);
            _context.SaveChanges();
        }

        public void DeleteFile(int id)
        {
            var file = _context.FileUploads.Find(id);
            if (file != null)
            {
                // Cascade delete details and headers manually if FK strictness/logic requires
                // But Entity Framework cascading might handle it if configured. 
                // Repository logic usually explicit
                DeleteDetailsByFileId(id);
                var headers = _context.KpiHeaders.Where(h => h.Hd_File_Id == id).ToList();
                _context.KpiHeaders.RemoveRange(headers);
                
                _context.FileUploads.Remove(file);
                _context.SaveChanges();
            }
        }

        public bool FileExistsForYear(int year)
        {
            return _context.FileUploads.Any(f => f.File_OfficialYear == year && !f.File_IsDeleted);
        }

        // Headers
        public IEnumerable<TblTDataCompanyKpiHd> GetHeadersByFileId(int fileId)
        {
            return _context.KpiHeaders.Where(h => h.Hd_File_Id == fileId).OrderBy(h => h.Hd_TopicNo).ToList();
        }

        public TblTDataCompanyKpiHd GetHeaderById(int id)
        {
            return _context.KpiHeaders.FirstOrDefault(h => h.Hd_Id == id);
        }

        public void AddHeader(TblTDataCompanyKpiHd header)
        {
            _context.KpiHeaders.Add(header);
            _context.SaveChanges();
        }

        public void UpdateHeader(TblTDataCompanyKpiHd header)
        {
             _context.Entry(header).State = EntityState.Modified;
             _context.SaveChanges();
        }

        public void DeleteHeader(int id)
        {
            DeleteDetailsByHeaderId(id);
            var header = _context.KpiHeaders.Find(id);
            if (header != null)
            {
                _context.KpiHeaders.Remove(header);
                _context.SaveChanges();
            }
        }

        // Details
        public IEnumerable<TblTDataCompanyKpiDt> GetDetailsByHeaderId(int headerId)
        {
            return _context.KpiDetails.Where(d => d.DT_Hd_Id == headerId).ToList();
        }

        public IEnumerable<TblTDataCompanyKpiDt> GetDetailsByFileId(int fileId)
        {
            return _context.KpiDetails.Where(d => d.DT_File_Id == fileId).ToList();
        }

        public TblTDataCompanyKpiDt GetDetailById(int id)
        {
            return _context.KpiDetails.Find(id);
        }

        public void AddDetails(IEnumerable<TblTDataCompanyKpiDt> details)
        {
            _context.KpiDetails.AddRange(details);
            _context.SaveChanges();
        }

        public void UpdateDetail(TblTDataCompanyKpiDt detail)
        {
            _context.Entry(detail).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteDetailsByHeaderId(int headerId)
        {
            var details = _context.KpiDetails.Where(d => d.DT_Hd_Id == headerId);
            _context.KpiDetails.RemoveRange(details);
            _context.SaveChanges();
        }

        public void DeleteDetailsByFileId(int fileId)
        {
            var details = _context.KpiDetails.Where(d => d.DT_File_Id == fileId);
            _context.KpiDetails.RemoveRange(details);
            _context.SaveChanges();
        }

        // Dept Upload
        public IEnumerable<DeptKpiUpload> GetDeptUploads(int year, string dept)
        {
            return _context.DeptUploads.Where(u => u.Year == year && u.Department == dept).ToList();
        }

        public DeptKpiUpload GetDeptUploadById(int id)
        {
            return _context.DeptUploads.Find(id);
        }

        public void AddDeptUpload(DeptKpiUpload upload)
        {
            _context.DeptUploads.Add(upload);
            _context.SaveChanges();
        }

        public void DeleteDeptUpload(int id)
        {
            var upload = _context.DeptUploads.Find(id);
            if (upload != null)
            {
                _context.DeptUploads.Remove(upload);
                _context.SaveChanges();
            }
        }

        public void ClearDeptUpload(int year, int month, string dept)
        {
            var uploads = _context.DeptUploads.Where(u => u.Year == year && u.Month == month && u.Department == dept);
            _context.DeptUploads.RemoveRange(uploads);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            if (_context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
    }
}
