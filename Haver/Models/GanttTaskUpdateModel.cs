namespace haver.Models
{
	public class GanttTaskUpdateModel
	{
		public int Id { get; set; }
		public string StartDate { get; set; }  // JSON will send as a string
		public string EndDate { get; set; }
		public int Progress { get; set; }
	}
}
