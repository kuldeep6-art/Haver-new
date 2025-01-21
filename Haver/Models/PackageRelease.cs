namespace haver.Models
{
    public class PackageRelease
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public DateTime? PReleaseDateP { get; set; }
        public DateTime? PReleaseDateA { get; set; }
        public string? Notes { get; set; }


        public int MachineScheduleID { get; set; }

        public MachineSchedule? Schedule { get; set; }
    }
}
