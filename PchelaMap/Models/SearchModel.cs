using System;
using System.ComponentModel.DataAnnotations;

namespace PchelaMap.Models
{
    public class UsersSearchModel
    {
        public string name { get; set; }
        public string mail { get; set; }
        public string phone { get; set; }
        public string adress { get; set; }
    }
    public class TasksSearchModel
    {
        public string name { get; set; }
        public string description { get; set; }
        public string mail { get; set; }
        public string phone { get; set; }
        public string adress { get; set; }
    }
}
