﻿namespace BlogProject.Domain.Entities
{
    public class BadgeUserEntity
    {
       
        public Guid BadgeId { get; set; }
        public virtual BadgeEntity Badge { get; set; }

        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; }


        public DateTime AwardDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
