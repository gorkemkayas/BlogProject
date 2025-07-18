﻿using BlogProject.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Web.ViewModels
{
    public class ExtendedProfileViewModel : VisitorProfileViewModel
    {
        public string Id { get; set; } = null!;
        public string? PhoneNumber { get; set; }

        [Required]
        public string EmailAddress { get; set; } = null!;
        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }

        public string? SecurityStamp {  get; set; }
        public string? ConcurrencyStamp { get; set; }

        public void SetProperty(PhotoType photoType, string value)
        {
            switch (photoType)
            {
                case PhotoType.ProfilePicture:
                    ProfilePicture = value;
                    break;
                case PhotoType.CoverImagePicture:
                    CoverImagePicture = value;
                    break;
                case PhotoType.WorkingAtLogo:
                    WorkingAtLogo = value;
                    break;
                default:
                    throw new Exception("Invalid operation.");
            }
        }
        
    }
}
