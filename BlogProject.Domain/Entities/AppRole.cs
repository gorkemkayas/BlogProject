﻿using Microsoft.AspNetCore.Identity;

namespace BlogProject.Domain.Entities
{
    public class AppRole : IdentityRole<Guid>
    {
        public string? CreatedBy { get; set; }
        public string? EditedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        public bool IsDeleted { get; set; } = false;
    }
}
