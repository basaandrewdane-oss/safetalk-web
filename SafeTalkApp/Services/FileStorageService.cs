using SafeTalkApp.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SafeTalkApp.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _rootPath;
        private readonly string _profileRoot;

        // Allowed file types
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp" };

        public FileStorageService()
        {
            _rootPath = HttpContext.Current.Server.MapPath("~/Uploads/Payments/");
            // Save profile pictures OUTSIDE the solution
            _profileRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Uploads", "ProfilePictures");
        }

        public string SavePaymentProof(HttpPostedFileBase file)
        {
            var ext = Path.GetExtension(file.FileName);
            var name = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(_rootPath, name);

            if (!Directory.Exists(_rootPath))
                Directory.CreateDirectory(_rootPath);

            file.SaveAs(path);

            return "/Uploads/Payments/" + name;
        }

        // New method for profile pictures
        public FileSaveResult SaveProfilePicture(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
                return new FileSaveResult { Success = false, ErrorMessage = "No file provided." };

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string mimeType = file.ContentType.ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension) || !AllowedMimeTypes.Contains(mimeType))
                return new FileSaveResult { Success = false, ErrorMessage = "Invalid file type." };

            if (!IsValidImage(file.InputStream))
                return new FileSaveResult { Success = false, ErrorMessage = "Invalid image content." };

            try
            {
                if (!Directory.Exists(_profileRoot))
                    Directory.CreateDirectory(_profileRoot);

                string safeFileName = $"{Guid.NewGuid()}{extension}";
                string fullPath = Path.Combine(_profileRoot, safeFileName);

                file.SaveAs(fullPath);

                return new FileSaveResult
                {
                    Success = true,
                    FileName = safeFileName // only file name, not full path
                };
            }
            catch (Exception ex)
            {
                return new FileSaveResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        private bool IsValidImage(Stream stream)
        {
            // Optional: implement real image signature check if needed
            return true;
        }

        public string MapPath(string relativePath)
        {
            // Map relative path to full server path
            return HttpContext.Current.Server.MapPath(relativePath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            return File.ReadAllText(path);
        }

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            return await Task.Run(() => File.ReadAllBytes(path));
        }

        public void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public void WriteAllText(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public string CombinePath(params string[] paths)
        {
            return Path.Combine(paths);
        }
    }

    public class FileSaveResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public string ErrorMessage { get; set; }
    }
}