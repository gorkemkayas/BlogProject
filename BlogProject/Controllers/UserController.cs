using AutoMapper;
using BlogProject.Application.CustomMethods.Interfaces;
using BlogProject.Application.DTOs;
using BlogProject.Application.Interfaces;
using BlogProject.Areas.Admin.Models;
using BlogProject.Domain.Entities;
using BlogProject.Extensions;
using BlogProject.Web.ViewModels;
using BlogProject.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICommentService _commentService;
        private readonly IEmailService _emailService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUrlGenerator _urlGenerator;

        private readonly IMapper _mapper;

        public UserController(IUserService userService, IEmailService emailService, UserManager<AppUser> userManager, ICommentService commentService, IUrlGenerator urlGenerator, IMapper mapper)
        {
            _userService = userService;
            _userManager = userManager;
            _commentService = commentService;
            _emailService = emailService;
            _urlGenerator = urlGenerator;
            _mapper = mapper;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId || currentUserId is null) return BadRequest();

            var isFollowing = await _userService.ToggleFollowAsync(currentUserId, userId);
            var followerCount = await _userService.GetFollowerCountByUserId(userId);

            return Json(new { success = true, following = isFollowing, followerCount = followerCount });
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel request, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(request); // Validation hataları varsa formu geri döndür

            var mappedRequest = _mapper.Map<SignInDto>(request);
            var result = await _userService.SignInAsync(mappedRequest);

            if (!result.Item1)
            {
                var errors = result.Item2?.ToList();

                if (errors != null)
                {
                    if (errors.Any(e => e.Code == "SuspendedAccount"))
                    {
                        TempData["SuspensionMessage"] = errors.FirstOrDefault(e => e.Code == "SuspendedAccount")?.Description;
                        TempData["SuspensionCategory"] = errors.FirstOrDefault(e => e.Code == "SuspendedAccountCategory")?.Description;
                        TempData["SuspensionDetail"] = errors.FirstOrDefault(e => e.Code == "SuspendedAccountReason")?.Description;

                        // Eğer bu bilgi UI'da kritikse sakla
                        var suspendedUser = await _userManager.FindByEmailAsync(request.Email);
                        TempData["SuspendedUserId"] = suspendedUser?.Id;

                        ModelState.AddModelError(string.Empty, TempData["SuspensionMessage"]?.ToString() ?? "Your account is suspended.");
                        return View(request); // Redirect yerine View
                    }

                    if (errors.Any(e => e.Code == "EmailNotConfirmed"))
                    {
                        TempData["Error"] = "Please confirm your email address!";
                        TempData["EmailNotConfirmed"] = true;
                        TempData["UserEmail"] = request.Email;
                        return View(request); // Redirect yerine View
                    }

                    ModelState.AddModelErrorList(errors);
                }

                return View(request); // Redirect yerine View
            }

            TempData["Succeed"] = "You have logged in successfully.";

            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl!);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = _urlGenerator.GenerateEmailConfirmationUrl(user, token);

            await _emailService.SendEmailConfirmationEmailAsync(user.Email!, confirmationLink);

            return Ok();
        }

        [HttpGet]
        public IActionResult ConfirmEmail(string? userId, string? token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return View();
            }
            ViewBag.UserId = userId;
            ViewBag.Token = token;
            return View(new ConfirmEmailViewModel() { UserId = userId, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailViewModel request)
        {
            var mappedRequest = _mapper.Map<ConfirmEmailDto>(request);
            var result = await _userService.ConfirmEmailAsync(mappedRequest);
            if (!result.IsSuccess)
            {
                ModelState.AddModelErrorList(result.Errors!);
                return View(request);
            }
            TempData["Succeed"] = "Email confirmed successfully!";

            return RedirectToAction(nameof(UserController.SignIn));

            //confirm edildiyse confirmEmail sayfasında 'basarıyla dogrulandı' mesajı ver
            //confirm edilmediyse 'dogrulama hatası' mesajı ver
            // yönlendirme olmadan girildiyse hata sayfasına yönlendir
        }


        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            var mappedRequest = _mapper.Map<SignUpDto>(request);
            var result = await _userService.SignUp(mappedRequest);

            if (result.IsSuccess)
            {
                TempData["Succeed"] = "User created successfully..";

            }
            else if (!result.IsSuccess)
            {
                TempData["Failed"] = "User could not be created.";

                foreach (var error in result.Errors!)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(request);

            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(result.Data!);
            var confirmationLink = _urlGenerator.GenerateEmailConfirmationUrl(result.Data!, token);

            await _emailService.SendEmailConfirmationEmailAsync(result.Data!.Email!, confirmationLink);

            return RedirectToAction("ConfirmEmail", "User");
        }

        public async Task Logout()
        {
            TempData["Succeed"] = "You have logged out successfully.";
            await _userService.LogoutAsync();
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var mappedRequest = _mapper.Map<ForgetPasswordDto>(request);
            (bool isOk, IEnumerable<IdentityError>? errors) = await _userService.ResetPasswordLinkAsync(mappedRequest);

            if (!isOk)
            {
                ModelState.AddModelErrorList(errors!.ToList());

                return View();
            }

            TempData["Succeed"] = "Password reset link has been sent to your e-mail address.";
            return RedirectToAction("forgetpassword", "user");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string userId)
        {
            TempData["Token"] = token;
            TempData["userId"] = userId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel request)
        {
            var userId = TempData["userId"];
            var token = TempData["Token"];

            var mappedRequest = _mapper.Map<ResetPasswordDto>(request);
            var isOk = await _userService.ResetPasswordAsync(mappedRequest, userId?.ToString(), token?.ToString());

            if (isOk.Item1)
            {
                TempData["Succeed"] = "Password reset successfully.";

                var user = await _userManager.FindByIdAsync(userId!.ToString()!);
                var userEmail = user!.Email;

                await _emailService.SendPasswordChangedNotificationAsync("You changed your password", userEmail!);

                return RedirectToAction("resetpassword", "user");
            }


            ModelState.AddModelErrorList(isOk.Item2!.ToList());

            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(PasswordChangeViewModel request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var mappedRequest = _mapper.Map<PasswordChangeDto>(request);
            var result = await _userService.ChangePasswordAsync(mappedRequest, User);
            if (!result.Item1)
            {
                ModelState.AddModelErrorList(result.Item2!.ToList());
                return View();
            }

            var user = await _userManager.GetUserAsync(User);
            var userEmail = user!.Email;

            TempData["Succeed"] = "Password changed successfully.";
            await _emailService.SendPasswordChangedNotificationAsync("You changed your password", userEmail!);

            return RedirectToAction(nameof(Profile), "User", new { userName = User.Identity!.Name });
        }

        public async Task<IActionResult> Profile(string? userName)
        {
            if (userName == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var visitedUser = await _userManager.FindByNameAsync(userName);

            if (visitedUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var postCount = await _userService.GetPostCountByUserAsync(visitedUser!);
            var commentCount = await _commentService.GetCommentCountByUserAsync(visitedUser!);


            if (currentUser == visitedUser)
            {
                ViewBag.IsOwner = true;

                var extendedProfileInfo = await _userService.GetExtendedProfileInformationAsync(currentUser);

                var mappedProfileInfo2 = _mapper.Map<ExtendedProfileViewModel>(extendedProfileInfo);
                return View(mappedProfileInfo2);
            }
            var visitorProfileInfo = await _userService.GetVisitorProfileInformationAsync(visitedUser);
            if (currentUser is null)
            {
                visitorProfileInfo.IsFollowing = false;
            }
            else
            {
                var isFollowing = await _userService.IsFollowing(currentUser!.Id.ToString(), visitedUser.Id.ToString());
            visitorProfileInfo.IsFollowing = isFollowing;

            }
            var mappedProfileInfo = _mapper.Map<VisitorProfileViewModel>(visitorProfileInfo);


            return View(mappedProfileInfo);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ExtendedProfileViewModel request, IFormFile? fileInputProfile, IFormFile? coverInputProfile, IFormFile? IconInputWorkingAt)
        {
            Console.WriteLine("Modelstate girisi");
            if (!ModelState.IsValid)
            {
                return View();
            }
            Console.WriteLine("Modelstate valid");

            var oldUserInfo = _userManager.FindByIdAsync(request.Id).Result;

            var step1 = await PhotoSaver.ConfigurePictureAsync(request, oldUserInfo!, fileInputProfile, PhotoType.ProfilePicture);
            var step2 = await PhotoSaver.ConfigurePictureAsync(step1, oldUserInfo!, coverInputProfile, PhotoType.CoverImagePicture);
            var step3 = await PhotoSaver.ConfigurePictureAsync(step2, oldUserInfo!, IconInputWorkingAt, PhotoType.WorkingAtLogo);

            //var mappedRequest = _mapper.Map(step3, oldUserInfo);

            //From ExtendedProfileDto
            oldUserInfo.Id = Guid.Parse(step3.Id);
            oldUserInfo.PhoneNumber = step3.PhoneNumber;
            oldUserInfo.Email = step3.EmailAddress;
            oldUserInfo.EmailConfirmed = step3.EmailConfirmed;
            oldUserInfo.TwoFactorEnabled = step3.TwoFactorEnabled;
            oldUserInfo.LockoutEnabled = step3.LockoutEnabled;
            oldUserInfo.SecurityStamp = step3.SecurityStamp;
            oldUserInfo.ConcurrencyStamp = step3.ConcurrencyStamp;

            //From VisitorProfileDto - which base of ExtendedProfileDto
            oldUserInfo.UserName = step3.UserName;
            oldUserInfo.Name = step3.Name;
            oldUserInfo.Surname = step3.Surname;
            oldUserInfo.Title = step3.Title;
            oldUserInfo.Bio = step3.Bio;
            oldUserInfo.WorkingAt = step3.WorkingAt;
            oldUserInfo.Country = step3.Country;
            oldUserInfo.FollowersCount = step3.FollowersCount;
            oldUserInfo.FollowingCount = step3.FollowingCount;
            oldUserInfo.Posts = oldUserInfo.Posts;
            oldUserInfo.Comments = oldUserInfo.Comments;
            oldUserInfo.Likes = oldUserInfo.Likes;
            oldUserInfo.CurrentPosition = step3.CurrentPosition;
            oldUserInfo.City = step3.City;
            oldUserInfo.Address = step3.Address;
            oldUserInfo.XAddress = step3.XAddress;
            oldUserInfo.LinkedinAddress = step3.LinkedinAddress;
            oldUserInfo.GithubAddress = step3.GithubAddress;
            oldUserInfo.MediumAddress = step3.MediumAddress;
            oldUserInfo.YoutubeAddress = step3.YoutubeAddress;
            oldUserInfo.PersonalWebAddress = step3.PersonalWebAddress;
            oldUserInfo.HighSchoolName = step3.HighSchoolName;
            oldUserInfo.HighSchoolStartYear = step3.HighSchoolStartYear;
            oldUserInfo.HighSchoolGraduationYear = step3.HighSchoolGraduationYear;
            oldUserInfo.UniversityName = step3.UniversityName;
            oldUserInfo.UniversityStartYear = step3.UniversityStartYear;
            oldUserInfo.UniversityGraduationYear = step3.UniversityGraduationYear;
            oldUserInfo.BirthDate = step3.BirthDate;
            oldUserInfo.ProfilePicture = step3.ProfilePicture;
            oldUserInfo.CoverImagePicture = step3.CoverImagePicture;
            oldUserInfo.WorkingAtLogo = step3.WorkingAtLogo;

            await _userManager.UpdateAsync(oldUserInfo);
            await _userService.SaveChangesAsync();

            try
            {
                if (request.EmailAddress != oldUserInfo.Email)
                {
                    await _userService.LogoutAsync();
                    await _userService.LogInAsync((await _userManager.FindByIdAsync(request.Id))!);

                    TempData["Succeed"] = "Profile updated successfully.";

                    return RedirectToAction(nameof(Profile), new { userName = User.Identity!.Name });
                }
                TempData["Succeed"] = "Profile updated successfully.";

                return RedirectToAction(nameof(Profile), new { userName = User.Identity!.Name });
            }
            catch (Exception)
            {
                TempData["Failed"] = "An error occurred while updating the profile.";
                return RedirectToAction(nameof(Profile), new { userName = User.Identity!.Name });
            }
        }

        public IActionResult AccessDenied()
        {
            TempData["AccessDeniedDescription"] = "You do not have permission to access this page or apply this action.";
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> NotificationSubscribe(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Failed"] = "Email is required.";
                return RedirectToAction("Index", "Home");
            }
            var result = await _userService.SubscribeToNotificationsAsync(email);
            if (result.IsSuccess)
            {
                await _emailService.SendEmailNotificationNewsletterEmailAsync(email);

                TempData["Succeed"] = "You have successfully subscribed to notifications.";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["Failed"] = "An error occurred while subscribing to notifications.";
                return RedirectToAction("Index", "Home");
            }
        }

    }
}
