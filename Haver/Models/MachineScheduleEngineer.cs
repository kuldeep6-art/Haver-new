namespace haver.Models
{
    public class MachineScheduleEngineer : Auditable
    {
        public int ID { get; set; }


        public int MachineScheduleID { get; set; }

        public MachineSchedule? Schedule { get; set; }

        public int EngineerID { get; set; }
        public Engineer? Engineer { get; set; }
    }
}
