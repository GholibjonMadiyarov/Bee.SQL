using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class ExecuteResult
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public List<Dictionary<string, object>> data { get; set; }

        public ExecuteResult()
        {
            execute = false;
            message = null;
            data = new List<Dictionary<string, object>>();
        }
    }
}
