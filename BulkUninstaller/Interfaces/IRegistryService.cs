using BulkUninstaller.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkUninstaller.Interfaces
{
    public interface IRegistryService
    {
        Task<IEnumerable<RegistryEntry>> GetAllInstalledSoftware();
    }
}
