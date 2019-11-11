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
        private bool _isLoading;
        public ICommand LoadInstalledProgramsCommand { get; set; }
        public ICommand UninstallSelectedCommand { get; set; }

        public MainModel()
        {
            RegistryEntries = new ObservableCollection<RegistryEntryGridRow>();
        }


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

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                this.MutateVerbose(ref _isLoading, value, RaisePropertyChanged());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }
}
