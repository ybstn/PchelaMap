using System;
using System.Collections.Generic;
namespace PchelaMap.Models
{
    public class AllUsers
    {
     public List<User> UsersList { get; set; }
     public List<UserWithTasks> UsersTaskList { get; set; }
        public List<UserWithTasks> UrgentTasksList { get; set; }
        public List<UserWithTasks> DoneTasksList { get; set; }
    }
}
