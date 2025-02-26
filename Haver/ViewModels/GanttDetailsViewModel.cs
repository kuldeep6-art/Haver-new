using haver.Models;

namespace haver.ViewModels
{
    public class GanttDetailsViewModel
    {
        public GanttData GanttData { get; set; }
        public List<GanttViewModel> GanttTasks { get; set; }
    }

}
