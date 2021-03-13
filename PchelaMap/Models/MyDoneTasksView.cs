using System;
using System.Collections.Generic;
namespace PchelaMap.Models
{
    public class MyDoneTasksView
    {
        public List<UserWithTasks> CreatedDoneTasks { get; set; }
        public List<UserWithTasks> TakenDoneTasks { get; set; }
    }
}
