using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class SelectItem
    {
        public int code { get; set; }
        public string message { get; set; }
        public Dictionary<string, string> data { get; set; }

        public SelectItem() 
        {
            code = 0;
            message = null;
            data = new Dictionary<string, string>();
        }
    }
}
