using BulkUninstaller.Common;
using BulkUninstaller.Data;
using BulkUninstaller.Interfaces;
using BulkUninstaller.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BulkUninstaller.ViewModel
{
    public class MainViewModel
    {
        public MainModel Model { get; }

        private readonly IRegistryService _registryService;

        public MainViewModel(IRegistryService registryService)
        {
            Model = new MainModel();

            _registryService = registryService ?? throw new ArgumentNullException(nameof(registryService));

            InitializeModel();
            OnLoadInstalledPrograms();
        }

        private void InitializeModel()
        {
            Model.LoadInstalledProgramsCommand = new Command(OnLoadInstalledPrograms);
            Model.UninstallSelectedCommand = new Command(OnUninstallSelceted);
        }

        private async void OnLoadInstalledPrograms()
        {
            Model.IsLoading = true;
            Model.RegistryEntries.Clear();
            var programs = (await _registryService.GetAllInstalledSoftware()).OrderBy(s => s.DisplayName);

            await TrySetEstimatedSizesAsync(programs);
            var registryEntryGridRows = programs.Select(p => new RegistryEntryGridRow
            {
                DisplayIcon = p.DisplayIcon,
                DisplayName = p.DisplayName,
                EstimatedSize = p.EstimatedSize,
                InstallLocation = p.InstallLocation,
                UninstallString = p.UninstallString,
                IsSelected = false
            });

            Model.RegistryEntries = new ObservableCollection<RegistryEntryGridRow>(registryEntryGridRows);
            Model.IsLoading = false;
        }

        private async Task TrySetEstimatedSizesAsync(IEnumerable<RegistryEntry> programs)
        {
            await Task.Run(() =>
            {
                foreach (var program in programs)
                {
                    if ((program.EstimatedSize == null || program.EstimatedSize == 0) && !string.IsNullOrEmpty(program.InstallLocation))
                        program.EstimatedSize = CalculateInstallSize(program.InstallLocation);
                }
            });
        }

        private long? CalculateInstallSize(string installLocation)
        {
            try
            {
               return Directory.GetFiles(installLocation, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
            }
            catch(Exception ex)
            {
                //swallow for now
                Console.WriteLine();
            }
            return null;
        }

        private void OnUninstallSelceted()
        {
            var selectedPrograms = Model.RegistryEntries.Where(_ => _.IsSelected).ToList();
            if (selectedPrograms.Count == 0)
            {
                MessageBox.Show("No programs selected", "Empty selecetion", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            foreach(var program in selectedPrograms)
            {
                Console.WriteLine(program.UninstallString);
                var pathEnd = program.UninstallString.IndexOf(".exe") + 4;

                if (pathEnd == program.UninstallString.Length || pathEnd + 1 == program.UninstallString.Length)
                {
                    try
                    {
                        Process.Start(program.UninstallString);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to uninstall", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var paramsIndex = program.UninstallString.IndexOf(' ', pathEnd);
                    var programLocation = program.UninstallString.Substring(0, paramsIndex);
                    var programParams = program.UninstallString.Substring(paramsIndex + 1);

                    try
                    {
                        Process.Start(programLocation, programParams);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Failed to uninstall", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
