namespace Bee.SQL.Models
{
    public class ExecuteQuery
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public bool duplicate { get; set; }

        public ExecuteQuery() 
        {
            execute = false;
            message = null;
            data = null;
            duplicate = false;
        }
    }
}
