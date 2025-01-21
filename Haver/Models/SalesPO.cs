namespace haver.Models
{
    public class SalesPO
    {
        public int ID { get; set; }
        public string? PoNumber { get; set; }
        public DateTime? PODueDate { get; set; }

        public ICollection<SalesOrderPO> SalesOrderPOs { get; set; } = new HashSet<SalesOrderPO>();
    }
}
