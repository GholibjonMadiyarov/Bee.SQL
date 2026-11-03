namespace Bee.SQL.Models
{
    public class ExecuteQuery
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public string stackTrace { get; set; }
        public int affectedRowCount { get; set; }

        public ExecuteQuery() 
        {
            execute = false;
            message = null;
            stackTrace = null;
            affectedRowCount = 0;
        }
    }
}
