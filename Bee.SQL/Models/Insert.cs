using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class Insert
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public object lastInsertedId { get; set; }
        public bool duplicate { get; set; }
        public bool exception { get; set; }
        public string exceptionType { get; set; }

        public Insert() 
        {
            execute = false;
            message = null;
            lastInsertedId = null;
            duplicate = false;
            exception = false;
            exceptionType = null;
        }
    }
}
