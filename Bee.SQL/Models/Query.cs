namespace Bee.SQL.Models
{
    public class Query
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public int data { get; set; }
        public bool duplicate { get; set; }

        public Query() 
        {
            execute = false;
            message = null;
            data = -1;
            duplicate = false;
        }
    }
}
