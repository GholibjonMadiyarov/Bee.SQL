namespace Bee.SQL.Models
{
    public class SelectValueDouble
    {
        public bool execute { get; set; }
        public string message { get; set; }
        public double? value { get; set; }
        public bool read { get; set; }
        public bool exception { get; set; }

        public SelectValueDouble() 
        {
            execute = false;
            message = null;
            value = null;
            read = false;
            exception = false;
        }
    }
}
