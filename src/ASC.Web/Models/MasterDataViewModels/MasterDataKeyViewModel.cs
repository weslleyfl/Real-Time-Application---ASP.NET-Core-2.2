using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Models.MasterDataViewModels
{
    public class MasterDataKeyViewModel
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public bool IsActive { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
