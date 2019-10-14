using System;
using System.Collections.Generic;
using System.Text;
using ASC.Models.BaseTypes;

namespace ASC.Models.Models
{
    /// <summary>
    /// MasterDataValue is the entity that will hold the values of master keys. Ex: for example, California and Oregon for State (Key)
    /// and Sacramento and Orange for County
    /// </summary>
    public class MasterDataValue : BaseEntity, IAuditTracker
    {
        public MasterDataValue() { }
        public MasterDataValue(string masterDataPartitionKey, string value)
        {
            this.PartitionKey = masterDataPartitionKey;
            this.RowKey = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// we have the IsActive property, which can be used to turn on/off the entity status in business operations.
        /// </summary>
        public bool IsActive { get; set; }
        public string Name { get; set; }
    }
}
