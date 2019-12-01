using System;
using System.Collections.Generic;
using System.Text;

namespace WikiDataHelpDeskBot
{
    public class SearchParameters
    {
        public string InstanceOf { get; set; }

        public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, DateTime> DateFilters { get; set; } = new Dictionary<string, DateTime>();
        public string ExtraMessage { get; set; }
    }
}
