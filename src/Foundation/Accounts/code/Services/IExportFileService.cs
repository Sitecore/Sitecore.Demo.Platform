namespace Sitecore.Demo.Platform.Foundation.Accounts.Services
{
    public interface IExportFileService
    {
        string CreateExportFile();

        void WriteExportedDataIntoFile(string filePath, string exportedData);

        byte[] ReadExportedDataFromFile(string filename);
    }
}