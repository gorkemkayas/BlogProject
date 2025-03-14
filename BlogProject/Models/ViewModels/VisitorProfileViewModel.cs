﻿using BlogProject.src.Infra.Entitites;

namespace BlogProject.Models.ViewModels
{
    public class VisitorProfileViewModel : BaseProfileViewModel
    {
        public string Name { get; set; } = null!;
        public string Surname { get; set; } = null!;

        public string FullName { get { return Name + " " + Surname; } }
        public string? Title { get; set; }
        public string? Bio { get; set; }
        public string? WorkingAt { get; set; }

        public string? Country { get; set; }

        // yeni eklediklerim
        public string? CurrentPosition { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }

        public string? XAddress { get; set; }
        public string? LinkedinAddress { get; set; }
        public string? GithubAddress { get; set; }
        public string? MediumAddress { get; set; }
        public string? YoutubeAddress { get; set; }
        public string? PersonalWebAddress { get; set; }

        public string? HighSchoolName { get; set; }
        public string? HighSchoolStartYear { get; set; }
        public string? HighSchoolGraduationYear { get; set; }

        public string? UniversityName { get; set; }
        public string? UniversityStartYear { get; set; }
        public string? UniversityGraduationYear { get; set; }

        public ICollection<PostEntity> FeaturedPosts { get; set; } = new List<PostEntity>();



        // sonu
        public DateTime BirthDate { get; set; }

        public DateTime? RegisteredDate { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverImagePicture { get; set; }
    }
}
