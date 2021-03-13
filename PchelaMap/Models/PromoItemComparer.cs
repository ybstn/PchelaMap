using System;
using System.Collections.Generic;
using PchelaMap.Areas.Identity.Data;

namespace PchelaMap.Models
{
    public class PromoItemComparer : IEqualityComparer<PromoCsv>
    {
        public bool Equals(PromoCsv x, PromoCsv y)
        {
            return x.code == y.code; 
        }
        public int GetHashCode(PromoCsv obj)
        {
            return obj.code.GetHashCode(); 
        }
    }
}
