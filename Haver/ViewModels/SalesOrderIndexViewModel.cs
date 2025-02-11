using haver.Models;
using haver.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace haver.ViewModels
{
    public class SalesOrderIndexViewModel
    {
        public PaginatedList<SalesOrder> ActiveSalesOrders { get; set; }
        public PaginatedList<SalesOrder> ArchivedSalesOrders { get; set; }

        public string SortField { get; set; }
        public string SortDirection { get; set; }

        public string SearchString { get; set; }
        public int? CustomerID { get; set; }

        public IEnumerable<SelectListItem> CustomerList { get; set; }
    }
}
