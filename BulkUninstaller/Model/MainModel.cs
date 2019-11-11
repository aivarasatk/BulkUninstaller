using BulkUninstaller.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BulkUninstaller.Model
{
    public class MainModel : INotifyPropertyChanged
    {
        private ObservableCollection<RegistryEntryGridRow> _registryEntries;
        public ICommand LoadInstalledProgramsCommand { get; set; }
        public ICommand UninstallSelectedCommand { get; set; }


        public ObservableCollection<RegistryEntryGridRow> RegistryEntries
        {
            get
            {
                return _registryEntries;
            }
            set
            {
                this.MutateVerbose(ref _registryEntries, value, RaisePropertyChanged());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }
}
