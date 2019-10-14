using ASC.Models.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Models.MasterDataViewModels
{
    public class MasterKeysViewModel
    {
        public List<MasterDataKeyViewModel> MasterKeys { get; set; }
        public MasterDataKeyViewModel MasterKeyInContext { get; set; }
        public bool IsEdit { get; set; }
    }
}
