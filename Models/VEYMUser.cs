﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Jp2Portal.Models
{
    public class VEYMUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Rank { get; set; }
        public string Leauge { get; set; }
        public string Chapter { get; set; }
    }
}
