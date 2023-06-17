namespace Bee.SQL.Models
{
    public class Query
    {
        public int code { get; set; }
        public string message { get; set; }

        public Query() 
        {
            code = 0;
            message = null;
        }
    }
}
