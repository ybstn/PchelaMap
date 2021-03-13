using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
namespace PchelaMap.Areas.Identity.Data
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MustBeTrueAttribute: ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value != null && value is bool && (bool)value;
        }
    }
}
