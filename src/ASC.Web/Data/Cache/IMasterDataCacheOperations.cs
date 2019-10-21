using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ASC.Web.Data.Cache
{
    public interface IMasterDataCacheOperations
    {
        Task<MasterDataCache> GetMasterDataCacheAsync();
        Task CreateMasterDataCacheAsync();
    }
}
