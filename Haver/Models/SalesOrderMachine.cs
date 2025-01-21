namespace haver.Models
{
    public class SalesOrderMachine
    {
        public int ID { get; set; }

        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public int MachineID { get; set; }
        public Machine? Machine { get; set; }
    }
}
