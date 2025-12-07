using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SafeTalkApp.Interfaces
{
    public interface IFileStorageService
    {
        string SavePaymentProof(HttpPostedFileBase file);
        FileSaveResult SaveProfilePicture(HttpPostedFileBase file);
        string MapPath(string relativePath);
        bool FileExists(string path);
        string ReadAllText(string path);
        Task<byte[]> ReadAllBytesAsync(string path);
        void CreateDirectory(string path);
        void WriteAllText(string path, string content);
        string CombinePath(params string[] paths);
    }
}