using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PchelaMap.Models
{
    public class UserWithTasks
    {
        public string id { get; set; }
        public int _index { get; set; }
        public string CreatorID { get; set; }
        public string CoordinateX { get; set; }
        public string CoordinateY { get; set; }
        public string Name { get; set; }
        public string PhotoUrl { get; set; }
        public string description { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Adress { get; set; }
        public int NeedsCar { get; set; }
        public int UrgentStatus { get; set; }
        public string Status { get; set; }
        public string GlobalStatus { get; set; }
        public bool UserLogined { get; set; }
        public string ResultMediaFolder { get; set; }
        public string[] ResultFiles { get; set; }
        public bool TaskAlreadyTaken { get; set; }
        public string TaskOwner { get; set; }
        public string UserTakenID { get; set; }
        public string UserTakenName { get; set; }
        public string UserTakenPhoto { get; set; }
        public string FromAdminMessage { get; set; }
        public string FromUserMessage { get; set; }
        public int UserTakenCount { get; set; }
        public int UserDoneCount { get; set; }
        public string _DateTaken { get; set; }
        public string _DateDone { get; set; }
        public string _CreatedDateUtc { get; set; }
        public string _TimeInProcess { get; set; }
        public bool LimitOfTasksReached { get; set; }
        public bool LimitOfUrgentTasksReached { get; set; }
        public bool _isOverdue { get; set; }
        public string promo { get; set; }

    }
}
