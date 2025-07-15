namespace NPMSyncWorker.Models
{
    internal class ShiftsAttribute
    {
        public int practice_id { get; set; }
        public string start_time { get; set; }
        public string end_time { get; set; }
        public bool is_available { get; set; }
    }
}
