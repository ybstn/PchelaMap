using System;
using System.ComponentModel.DataAnnotations;
namespace PchelaMap.Models
{
    public class CreateTaskModel
    {
        public string UserAdress { get; set; }
        [Required(ErrorMessage = "добавьте описание задания")]
        public string TaskDescription { get; set; }
        public string UserCoordinates { get; set; }
        public string UserCoordX { get; set; }
        public string UserCoordY { get; set; }
        [Required(ErrorMessage = "добавьте имя пользователя")]
        public string UserName { get; set; }
        [Phone]
        [Required(ErrorMessage = "введите номер телефона")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([8]{1})\)?([0-9]{10})$", ErrorMessage = "введите номер в формате: 89991112233")]
        public string UserPhone { get; set; }
        [EmailAddress]
        [Required(ErrorMessage = "введите почту пользователя")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@([a-zA-Z0-9_\-\.]+)\.([a-zA-Z]{2,5})$", ErrorMessage = "неверный формат почты")]
        public string UserMail { get; set; }
        public bool NeedsCar { get; set; }
        public string Status { get; set; }
        public string id { get; set; }
        public bool Urgentability { get; set; }
        public bool NYTaskBool { get; set; }
        public int CreatedTasksCount { get; set; }
        //technical stuff
        public string UserTakenId { get; set; }
        public string userId { get; set; }
        public bool SelectedTasksOrAll { get; set; }
        public bool OwnTasksOrTaken { get; set; }
    }
}
