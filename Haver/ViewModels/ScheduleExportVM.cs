namespace haver.ViewModels
{
    public class ScheduleExportOptionsViewModel
    {
        public ReportType ReportType { get; set; }

        // Machine Schedule Options
        public bool IncludeSalesOrderNumber { get; set; } = true;
        public bool IncludeSalesOrderDate { get; set; } = true;
        public bool IncludeCustomerName { get; set; } = true;
        public bool IncludeMachineDescriptions { get; set; } = true;
        public bool IncludeSerialNumbers { get; set; } = true;
        public bool IncludeProductionOrderNumbers { get; set; } = true;
        public bool IncludePackageReleaseDateE { get; set; } = true;
        public bool IncludePackageReleaseDateA { get; set; } = true;
        public bool IncludeVendorNames { get; set; } = true;
        public bool IncludePoNumbers { get; set; } = true;
        public bool IncludePoDueDates { get; set; } = true;
        public bool IncludeDeliveryDates { get; set; } = true;
        public bool IncludeMedia { get; set; } = true;
        public bool IncludeSpareParts { get; set; } = true;
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

        // Gantt Schedule Options
        public bool IncludeOrderNumber { get; set; } = true;
        public bool IncludeEngineer { get; set; } = true;
        public bool IncludeGanttCustomerName { get; set; } = true;
        public bool IncludeQuantity { get; set; } = true;

        public bool IncludeMachineModel { get; set; } = true;
        //public bool IncludeSize { get; set; } = true;
        //public bool IncludeClass { get; set; } = true;
        //public bool IncludeSizeDeck { get; set; } = true;
        public bool IncludeGanttMedia { get; set; } = true;
        public bool IncludeGanttSpareParts { get; set; } = true;
        public bool IncludeApprovedDrawingReceived { get; set; } = true;
        public bool IncludeGanttData { get; set; } = true;
        public bool IncludeSpecialNotes { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<ScheduleExportOptionsViewModel> PreviousSelections { get; set; } = new List<ScheduleExportOptionsViewModel>();
    }

    public enum ReportType
    {
        MachineSchedules,
        GanttSchedules,
        Both
    }
}
