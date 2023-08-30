using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class Insert
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public List<int?> insertedId { get; set; }

        public Insert() 
        {
            execute = false;
            message = null;
            insertedId = new List<int?>();
        }
    }
}
