using System.Collections.Generic;

namespace Bee.SQL.Models
{
    internal class Select
    {
        public int code { get; set; }
        public string message { get; set; }
        public Dictionary<string, string> data { get; set; }

        public Select() 
        {
            code = 0;
            message = null;
            data = new Dictionary<string, string>();
        }
    }
}
