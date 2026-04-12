using BlogProject.Application.Common;
using BlogProject.Application.CustomMethods.Interfaces;
using BlogProject.Application.DTOs;
using BlogProject.Application.Enums;
using BlogProject.Application.Interfaces;
using BlogProject.Application.Models;
using BlogProject.Domain.Entities;
using BlogProject.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogProject.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly BlogDbContext _blogdbContext;
        private readonly IDbContextFactory<BlogDbContext> _contextFactory;
        private readonly IEmailService _emailService;
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;
        private readonly IUsernameGenerator _usernameGenerator;
        private readonly IUserTokenGenerator _userTokenGenerator;
        private readonly IUrlGenerator _urlGenerator;
        public UserService(UserManager<AppUser> userManager, IUsernameGenerator usernameGenerator, SignInManager<AppUser> signInManager, IUserTokenGenerator userTokenService, IUrlGenerator urlGenerator, IEmailService emailService, BlogDbContext blogdbContext, ICommentService commentService, IPostService postService, IDbContextFactory<BlogDbContext> contextFactory)
        {
            _userManager = userManager;
            _usernameGenerator = usernameGenerator;
            _signInManager = signInManager;
            _userTokenGenerator = userTokenService;
            _urlGenerator = urlGenerator;
            _emailService = emailService;
            _blogdbContext = blogdbContext;
            _commentService = commentService;
            _postService = postService;
            _contextFactory = contextFactory;
        }
        public async Task<ServiceResult<AppUser>> DeleteUserByTypeAsync(string id, DeleteType deleteType, string deleterId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                var serviceResult = new ServiceResult<AppUser>()
                {
                    Data = null,
                    IsSuccess = false,
                    Errors = new List<IdentityError>() { new IdentityError { Code = "UserNotFound", Description = "User not found." } }
                };

                return serviceResult;
            }
            var deleterUserId = deleterId;

            var result = new IdentityResult();
            switch (deleteType)
            {
                case DeleteType.Soft:
                    user.IsDeleted = true;
                    //user.DeletedById = deleterUserId;
                    //user.DeletedDate = DateTime.Now;
                    result = await _userManager.UpdateAsync(user);
                    break;
                case DeleteType.Hard:
                    user.IsDeleted = true;
                    user.DeletedBy = deleterUserId;
                    result = await _userManager.DeleteAsync(user);
                    break;
            }
            if (!result.Succeeded)
            {
                return new ServiceResult<AppUser>() { IsSuccess = false, Errors = new List<IdentityError>() { new IdentityError() { Code = "UserCannotDeleted", Description = "User cannot deleted." } } };
            }
            return new ServiceResult<AppUser>()
            {
                IsSuccess = true
            };
        }

        public async Task<bool> ToggleFollowAsync(string followerId, string followingId)
        {
            var existingFollow = await _blogdbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerId.ToString() == followerId && f.FollowingId.ToString() == followingId);

            if (existingFollow != null)
            {
                _blogdbContext.Follows.Remove(existingFollow);
                await _blogdbContext.SaveChangesAsync();
                return false; // Unfollow edildi
            }

            var follow = new FollowEntity
            {
                FollowerId = Guid.Parse(followerId),
                FollowingId = Guid.Parse(followingId)
            };

            _blogdbContext.Follows.Add(follow);
            await _blogdbContext.SaveChangesAsync();
            return true; // Takip edildi
        }
        public async Task<int> GetFollowerCountByUserId(string userId)
        {
            var followerCount = await _blogdbContext.Follows
        .CountAsync(f => f.FollowingId.ToString() == userId);
            return followerCount;
        }

        public bool CheckEmailConfirmed(AppUser user)
        {
            return user.EmailConfirmed;
        }
        public async Task SaveChangesAsync()
        {
            await _blogdbContext.SaveChangesAsync();
        }
        public async Task<ServiceResult<AppUser>> SubscribeToNotificationsAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new ServiceResult<AppUser>()
                {
                    IsSuccess = false,
                    Errors = new List<IdentityError>() { new IdentityError { Code = "UserNotFound", Description = "User not found." } }
                };
            }
            user.GeneralNotificationActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ServiceResult<AppUser>()
                {
                    IsSuccess = false,
                    Errors = result.Errors.ToList()
                };
            }
            return new ServiceResult<AppUser>()
            {
                IsSuccess = true,
                Data = user
            };
        }

        public async Task<int> GetCommentCountByUserAsync(AppUser user)
        {
            var commentCount = await _blogdbContext.Comments.CountAsync(x => x.AuthorId == user.Id);
            return commentCount;
        }
        public async Task<List<AppUser>> GetUsers()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();
            return users;
        }

        public async Task<ItemPagination<UserDto>> GetPagedUsersAsync(int page, int pageSize, bool includeDeleted = false)
        {
            var itemsQuery = _userManager.Users;
            if (!includeDeleted)
            {
                itemsQuery = itemsQuery.Where(p => p.IsDeleted == false);
            }

            var pagedUsers = new ItemPagination<UserDto>()
            {
                PageSize = pageSize,
                CurrentPage = page,
                TotalCount = (includeDeleted is true) ? _userManager.Users.Count() : _userManager.Users.Where(p => p.IsDeleted == false).Count(),
                Items = await itemsQuery
                                    .Skip((page - 1) * pageSize)
                                    .Take(pageSize)
                                    .Select(user => new UserDto
                                    {
                                        Id = user.Id.ToString(),
                                        Name = user.Name,
                                        Surname = user.Surname,
                                        Username = user.UserName!,
                                        Email = user.Email!,
                                        IsDeleted = user.IsDeleted,
                                        PhoneNumber = user.PhoneNumber,
                                        RegisteredDate = user.RegisteredDate,
                                        EmailConfirmed = user.EmailConfirmed,
                                        PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                                        TwoFactorEnabled = user.TwoFactorEnabled,
                                        LastLoginDate = user.LastLoginDate,
                                        SuspendedTo = user.SuspendedTo,
                                        IsSuspended = user.IsSuspended

                                    }).ToListAsync()
            };
            return pagedUsers;
        }

        public async Task<int> GetUserTotalLikeCount(AppUser user)
        {
            var likeCount = await _blogdbContext.Likes.CountAsync(x => x.UserId == user.Id);
            return likeCount;
        }

        public async Task<int> GetPostCountByUserAsync(AppUser user)
        {
            var postCount = await _blogdbContext.Posts.CountAsync(x => x.AuthorId == user.Id);
            return postCount;
        }

        public async Task<bool> IsFollowing(string follower, string following)
        {
            var follow = await _blogdbContext.Follows
                .FirstOrDefaultAsync(f => f.FollowerId.ToString() == follower && f.FollowingId.ToString() == following);
            return follow != null;
        }
        public async Task<VisitorProfileDto> GetVisitorProfileInformationAsync(AppUser visitedUser)
{
    var likeCount = await _blogdbContext.Posts
        .Where(p => p.AuthorId == visitedUser.Id)
        .SelectMany(p => p.Likes)
        .CountAsync();

    var featuredPosts = await _blogdbContext.Posts.AsNoTracking()
        .Where(p => p.AuthorId == visitedUser.Id)
        .OrderByDescending(p => p.ViewCount)
        .Take(2)
        .Select(p => new PostCardDto
        {
            Id = p.Id,
            Title = p.Title,
            Subtitle = p.Subtitle,
            CoverImageUrl = p.CoverImageUrl,
            LikeCount = p.Likes.Count,
            CommentCount = p.Comments.Count
        })
        .ToListAsync();

    var recentPosts = await _blogdbContext.Posts.AsNoTracking()
        .Where(p => p.AuthorId == visitedUser.Id)
        .OrderByDescending(p => p.CreatedTime)
        .Take(2)
        .Select(p => new PostCardDto
        {
            Id = p.Id,
            Title = p.Title,
            Subtitle = p.Subtitle,
            CoverImageUrl = p.CoverImageUrl,
            LikeCount = p.Likes.Count,
            CommentCount = p.Comments.Count
        })
        .ToListAsync();

    var followersCount = await _blogdbContext.Follows
        .CountAsync(f => f.FollowingId == visitedUser.Id);

    var followingCount = await _blogdbContext.Follows
        .CountAsync(f => f.FollowerId == visitedUser.Id);

    var commentCount = await _commentService.GetCommentCountByUserAsync(visitedUser);
    var postCount = await _postService.GetPostCountByUserAsync(visitedUser);

    return new VisitorProfileDto()
    {
        UserName = visitedUser.UserName!,
        Name = visitedUser.Name,
        Surname = visitedUser.Surname,
        Bio = visitedUser.Bio,
        BirthDate = visitedUser.BirthDate,
        Country = visitedUser.Country,
        Title = visitedUser.Title,
        RegisteredDate = visitedUser.RegisteredDate,
        ProfilePicture = visitedUser.ProfilePicture,
        CoverImagePicture = visitedUser.CoverImagePicture,
        WorkingAtLogo = visitedUser.WorkingAtLogo,
        FollowersCount = followersCount,
        FollowingCount = followingCount,
        CommentCount = commentCount,
        PostCount = postCount,
        LikeCount = likeCount,
        FeaturedPosts = featuredPosts,
        RecentPosts = recentPosts,
        WorkingAt = visitedUser.WorkingAt,
        VisitedUserId = visitedUser.Id.ToString()
    };
}

        public async Task<ExtendedProfileDto> GetExtendedProfileInformationAsync(AppUser currentUser)
        {
            var postCount = await _postService.GetPostCountByUserAsync(currentUser);
            var commentCount = await _commentService.GetCommentCountByUserAsync(currentUser);
            var likeCount = await _blogdbContext.Posts
                .Where(p => p.AuthorId == currentUser.Id)
                .SelectMany(p => p.Likes)
                .CountAsync();

            var followersCount = await _blogdbContext.Follows
                .CountAsync(f => f.FollowingId == currentUser.Id);
            var followingCount = await _blogdbContext.Follows
                .CountAsync(f => f.FollowerId == currentUser.Id);

            var featuredPosts = await _blogdbContext.Posts.AsNoTracking()
                .Where(p => p.AuthorId == currentUser.Id)
                .OrderByDescending(p => p.ViewCount)
                .Take(2)
                .Select(p => new PostCardDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Subtitle = p.Subtitle,
                    CoverImageUrl = p.CoverImageUrl,
                    LikeCount = p.Likes.Count,
                    CommentCount = p.Comments.Count
                })
                .ToListAsync();

            var recentPosts = await _blogdbContext.Posts.AsNoTracking()
                .Where(p => p.AuthorId == currentUser.Id)
                .OrderByDescending(p => p.CreatedTime)
                .Take(2)
                .Select(p => new PostCardDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Subtitle = p.Subtitle,
                    CoverImageUrl = p.CoverImageUrl,
                    LikeCount = p.Likes.Count,
                    CommentCount = p.Comments.Count
                })
                .ToListAsync();

            var extendedProfile = new ExtendedProfileDto()
            {
                Id = currentUser.Id.ToString(),
                PhoneNumber = currentUser.PhoneNumber,
                EmailAddress = currentUser.Email ?? string.Empty,
                EmailConfirmed = currentUser.EmailConfirmed,
                TwoFactorEnabled = currentUser.TwoFactorEnabled,
                LockoutEnabled = currentUser.LockoutEnabled,
                SecurityStamp = currentUser.SecurityStamp,
                ConcurrencyStamp = currentUser.ConcurrencyStamp?.ToString(),

                UserName = currentUser.UserName,
                Name = currentUser.Name,
                Surname = currentUser.Surname,
                Title = currentUser.Title,
                Bio = currentUser.Bio,
                WorkingAt = currentUser.WorkingAt,
                Country = currentUser.Country,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                PostCount = postCount,
                CommentCount = commentCount,
                LikeCount = likeCount,
                CurrentPosition = currentUser.CurrentPosition,
                City = currentUser.City,
                Address = currentUser.Address,
                XAddress = currentUser.XAddress,
                LinkedinAddress = currentUser.LinkedinAddress,
                GithubAddress = currentUser.GithubAddress,
                MediumAddress = currentUser.MediumAddress,
                YoutubeAddress = currentUser.YoutubeAddress,
                PersonalWebAddress = currentUser.PersonalWebAddress,
                HighSchoolName = currentUser.HighSchoolName,
                HighSchoolStartYear = currentUser.HighSchoolStartYear,
                HighSchoolGraduationYear = currentUser.HighSchoolGraduationYear,
                BirthDate = currentUser.BirthDate,
                RegisteredDate = currentUser.RegisteredDate,
                ProfilePicture = currentUser.ProfilePicture,
                CoverImagePicture = currentUser.CoverImagePicture,
                WorkingAtLogo = currentUser.WorkingAtLogo,
                FeaturedPosts = featuredPosts,
                RecentPosts = recentPosts
            };

            return extendedProfile;
        }


        public async Task<(bool, List<IdentityError>?, bool isCritical)> UpdateProfileAsync(AppUser oldUserInfo, ExtendedProfileDto newUserInfo, IFormFile? fileInputProfile, IFormFile? coverInputProfile, IFormFile? IconInputWorkingAt)
        {
            var errors = new List<IdentityError>();

            bool criticalUpdate = false;

            if (oldUserInfo == null)
            {
                errors.Add(new() { Code = "UserNotFound", Description = "The user not found in the system." });
                return (false, errors, false);
            }

            if (oldUserInfo.Id.ToString() != newUserInfo.Id)
            {
                errors.Add(new() { Code = "UsersNotMatched", Description = "The users not matched." });
                return (false, errors, false);
            }

            if (oldUserInfo.Email != newUserInfo.EmailAddress) criticalUpdate = true;

            var step1 = await ConfigurePictureAsync(newUserInfo, oldUserInfo, fileInputProfile, PhotoType.ProfilePicture);
            var step2 = await ConfigurePictureAsync(step1, oldUserInfo, coverInputProfile, PhotoType.CoverImagePicture);
            var step3 = await ConfigurePictureAsync(step2, oldUserInfo, IconInputWorkingAt, PhotoType.WorkingAtLogo);


            //var step1 = await ConfigureProfilePictureOfNewUserInfoAsync(newUserInfo, oldUserInfo, fileInputProfile);
            //var step2 = await ConfigureCoverPictureOfNewUserInfoAsync(step1, oldUserInfo, coverInputProfile);
            //var step3 = await ConfigureWorkingIconOfNewUserInfoAsync(step2, oldUserInfo, IconInputWorkingAt);

            var updatedUser = new AppUser()
            {
                //From ExtendedProfileDto
                Id = Guid.Parse(step3.Id),
                PhoneNumber = step3.PhoneNumber,
                Email = step3.EmailAddress,
                EmailConfirmed = step3.EmailConfirmed,
                TwoFactorEnabled = step3.TwoFactorEnabled,
                LockoutEnabled = step3.LockoutEnabled,
                SecurityStamp = step3.SecurityStamp,
                ConcurrencyStamp = step3.ConcurrencyStamp,

                //From VisitorProfileDto - which base of ExtendedProfileDto
                UserName = step3.UserName,
                Name = step3.Name,
                Surname = step3.Surname,
                Title = step3.Title,
                Bio = step3.Bio,
                WorkingAt = step3.WorkingAt,
                Country = step3.Country,
                FollowersCount = step3.FollowersCount,
                FollowingCount = step3.FollowingCount,
                Posts = oldUserInfo.Posts,
                Comments = oldUserInfo.Comments,
                Likes = oldUserInfo.Likes,
                CurrentPosition = step3.CurrentPosition,
                City = step3.City,
                Address = step3.Address,
                XAddress = step3.XAddress,
                LinkedinAddress = step3.LinkedinAddress,
                GithubAddress = step3.GithubAddress,
                MediumAddress = step3.MediumAddress,
                YoutubeAddress = step3.YoutubeAddress,
                PersonalWebAddress = step3.PersonalWebAddress,
                HighSchoolName = step3.HighSchoolName,
                HighSchoolStartYear = step3.HighSchoolStartYear,
                HighSchoolGraduationYear = step3.HighSchoolGraduationYear,
                UniversityName = step3.UniversityName,
                UniversityStartYear = step3.UniversityStartYear,
                UniversityGraduationYear = step3.UniversityGraduationYear,
                BirthDate = step3.BirthDate,
                ProfilePicture = step3.ProfilePicture,
                CoverImagePicture = step3.CoverImagePicture,
                WorkingAtLogo = step3.WorkingAtLogo,

            };
            await _userManager.UpdateAsync(updatedUser);

            if (criticalUpdate)
            {
                await _userManager.UpdateSecurityStampAsync(oldUserInfo);
                return (true, null, true);
            }

            return (true, null, false);
        }
        public async Task<ExtendedProfileDto> ConfigurePictureAsync(ExtendedProfileDto newUserInfo, AppUser oldUserInfo, IFormFile? formFile, PhotoType type)
        {

            if (formFile is null)
            {
                newUserInfo.SetProperty(type, oldUserInfo.GetPropertyValue(type));
                return newUserInfo;
            }

            string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "userPhotos", $"{oldUserInfo.UserName}");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }
            string fileName;
            if (formFile.FileName.Length > 100)
            {
                fileName = formFile.FileName.Replace(" ", "_").Substring(0, 100);
            }
            else
            {
                fileName = formFile.FileName.Replace(" ", "_");
            }
            string filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }
            newUserInfo.SetProperty(type, fileName);

            if (oldUserInfo.GetPropertyValue(type) != null)
            {
                var oldPhotoPath = Path.Combine(Directory.GetCurrentDirectory(), uploadPath, oldUserInfo.GetPropertyValue(type));

                if (File.Exists(oldPhotoPath))
                {
                    File.Delete(oldPhotoPath);
                }

            }
            return newUserInfo;
        }



        public async Task<List<ContributorDto>> MostContributors(int countUser)
        {
            if (countUser <= 0)
                throw new ArgumentException("CountUser değeri 0'dan büyük olmalıdır.", nameof(countUser));

            using var context = _contextFactory.CreateDbContext();

            var usersWithCounts = await context.Users
                .AsNoTracking()
                .Select(u => new ContributorDto
                {
                     FullName = $"{u.Name} {u.Surname}",
                     Title = u.Title ?? string.Empty,
                    UserName = u.UserName,
                    ProfileImageUrl = u.ProfilePicture ?? string.Empty,
                    PostCount = context.Posts.Count(p => p.AuthorId == u.Id),
                    FollowerCount = context.Follows.Count(f => f.FollowingId == u.Id)
                })
                .OrderByDescending(x => x.PostCount)
                .Take(countUser)
                .ToListAsync();

            // İstersen FollowersCount'u kullanabilirsin, ben sadece User döndürüyorum
            return usersWithCounts;
        }

        public async Task<List<AppUser>> NewUsers(int countUser)
        {
            var users = await _userManager.Users.AsNoTracking().OrderByDescending(x => x.RegisteredDate).Take(countUser).AsNoTracking().ToListAsync();
            return users;
        }

        public async Task<ServiceResult<AppUser>> SignUp(SignUpDto request)
        {
            var identityResult = await _userManager.CreateAsync(new()
            {
                UserName = _usernameGenerator.GenerateUsernameByEmail(request.Email),
                Email = request.Email,
                Name = request.Name,
                Surname = request.Surname,
                BirthDate = (DateTime)request.BirthDate!
            }, request.Password);

            if (identityResult.Succeeded)
            {
                return new ServiceResult<AppUser>()
                {
                    IsSuccess = true,
                    Data = await _userManager.FindByEmailAsync(request.Email)
                };
            }

            return new ServiceResult<AppUser>()
            {
                IsSuccess = false,
                Errors = identityResult.Errors.ToList()
            };
        }
        public async Task<ServiceResult<AppUser>> ConfirmEmailAsync(ConfirmEmailDto request)
        {
            if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
            {
                return Failure("InvalidRequest", "User ID or token cannot be null.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return Failure("UserNotFound", "User not found.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                return new ServiceResult<AppUser>
                {
                    IsSuccess = false,
                    Errors = result.Errors.ToList()
                };
            }

            return new ServiceResult<AppUser>
            {
                IsSuccess = true,
                Data = user
            };
        }

        private ServiceResult<AppUser> Failure(string code, string description)
        {
            return new ServiceResult<AppUser>
            {
                IsSuccess = false,
                Errors = new List<IdentityError>
        {
            new IdentityError { Code = code, Description = description }
        }
            };
        }


        public async Task<(bool, IEnumerable<IdentityError>?)> SignInAsync(SignInDto request)
        {
            var errors = new List<IdentityError>();

            var hasUser = await _userManager.FindByEmailAsync(request.Email);

            if (hasUser == null)
            {
                errors.Add(CreateError("SignInError", "The email or password is incorrect."));
                return (false, errors);
            }

            if (hasUser.SuspendedTo != null && hasUser.SuspendedTo > DateTime.UtcNow)
            {
                errors.Add(CreateError("SuspendedAccount", $"Your account is suspended and will be accessible at {hasUser.SuspendedTo}."));
                errors.Add(CreateError("SuspendedAccountCategory", hasUser.SuspensionReasonCategory ?? string.Empty));
                errors.Add(CreateError("SuspendedAccountReason", hasUser.SuspensionReasonDetail ?? string.Empty));
                return (false, errors);
            }

            if (!hasUser.EmailConfirmed)
            {
                errors.Add(CreateError("EmailNotConfirmed", "Your email is not confirmed."));
                return (false, errors);
            }

            await _signInManager.SignOutAsync();

            var signInResult = await _signInManager.PasswordSignInAsync(hasUser, request.Password, request.RememberMe, lockoutOnFailure: true);

            if (signInResult.Succeeded)
            {
                // Claim güncellemesi (sadece başarılı girişte)
                var profilePicture = hasUser.ProfilePicture ?? string.Empty;
                var existingClaims = await _userManager.GetClaimsAsync(hasUser);
                var existingProfileClaim = existingClaims.FirstOrDefault(c => c.Type == "ProfilePictureUrl");

                if (existingProfileClaim == null || existingProfileClaim.Value != profilePicture)
                {
                    if (existingProfileClaim != null)
                        await _userManager.RemoveClaimAsync(hasUser, existingProfileClaim);

                    await _userManager.AddClaimAsync(hasUser, new Claim("ProfilePictureUrl", profilePicture));
                }

                // Kullanıcı verilerini güncelle (varsa)
                bool needsUpdate = hasUser.SuspendedTo != null ||
                                   hasUser.SuspensionReasonCategory != null ||
                                   hasUser.SuspensionReasonDetail != null ||
                                   hasUser.LastLoginDate?.Date != DateTime.UtcNow.Date;

                if (needsUpdate)
                {
                    hasUser.SuspendedTo = null;
                    hasUser.SuspensionReasonCategory = null;
                    hasUser.SuspensionReasonDetail = null;
                    hasUser.LastLoginDate = DateTime.UtcNow;

                    await _userManager.UpdateAsync(hasUser);
                }

                return (true, null);
            }

            if (signInResult.IsLockedOut)
            {
                errors.Add(CreateError("AccountLocked", $"Your account is locked and will be accessible at {hasUser.LockoutEnd}."));
                return (false, errors);
            }

            // Kullanıcıyı güncel çek → AccessFailedCount değişmiş olabilir
            hasUser = await _userManager.FindByEmailAsync(request.Email);

            errors.Add(CreateError("SignInAttempts", $"You have tried {hasUser?.AccessFailedCount ?? 0} times. Remaining attempts: {5 - (hasUser?.AccessFailedCount ?? 0)}"));
            errors.Add(CreateError("SignInError", "The email or password is incorrect."));

            return (false, errors);
        }

        private IdentityError CreateError(string code, string description)
        {
            return new IdentityError
            {
                Code = code,
                Description = description
            };
        }



        public async Task<AppUser?> FindByUsername(string? userName)
        {
            if (userName == null)
            {
                return null;
            }
            return await _blogdbContext.Users
                    .FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<CurrentUserDto?> FindByUsernameWithDto(string? userName)
        {
            if (userName == null)
            {
                return null;
            }
            return await _blogdbContext.Users.Select(u => new CurrentUserDto() { UserName = u.UserName, ProfilePicture = u.ProfilePicture })
                    .FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<List<AppUser>> GetUsersByCount(int countUser)
        {
            var users = await _userManager.Users.AsNoTracking().Take(countUser).ToListAsync();
            return users;
        }
        public async Task SuspendUser(SuspendUserDto request)
        {

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) throw new Exception("User not found");

            //user.IsSuspended = true;
            if (request.SuspensionMinutes == 0)
            {
                user.SuspendedTo = null;
            }
            else
            {
                user.SuspendedTo = DateTime.Now.AddMinutes(request.SuspensionMinutes);
            }

            user.SuspensionReasonCategory = request.ReasonCategory;
            user.SuspensionReasonDetail = request.ReasonDetail;


            var signInUrl = _urlGenerator.GenerateCustomUrl("User", "SignIn");
            await _emailService.SendSuspensionNotificationEmailAsync(user.Email, signInUrl, request.ReasonCategory == "Remove");
            await _userManager.UpdateSecurityStampAsync(user);
            await _userManager.UpdateAsync(user);
        }
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();

        }
        public async Task LogInAsync(AppUser user)
        {
            await _signInManager.SignInAsync(user, false);

            user.LastLoginDate = DateTime.Now;
            await _userManager.UpdateAsync(user);
        }

        public async Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(PasswordChangeDto request, ClaimsPrincipal user)
        {
            var errors = new List<IdentityError>();

            var currentUser = await _userManager.GetUserAsync(user);

            var isSame = await _userManager.CheckPasswordAsync(currentUser, request.OldPassword);

            if (!isSame)
            {
                errors.Add(new IdentityError() { Code = "ChangePasswordError", Description = "The old password is incorrect." });
                return (false, errors);
            }

            var result = await _userManager.ChangePasswordAsync(currentUser, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors);
                return (false, errors);
            }

            await _userManager.UpdateSecurityStampAsync(currentUser);
            await _signInManager.SignOutAsync();

            await _signInManager.SignInAsync(currentUser, true);

            return (true, null);




        }

        public async Task<(bool, IEnumerable<IdentityError>?)> ResetPasswordLinkAsync(ForgetPasswordDto request)
        {
            var errors = new List<IdentityError>();

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                errors.Add(new IdentityError() { Code = "ResetPasswordError", Description = "The email is not registered." });
                return (false, errors);
            }

            var passwordResetToken = await _userTokenGenerator.GeneratePasswordResetTokenAsync(user);
            var passwordResetLink = _urlGenerator.GenerateResetPasswordUrl(user, passwordResetToken);

            await _emailService.SendResetPasswordEmailAsync(passwordResetLink, user.Email!);

            return (true, null);
        }

        public async Task<(bool, IEnumerable<IdentityError>?)> ResetPasswordAsync(ResetPasswordDto request, string? userId, string? token)
        {
            var errors = new List<IdentityError>();

            if (userId == null || token == null)
            {
                errors.Add(new IdentityError() { Code = "ResetPasswordError", Description = "Token or userId not found." });
                return (false, errors);
            }

            var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);

            if (hasUser == null)
            {
                errors.Add(new IdentityError() { Code = "ResetPasswordError", Description = "User not found." });
                return (false, errors);
            }

            var result = await _userManager.ResetPasswordAsync(hasUser, token.ToString()!, request.NewPassword);

            if (result.Succeeded)
            {
                return (true, null);
            }

            errors.AddRange(result.Errors);

            return (false, errors);
        }

        public async Task<ServiceResult<AppUser>> ActivateUserById(string userId)
        {
            var user = await _blogdbContext.Users.FindAsync(Guid.Parse(userId));
            if (user == null)
            {
                return new ServiceResult<AppUser>()
                {
                    IsSuccess = false,
                    Errors = new List<IdentityError>() { new IdentityError { Code = "UserNotFound", Description = "User not found." } }
                };
            }
            try
            {
                user.IsDeleted = false;
                _blogdbContext.Users.Update(user);
                await _blogdbContext.SaveChangesAsync();
                return new ServiceResult<AppUser>() { IsSuccess = true, Data = user };
            }
            catch (Exception)
            {
                return new ServiceResult<AppUser>()
                {
                    IsSuccess = false,
                    Errors = new List<IdentityError>() { new IdentityError { Code = "ActivateUserError", Description = "An error occurred while activating the user." } }
                };
            }
        }
        public async Task<ServiceResult<LikeEntity>> PostLikeOrDisLike(string userId, string postId)
        {
            var userExist = await _userManager.FindByIdAsync(userId);
            if (userExist == null)
            {
                return new ServiceResult<LikeEntity>()
                {
                    IsSuccess = false,
                    Errors = new List<IdentityError>() { new IdentityError { Code = "UserNotFound", Description = "User not found." } }
                };
            }
            var post = await _blogdbContext.Posts.FindAsync(Guid.Parse(postId));
            if (post == null)
            {
                return new ServiceResult<LikeEntity>()
                {
                    IsSuccess = false,
                    Errors = new List<IdentityError>() { new IdentityError { Code = "PostNotFound", Description = "Post not found." } }
                };
            }
            var like = await _blogdbContext.Likes.FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId) && x.PostId == Guid.Parse(postId));
            if (like is null)
            {
                like = new LikeEntity()
                {
                    UserId = Guid.Parse(userId),
                    PostId = Guid.Parse(postId)
                };
                try
                {
                    await _blogdbContext.Likes.AddAsync(like);
                    return new ServiceResult<LikeEntity>()
                    {
                        IsSuccess = true,
                        Data = like
                    };
                }
                catch (Exception)
                {
                    return new ServiceResult<LikeEntity>()
                    {
                        IsSuccess = false,
                        Errors = new List<IdentityError>() { new IdentityError { Code = "LikePostError", Description = "An error occurred while liking the post." } }
                    };
                }
            }
            else
            {
                _blogdbContext.Likes.Remove(like);
                return new ServiceResult<LikeEntity>()
                {
                    IsSuccess = true,
                    Data = like
                };
            }
        }
    }

}
