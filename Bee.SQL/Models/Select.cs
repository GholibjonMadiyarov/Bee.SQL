using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class Select
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public List<Dictionary<string, string>> result { get; set; }

        public Select() 
        {
            execute = false;
            message = null;
            result = new List<Dictionary<string, string>>();
        }
    }
}
