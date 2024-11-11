﻿using System.Collections.Generic;

namespace Bee.SQL.Models
{
    public class SelectRow
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public Dictionary<string, object> data { get; set; }
        public bool read { get; set; }
        public bool exception { get; set; }

        public SelectRow() 
        {
            execute = false;
            message = null;
            data = new Dictionary<string, object>();
            read = false;
            exception = false;
        }
    }
}
