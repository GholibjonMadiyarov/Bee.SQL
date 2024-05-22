using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class ExecuteSelect
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public List<Dictionary<string, object>> data { get; set; }

        public ExecuteSelect()
        {
            execute = false;
            message = null;
            data = new List<Dictionary<string, object>>();
        }
    }
}
