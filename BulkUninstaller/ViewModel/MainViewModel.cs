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
using System.Windows.Threading;

namespace BulkUninstaller.ViewModel
{
    public class MainViewModel
    {
        public MainModel Model { get; }

        private static Dispatcher _uiDispatcher = Application.Current.Dispatcher;

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

            await LoadDataToViewAsync(programs);
            Model.IsLoading = false;
        }

        private async Task LoadDataToViewAsync(IEnumerable<RegistryEntry> programs)
        {
            await Task.Run(() =>
            {
                foreach (var program in programs)
                {
                    var finalProgram = GetProgramWithEstimatedSize(program);
                    _uiDispatcher.Invoke(() =>  Model.RegistryEntries.Add(RegistryEntryToGridRow(finalProgram)));
                }
            });
            
        }

        private RegistryEntryGridRow RegistryEntryToGridRow(RegistryEntry finalProgram)
        {
            return new RegistryEntryGridRow
            {
                DisplayIcon = finalProgram.DisplayIcon,
                DisplayName = finalProgram.DisplayName,
                EstimatedSize = finalProgram.EstimatedSize,
                InstallLocation = finalProgram.InstallLocation,
                UninstallString = finalProgram.UninstallString,
                IsSelected = false
            };
        }

        private RegistryEntry GetProgramWithEstimatedSize(RegistryEntry program)
        {
            if ((program.EstimatedSize == null || program.EstimatedSize == 0) && !string.IsNullOrEmpty(program.InstallLocation))
                program.EstimatedSize = CalculateInstallSize(program.InstallLocation);

            return program;
        }

        private long? CalculateInstallSize(string installLocation)
        {
            try
            {
               return Directory.GetFiles(installLocation, "*", SearchOption.AllDirectories).Sum(t => (new FileInfo(t).Length));
            }
            catch(Exception ex)
            {
                //nothing we can do - size is left empty
                return null;
            }
        }

        private async void OnUninstallSelceted()
        {
            var selectedPrograms = Model.RegistryEntries.Where(_ => _.IsSelected).ToList();
            if (selectedPrograms.Count == 0)
            {
                MessageBox.Show("No programs selected", "Empty selecetion", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Model.IsLoading = true;
            await Task.Run(() =>
            {
                foreach (var program in selectedPrograms)
                {
                    UninstallProgram(program);
                }
            });
            Model.IsLoading = false;
        }

        private void UninstallProgram(RegistryEntryGridRow program)
        {
            var pathEnd = program.UninstallString.IndexOf(".exe") + 4;

            if (ProgramUninstallPathHasParams(pathEnd, program))
            {
                ExecuteUninstallProcess(() => Process.Start(program.UninstallString));
            }
            else
            {
                var paramsIndex = program.UninstallString.IndexOf(' ', pathEnd);
                var programLocation = program.UninstallString.Substring(0, paramsIndex);
                var programParams = program.UninstallString.Substring(paramsIndex + 1);

                ExecuteUninstallProcess(() => Process.Start(programLocation, programParams));
            }
        }

        private void ExecuteUninstallProcess(Func<Process> func)
        {
            try
            {
                var processHandle = func.Invoke();

                //since we are executing an uninstall command it might spawn 
                //others that we do not control and this line will do nothing to prevent spam
                processHandle.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to uninstall", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ProgramUninstallPathHasParams(int pathEnd, RegistryEntryGridRow program)
        {
            return pathEnd == program.UninstallString.Length || pathEnd + 1 == program.UninstallString.Length;
        }
    }
}
