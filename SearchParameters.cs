﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WikiDataHelpDeskBot
{
    public class SearchParameters
    {
        public string InstanceOf { get; set; }

        public Dictionary<string, string> Filters { get; set; } = new Dictionary<string, string>();
    }
}