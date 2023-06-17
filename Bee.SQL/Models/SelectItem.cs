using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class SelectItem
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public Dictionary<string, string> result { get; set; }

        public SelectItem() 
        {
            execute = false;
            message = null;
            result = new Dictionary<string, string>();
        }
    }
}
