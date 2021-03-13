using System;


namespace PchelaMap.Models
{
    public class SortSearchPagin
    {

        public enum UserSortState
        {
            none,
            IndexAsc,
            IndexDesc,
            DateAsc,
            DateDesc,
            NameAsc,
            NameDesc,
            HasAutoAsc,
            HasAutoDesc,
            CreatedTasksAsc,
            CreatedTasksDesc,
            TakenTasksAsc,
            TakenTasksDesc,
            PointsAsc,
            PointsDesc,
            UnfinishedAsc,
            UnfinishedDesc
        }
     
        public enum TaskSortState
        {
            none,
            IndexAsc,
            IndexDesc,
            DateAsc,
            DateDesc,
            NameAsc,
            NameDesc,
            HasAutoAsc,
            HasAutoDesc,
            UrgentableAsc,
            UrgentableDesc,
            UsersTakenAsc,
            UsersTakenDesc,
            UsersDoneAsc,
            UsersDoneDesc
        }
        public enum PromoSortState
        {
            none,
            IndexAsc,
            IndexDesc,
            StatusAsc,
            StatusDesc
        }
    }
}
