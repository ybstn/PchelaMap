using System;

namespace PchelaMap.Models
{
    public class MainPageClass
    {
       public int UsersCount { get; set; }
        public int TasksCount { get; set; }
        public int CurrUserTasksCount { get; set; }
        public UserModal ModalData { get; set; }
    }
}
