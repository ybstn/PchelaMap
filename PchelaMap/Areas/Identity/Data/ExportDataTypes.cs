using System;
using System.Collections.Generic;
namespace PchelaMap.Areas.Identity.Data
{
    public class ExportDataTypes
    {
        public static List<string> UserRow = new List<string>()
        {
            "Имя",
            "Почта",
            "Телефон",
            "Адрес",
            "Дата регистрации",
            "Не выполннные задания",
            "Созданные задания",
            "Взятые задания",
            "Баллы",
            "Роль"
            
        };
        public static List<string> TaskRow = new List<string>()
        {
            "Описание задания",
            "Адрес",
            "Срочность",
            "Статус",
            "Дата создания",
            "Дата взятия",
            "Дата выполнения",
            "Дата закрытия",
            "Создатель задания",
            "Взял задание"
        };
    }
}
