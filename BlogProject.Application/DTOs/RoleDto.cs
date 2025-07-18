﻿namespace BlogProject.Application.DTOs
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public bool IsDeleted { get; set; } = false; // Indicates if the role is deleted or not.

        public string CreatedBy { get; set; } = null!; // Id value of role creator.
        public string? EditedBy { get; set; } // Id value of role editor.
    }
}
