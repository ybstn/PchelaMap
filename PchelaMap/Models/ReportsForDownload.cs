using System;
namespace PchelaMap.Models
{
    public class ReportsForDownload
    {
        public string id { get; set; }
        public string CreatorID { get; set; }
        public string Name { get; set; }
        public string description { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Adress { get; set; }
        public int NeedsCar { get; set; }
        public int UrgentStatus { get; set; }
        public string ResultMediaFolder { get; set; }
        public string[] ResultFiles { get; set; }
        public string UserTakenID { get; set; }
        public string UserTakenName { get; set; }
        public string FromAdminMessage { get; set; }
        public string FromUserMessage { get; set; }
        public string _DateTaken { get; set; }
        public string _DateDone { get; set; }
        public string _CreatedDateUtc { get; set; }
        public string _TimeInProcess { get; set; }
    }
}
