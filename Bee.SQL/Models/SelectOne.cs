namespace Bee.SQL.Models
{
    public class SelectOne
    {
        public int code { get; set; }
        public string message { get; set; }
        public string data { get; set; }

        public SelectOne() 
        {
            code = 0;
            message = null;
            data = null;
        }
    }
}
