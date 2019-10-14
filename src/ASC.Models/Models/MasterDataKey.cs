using ASC.Models.BaseTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Models.Models
{
    /// <summary>
    ///  MasterDataKey is the entity that will hold all the keys of the master data such as State and County.
    /// </summary>
    public class MasterDataKey : BaseEntity, IAuditTracker
    {
        public MasterDataKey() { }
        public MasterDataKey(string key)
        {
            this.RowKey = Guid.NewGuid().ToString();
            this.PartitionKey = key;
        }

        public bool IsActive { get; set; }
        public string Name { get; set; }

    }
}
