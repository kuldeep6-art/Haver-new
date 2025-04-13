namespace haver.ViewModels
{
	public class ScheduleExportOptionsViewModel
	{
		public ReportType ReportType { get; set; }

		// Consolidated Fields (shared across Machine and Gantt)
		public bool IncludeOrderNumber { get; set; } = true; // Replaces SalesOrderNumber, OrderNumber
		public bool IncludeCustomerName { get; set; } = true; // Replaces CustomerName, GanttCustomerName
		public bool IncludeMedia { get; set; } = true; // Replaces Media, GanttMedia
		public bool IncludeSpareParts { get; set; } = true; // Replaces SpareParts, GanttSpareParts
		public bool IncludeMachineModel { get; set; } = true; // Replaces MachineDescriptions, MachineModel

		// Machine Schedule Specific
		public bool IncludeSalesOrderDate { get; set; } = true;
		public bool IncludeSerialNumbers { get; set; } = true;
		public bool IncludeProductionOrderNumbers { get; set; } = true;
		public bool IncludePackageReleaseDateE { get; set; } = true;
		public bool IncludePackageReleaseDateA { get; set; } = true;
		public bool IncludeVendorNames { get; set; } = true;
		public bool IncludePoNumbers { get; set; } = true;
		public bool IncludePoDueDates { get; set; } = true;
		public bool IncludeDeliveryDates { get; set; } = true;
		public bool IncludeBase { get; set; } = true;
		public bool IncludeAirSeal { get; set; } = true;
		public bool IncludeCoatingLining { get; set; } = true;
		public bool IncludeDisassembly { get; set; } = true;
		public bool IncludePreOrder { get; set; } = true;
		public bool IncludeScope { get; set; } = true;
		public bool IncludeActualAssemblyHours { get; set; } = true;
		public bool IncludeReworkHours { get; set; } = true;
		public bool IncludeNamePlate { get; set; } = true;
		public bool IncludeNotes { get; set; } = true;

		// Gantt Schedule Specific
		public bool IncludeEngineer { get; set; } = true;
		public bool IncludeQuantity { get; set; } = true;
		public bool IncludeApprovedDrawingReceived { get; set; } = true;
		public bool IncludeGanttData { get; set; } = true;
		public bool IncludeSpecialNotes { get; set; } = true;

		public DateTime SelectionDate { get; set; } = DateTime.Now;


		public List<ScheduleExportOptionsViewModel> PreviousSelections { get; set; } = new List<ScheduleExportOptionsViewModel>();
	}
	public enum ReportType
	{
		MachineSchedules,
		GanttSchedules,
		Both
	}
}
