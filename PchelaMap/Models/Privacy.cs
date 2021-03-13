using System;
using System.Collections.Generic;
namespace PchelaMap.Models
{
    public class Privacy
    {
        public string header { get; set; }
        public List<string> text { get; set; }
        public List<string> alert { get; set; }
    }
    public class Policy
    {
        public string header { get; set; }
        public List<List<string>> text { get; set; }
    }
}
