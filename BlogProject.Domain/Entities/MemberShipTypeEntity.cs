﻿using BlogProject.Domain.Entities.Base;

namespace BlogProject.Domain.Entities
{
    public class MemberShipTypeEntity : BaseEntity
    {
        public string Name  { get; set; }
        public string Description { get; set; }
        public bool CanReadPosts { get; set; }
        public bool CanCreatePosts { get; set; }
        public bool CanComment { get; set; }
        public bool CanAccessPremiumContent { get; set; }

        public Guid UserMemberShipId { get; set; }
        public UserMemberShipEntity UserMemberShipEntity { get; set; }
    }
}
