
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace AzureStorageTestesConsole
{

    public static class ObjectExtension
    {
        public static T CopyObject<T>(this object objSource)
        {
            var serialized = JsonConvert.SerializeObject(objSource);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
    }
}
