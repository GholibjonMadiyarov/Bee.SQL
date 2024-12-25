using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class SelectRowString
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public Dictionary<string, string> data { get; set; }
        public bool read { get; set; }
        public bool exception { get; set; }

        public SelectRowString() 
        {
            execute = false;
            message = null;
            data = new Dictionary<string, string>();
            read = false;
            exception = false;
        }
    }
}
