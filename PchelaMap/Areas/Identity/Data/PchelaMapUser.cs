using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.AspNetCore.Identity;

namespace PchelaMap.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the PchelaMapUser class
    public class PchelaMapUser : IdentityUser
    {
        [PersonalData]
        public string Name { get; set; } = "";

        [PersonalData]
        public string UserAdress { get; set; } = "";

        [PersonalData]
        public string UserCoordinateX { get; set; } = "";

        [PersonalData]
        public string UserCoordinateY { get; set; } = "";

        [PersonalData]
        public string SocialAccountID { get; set; } = "";

        [PersonalData]
        public string UserPhoto { get; set; } = "";

        [PersonalData]
        public int HasCar { get; set; }

        [PersonalData]
        public int UserPoints { get; set; }

        [PersonalData]
        public string CreatedDateUtc { get; set; } = "";

        [PersonalData]
        public int uncompletedTasks { get; set; }

        [PersonalData]
        public string SystemMessageForUser { get; set; }

        public virtual ICollection<PchelaMapTask> Tasks { get; set; }

    }
    public class PchelaMapTask
    {
        [Key]
        public string id { get; set; }
        public string CoordinateX { get; set; }
        public string CoordinateY { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Adress { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string Phone { get; set; }
        public string UserMail { get; set; }
        public int NeedsCar { get; set; }
        public string ResultMediaFolder { get; set; }
        public string AdminComment { get; set; }
        public string CreatedDateUtc { get; set; }
        public string ClosedDateUtc { get; set; }
        public int Urgentable { get; set; }
        public int NY_task { get; set; }
        public virtual PchelaMapUser User { get; set; }
    }
    public class GlobalStatusEditModel
    {
        public string TaskId { get; set; }
        public string TaskStatus { get; set; }
        public List<string> AllStatuses { get; set; }
        public static readonly Dictionary<string, string> GlobalTaskStatusDictionary = new Dictionary<string, string>()
        {
             { "active","Активно" },
             { "in_progress" ,"Выполняется"},
            { "closed" ,"Закрыто"},
            { "moderating" , "Ожидает модерации"},
            {"stoped", "Не прошло модерацию" }
        };
    }
    public class PchelaMapUserTasks
    {
        [ForeignKey("TaskID")]
        public string TaskID { get; set; }
        [ForeignKey("UserID")]
        public string UserID { get; set; }
        public string Status { get; set; }
       
        public string MediaFolder { get; set; }
        public int CompliteCounts { get; set; }
        public string AdminComment { get; set; }
        public string UserComment { get; set; }
        public string DateTaken { get; set; }
        public string DateDone { get; set; }

        public string Promo { get; set; }
    }
    public class StatusEditModel
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string TaskId { get; set; }
        public string TaskStatus { get; set; }
        public List<string> AllStatuses { get; set; }
        public static readonly Dictionary<string, string> TaskStatusDictionary = new Dictionary<string, string>()
        {
            { "active","Активно" },
            { "done" ,"Выполнено"},
            { "complite_moderation" , "Ожидает модерации"},
            {"StopedOnModeration" ,"Не прошло модерацию"}
        };
    }
    public class UsersRefusedTasks
    {
        [ForeignKey("TaskID")]
        public string TaskID { get; set; }
        [ForeignKey("UserID")]
        public string UserID { get; set; }
        public string Reason { get; set; }
        public string MediaFolder { get; set; }
        public string AdminComment { get; set; }
        public string DateTaken { get; set; }
        public string DateRefused { get; set; }
        public int RefuseCount { get; set; }
    }
    public class RefuseReasons
    {
        public static string header { get; } = "Жаль, что Вы не можете помочь в этот раз.";
        public static string bottom_header { get; } = "До встречи. Нашим подопечным нужна Ваша помощь!";
        public List<string> AllReasons { get; set; }
        public static readonly Dictionary<string, string> RefuseReasonsDict = new Dictionary<string, string>()
        {
            { "plans_changed", "Изменились планы" },
            { "cannot_complite", "Не смогу выполнить задание"},
            { "interest" , "Было интересно, как это работает"}
        };
    }
    public class PromoCsv
    {
        public string code { get; set; }
    }

    public class PromoBd
    {
        public string code { get; set; }
        public int status { get; set; }
        public string userId { get; set; }
        public string taskId { get; set; }
    }
    public class Promo
    {
        
        public string code { get; set; }
        public int status { get; set; }
        public string userId { get; set; }
        public string userName { get; set; } 
        public string userPhoto { get; set; } 
        public string taskId { get; set; }
        public string taskInfo { get; set; } 
        public int indx { get; set; }
        public static readonly Dictionary<int, string> PromoDict = new Dictionary<int, string>()
        {
            { 0, "СВОБОДНО" },
            { 1, "ВЗЯТО"},
            { 2, "ПОТРАЧЕН"}
        };
    }
}
