using Sitecore.HabitatHome.Foundation.DependencyInjection;
using System.IO;

namespace Sitecore.HabitatHome.Foundation.Accounts.Services
{
    [Service(typeof(IExportFileService))]
    public class ExportFileService : IExportFileService
    {
        private readonly string _tempFolder = Path.Combine(Sitecore.IO.FileUtil.MapPath(Sitecore.Configuration.Settings.TempFolderPath), "ExportedFiles");

        public string CreateExportFile()
        {
            var id = System.Guid.NewGuid();

            var fileName = id + ".json";

            if (!Directory.Exists(_tempFolder))
            {
                Directory.CreateDirectory(_tempFolder);
            }

            File.Create(GetFilePath(fileName)).Close();

            return fileName;
        }

        public void WriteExportedDataIntoFile(string fileName, string exportedData)
        {
            System.IO.File.WriteAllText(GetFilePath(fileName), exportedData);
        }

        public byte[] ReadExportedDataFromFile(string fileName)
        {
            return File.ReadAllBytes(GetFilePath(fileName));
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_tempFolder, fileName);
        }
    }
}