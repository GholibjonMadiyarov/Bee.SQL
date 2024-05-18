using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class Insert
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public bool duplicate { get; set; }
        public int affectedRowCount { get; set; }

        public Insert() 
        {
            execute = false;
            message = null;
            duplicate = false;
            affectedRowCount = 0;
        }
    }
}
