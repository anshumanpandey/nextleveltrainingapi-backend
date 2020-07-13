﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NextLevelTrainingApi.ViewModels
{
    public class TrainingLocationViewModel
    {
        
        public Guid? TrainingLocationId { get; set; }
        [Required]
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
    }
}
