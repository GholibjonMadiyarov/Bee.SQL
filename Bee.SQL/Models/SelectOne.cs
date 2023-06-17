namespace Bee.SQL.Models
{
    public class SelectOne
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public string result { get; set; }

        public SelectOne() 
        {
            execute = false;
            message = null;
            result = null;
        }
    }
}
