using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class Select
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<Dictionary<string, string>> data { get; set; }

        public Select() 
        {
            code = 0;
            message = null;
            data = new List<Dictionary<string, string>>();
        }
    }
}
