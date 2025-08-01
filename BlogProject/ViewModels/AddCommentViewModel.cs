﻿using System.Security.Policy;

namespace BlogProject.Web.ViewModels
{
    public class AddCommentViewModel
    {
        public string AuthorId { get; set; } = null!;
        public string Comment { get; set; } = null!;
        public string? PostId { get; set; }
        public string? ParentCommentId { get; set; }
    }
}
