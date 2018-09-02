using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicSorter2
{
    public abstract class UserSettings
    {
        public abstract void SetValue(string key, object value);
        public abstract object GetValue(string key, object default_val=null);

        public bool GetBoolean(string key, bool default_val = false)
        {
            object result = GetValue(key, default_val);
            if (result is bool) return (bool)result;

            return bool.Parse(result.ToString());
        }

        public string GetString(string key, string default_val = null)
        {
            return GetValue(key, default_val).ToString();
        }
    }
}
