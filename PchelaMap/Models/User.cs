using System;
using System.Collections.Generic;
namespace PchelaMap.Models
{
    public class User
    {
        public int InListNumber { get; set; }
        public string id { get; set; }
        public string CoordinateX { get; set; } = "";
        public string CoordinateY { get; set; } = "";
        public string Name { get; set; } = "";
        public string PhotoUrl { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Adress { get; set; } = "";
        public string Email { get; set; }
        public string SocialMediaId { get; set; } = "";
        public string LoginProviderName { get; set; }
        public string TasksCount { get; set; }
        public string TakenTasksCount { get; set; }
        public string DoneTasksCount { get; set; }
        public int HasCar { get; set; }
        public string RegistrationDate { get; set; } = "";
        public int UserPoints { get; set; }
        public int uncompletedTasks { get; set; }
        public string Role { get; set; }
        public int _index { get; set; }
    }
   
}
