using System.Windows.Media;

namespace BulkUninstaller.Data
{
    public class RegistryEntry
    {
        public ImageSource DisplayIcon { get; set; }
        public string DisplayName { get; set; }
        public long? EstimatedSize{ get; set; }
        public string InstallLocation{ get; set; }
        public string UninstallString{ get; set; }
    }
}
