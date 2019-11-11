using BulkUninstaller.Data;
using BulkUninstaller.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BulkUninstaller.Services
{
    public class RegistryService : IRegistryService
    {
        private readonly string _microsoftUninstallReg = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        private readonly string _wow6432UninstallReg = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

        public async Task<IEnumerable<RegistryEntry>> GetAllInstalledSoftware()
        {
            var softwareList = new List<RegistryEntry>();
            await Task.Run(() =>
            {
                using (var uninstallRegKey = Registry.CurrentUser.OpenSubKey(_microsoftUninstallReg))
                    softwareList.AddRange(GetInstalledSoftwareUnderSubKey(uninstallRegKey));

                //the app is being redirected to wow64 due to app being 32-bit, so we have to specify RegistryView
                using (var root = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var uninstallRegKey = root.OpenSubKey(_microsoftUninstallReg))
                        softwareList.AddRange(GetInstalledSoftwareUnderSubKey(uninstallRegKey));
                }

                using (var uninstallRegKey = Registry.LocalMachine.OpenSubKey(_wow6432UninstallReg))
                    softwareList.AddRange(GetInstalledSoftwareUnderSubKey(uninstallRegKey));
            });

            return softwareList;
        }
        private IEnumerable<RegistryEntry> GetInstalledSoftwareUnderSubKey(RegistryKey uninstallRegKey)
        {
            var keys = uninstallRegKey.GetSubKeyNames();
            foreach (var key in uninstallRegKey.GetSubKeyNames())
            {
                var subKey = uninstallRegKey.OpenSubKey(key);
                if (IsProgramInControlPanel(subKey))
                {
                    var iconPath = subKey.GetValue("DisplayIcon") as string;
                    yield return new RegistryEntry
                    {
                        DisplayIcon = GetDisplayIcon(iconPath),
                        DisplayName = subKey.GetValue("DisplayName") as string,
                        EstimatedSize = subKey.GetValue("EstimatedSize") as long?,
                        InstallLocation = subKey.GetValue("InstallLocation") as string,
                        UninstallString = subKey.GetValue("UninstallString") as string
                    };
                }
            }
        }

        private static bool IsProgramInControlPanel(RegistryKey subKey)
        {
            var name = subKey.GetValue("DisplayName") as string;
            var releaseType = subKey.GetValue("ReleaseType") as string;
            var uninstallString = subKey.GetValue("UninstallString") as string;
            var systemComponent = subKey.GetValue("SystemComponent") as int?;
            var parentName = subKey.GetValue("ParentDisplayName") as string;

            return
                !string.IsNullOrEmpty(name)
                && !string.IsNullOrEmpty(uninstallString)
                && string.IsNullOrEmpty(releaseType)
                && string.IsNullOrEmpty(parentName)
                && (systemComponent == null || systemComponent.Value == 0);
        }

        private ImageSource GetDisplayIcon(string filePath)
        {
            var defaultIconPath = "broken.ico";
            if (string.IsNullOrEmpty(filePath)) return GetImageSourceFromIconPath(defaultIconPath);
            try
            {
                var splitFilePath = filePath.Split(',');
                var cleanPath = splitFilePath[0].Replace("\"", "");
                return GetImageSourceFromIconPath(cleanPath);
            }
            catch(Exception ex)
            {
                return GetImageSourceFromIconPath(defaultIconPath);
            }
            
        }

        private ImageSource GetImageSourceFromIconPath(string path)
        {
            var icon = Icon.ExtractAssociatedIcon(path);
            using (Bitmap bmp = icon.ToBitmap())
            {
                var stream = new MemoryStream();
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return BitmapFrame.Create(stream);
            }
        }
    }
}
