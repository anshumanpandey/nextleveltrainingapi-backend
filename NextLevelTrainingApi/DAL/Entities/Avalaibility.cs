﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.DAL.Entities
{
    public class Availability
    {
        public string Day { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
    }
}
