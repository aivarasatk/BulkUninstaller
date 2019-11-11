using System.Windows.Media;

namespace BulkUninstaller.Data
{
    public class RegistryEntryGridRow
    {
        public ImageSource DisplayIcon { get; set; }
        public string DisplayName { get; set; }
        public long? EstimatedSize { get; set; }
        public string InstallLocation { get; set; }
        public string UninstallString { get; set; }
        public bool IsSelected { get; set; }
    }
}
