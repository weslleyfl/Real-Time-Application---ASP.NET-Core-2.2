using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ASC.Utilities
{
    public static class SessionExtensions
    {
        public static void SetSession(this ISession session, string key, object value)
        {
            session.Set(key, Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value)));
        }
        
        public static T GetSession<T>(this ISession session, string key)
        {
            if (session.TryGetValue(key, out byte[] value))
                return JsonConvert.DeserializeObject<T>(Encoding.ASCII.GetString(value));
            else
                return default(T);
        }

    }
}
