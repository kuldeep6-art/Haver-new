    namespace haver.ViewModels
{
    public class GanttViewModel
    {
        public int ID { get; set; }

		public string UniqueID { get; set; } // New unique string identifier
		public string MachineName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Progress { get; set; }
        public string MilestoneClass { get; set; }
    }
}
