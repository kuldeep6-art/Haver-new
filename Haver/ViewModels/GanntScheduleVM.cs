namespace haver.ViewModels
{

		//public string Size { get; set; }
		//public string Class { get; set; }
		//public string SizeDeck { get; set; }
		public class GanttScheduleViewModel
		{
			public string OrderNumber { get; set; }
			public string Engineer { get; set; }
			public string CustomerName { get; set; }
			public int Quantity { get; set; }
			public string MachineModel { get; set; }
			public string Media { get; set; }
			public string SpareParts { get; set; }
			public DateTime ApprovedDrawingReceived { get; set; }
			public List<GanttViewModel> GanttData { get; set; }
			public string SpecialNotes { get; set; }
		}
	
}
