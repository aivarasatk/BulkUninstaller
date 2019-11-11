using BulkUninstaller.Interfaces;
using BulkUninstaller.Services;
using BulkUninstaller.ViewModel;
using Ninject.Modules;

namespace BulkUninstaller.IoC
{
    public class ContainerConfig : NinjectModule
    {
        public override void Load()
        {
            Bind<MainViewModel>().ToSelf();
            Bind<IRegistryService>().To<RegistryService>();
        }
    }
}
