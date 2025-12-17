using CompanyKPI_Project.Models;
using System.Collections.Generic;

namespace CompanyKPI_Project.Repositories
{
    public interface IKpiRepository
    {
        // Files
        IEnumerable<TblTFileUpload> GetAllFiles();
        TblTFileUpload GetFileById(int id);
        TblTFileUpload GetFileByYear(int year);
        void AddFile(TblTFileUpload file);
        void DeleteFile(int id);
        bool FileExistsForYear(int year);

        // Headers (KPI Items)
        IEnumerable<TblTDataCompanyKpiHd> GetHeadersByFileId(int fileId);
        TblTDataCompanyKpiHd GetHeaderById(int id);
        void AddHeader(TblTDataCompanyKpiHd header);
        void UpdateHeader(TblTDataCompanyKpiHd header);
        void DeleteHeader(int id);

        // Details (Monthly Data)
        IEnumerable<TblTDataCompanyKpiDt> GetDetailsByHeaderId(int headerId);
        IEnumerable<TblTDataCompanyKpiDt> GetDetailsByFileId(int fileId); // For Export/Bulk ops
        TblTDataCompanyKpiDt GetDetailById(int id);
        void AddDetails(IEnumerable<TblTDataCompanyKpiDt> details); // Batch add
        void UpdateDetail(TblTDataCompanyKpiDt detail);
        void DeleteDetailsByHeaderId(int headerId);
        void DeleteDetailsByFileId(int fileId);

        // Department Uploads
        IEnumerable<DeptKpiUpload> GetDeptUploads(int year, string dept); // Filtered
        DeptKpiUpload GetDeptUploadById(int id);
        void AddDeptUpload(DeptKpiUpload upload);
        void DeleteDeptUpload(int id);
        void ClearDeptUpload(int year, int month, string dept);
        
        // Logging
        void AddLog(ApplicationLog log);

        void Dispose();
    }
}
