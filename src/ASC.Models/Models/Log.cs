using ASC.Models.BaseTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Models.Models
{
    public class Log : BaseEntity
    {
        public string Message { get; set; }

        public Log() { }
        public Log(string key)
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = key;
        }
    }
}
