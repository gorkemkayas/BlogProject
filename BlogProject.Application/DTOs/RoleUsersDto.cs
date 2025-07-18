﻿namespace BlogProject.Application.DTOs
{
    public class RoleUsersDto
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;

        public string CreatorName { get; set; } = null!;
        public string CreatorUserName { get; set; } = null!;
        public string? ModifiedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public List<UserDto> Users { get; set; } = new List<UserDto>();
    }
}
