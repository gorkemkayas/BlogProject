﻿using BlogProject.Application.Common;
using BlogProject.Application.DTOs;
using BlogProject.Application.Enums;
using BlogProject.Application.Models;
using BlogProject.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using static BlogProject.Domain.Entities.AppUser;

namespace BlogProject.Application.Interfaces
{
    public interface IUserService
    {
        Task<int> GetFollowerCountByUserId(string userId);
        Task<bool> IsFollowing(string follower, string following);
        Task<bool> ToggleFollowAsync(string followerId, string followingId);
        Task<ServiceResult<LikeEntity>> PostLikeOrDisLike(string userId, string postId);
        Task SaveChangesAsync();
        bool CheckEmailConfirmed(AppUser user);
        Task<AppUser?> FindByUsername(string? userName);
        Task<CurrentUserDto?> FindByUsernameWithDto(string? userName);
        Task<List<AppUser>> GetUsersByCount(int countUser);
        Task<ServiceResult<AppUser>> SubscribeToNotificationsAsync(string email);
        Task<ExtendedProfileDto> ConfigurePictureAsync(ExtendedProfileDto newUserInfo, AppUser oldUserInfo, IFormFile? formFile, PhotoType type);
        Task<ServiceResult<AppUser>> SignUp(SignUpDto request);
        Task<(bool, IEnumerable<IdentityError>?)> SignInAsync(SignInDto request);
        Task<ServiceResult<AppUser>> ConfirmEmailAsync(ConfirmEmailDto request);

        Task<(bool, List<IdentityError>?, bool isCritical)> UpdateProfileAsync(AppUser oldUserInfo, ExtendedProfileDto newUserInfo, IFormFile? fileInputProfile, IFormFile? coverInputProfile, IFormFile? IconInputWorkingAt);
        Task<(bool, IEnumerable<IdentityError>?)> ResetPasswordLinkAsync(ForgetPasswordDto request);

        Task<(bool, IEnumerable<IdentityError>?)> ResetPasswordAsync(ResetPasswordDto request, string? userId, string? token);
        Task<(bool, IEnumerable<IdentityError>?)> ChangePasswordAsync(PasswordChangeDto request, ClaimsPrincipal user);

        Task<ItemPagination<UserDto>> GetPagedUsersAsync(int page, int pageSize, bool includeDeleted = false);
        Task LogInAsync(AppUser user);
        Task LogoutAsync();

        Task SuspendUser(SuspendUserDto request);

        Task<int> GetCommentCountByUserAsync(AppUser user);
        Task<int> GetUserTotalLikeCount(AppUser user);
        Task<int> GetPostCountByUserAsync(AppUser user);
        Task<ExtendedProfileDto> GetExtendedProfileInformationAsync(AppUser currentUser);
        Task<VisitorProfileDto> GetVisitorProfileInformationAsync(AppUser visitedUser);

        Task<List<AppUser>> MostContributors(int countUser);
        Task<List<AppUser>> NewUsers(int countUser);
        Task<List<AppUser>> GetUsers();

        Task<ServiceResult<AppUser>> DeleteUserByTypeAsync(string id, DeleteType deleteType, string deleterUserId);

        Task<ServiceResult<AppUser>> ActivateUserById(string userId);
    }
}
