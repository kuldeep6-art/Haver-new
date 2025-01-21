namespace haver.Models
{
    public class SalesOrderPO
    {
        public int ID { get; set; }
        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public int SalesPOID { get; set; }
        public SalesPO? SalesPO { get; set; }
    }
}
