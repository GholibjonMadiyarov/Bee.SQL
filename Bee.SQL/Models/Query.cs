namespace Bee.SQL.Models
{
    public class Query
    {
        public bool execute { get; set; }
        public string message { get; set; }

        public Query() 
        {
            execute = false;
            message = null;
        }
    }
}
