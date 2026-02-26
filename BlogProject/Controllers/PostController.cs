using AutoMapper;
using BlogProject.Application.DTOs;
using BlogProject.Application.Interfaces;
using BlogProject.Domain.Entities;
using BlogProject.Infrastructure.Persistence;
using BlogProject.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogProject.Controllers
{
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class PostController : Controller
    {
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly ITagService _tagService;
        private readonly ICategoryService _categoryService;
        private readonly ICommentService _commentService;
        private readonly IWebHostEnvironment _env;

        private readonly BlogDbContext _context;

        private readonly IMapper _mapper;

        public PostController(IPostService postService, ITagService tagService, ICategoryService categoryService, ICommentService commentService, IWebHostEnvironment env, IMapper mapper, IUserService userService, BlogDbContext context)
        {
            _postService = postService;
            _tagService = tagService;
            _categoryService = categoryService;
            _commentService = commentService;
            _env = env;
            _mapper = mapper;
            _userService = userService;
            _context = context;
        }


        public IActionResult Index()
        {

            return View();
        }

        [HttpGet("Post/NewPost")]
        public async Task<IActionResult> NewPost()
        {
            var availableCategories = _categoryService.GetAllCategorySelectList();
            var availableTags = _tagService.GetAllTagSelectList();
            List<SelectListItem>? selectCategoriesList = availableCategories.Data?.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value,
                Selected = x.Selected
            }).ToList();
            List<SelectListItem>? selectTagsList = availableTags.Data?.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value,
                Selected = x.Selected
            }).ToList();
            var model = new CreatePostViewModel
            {
                // Kategorileri veritabanından veya servisten doldurun
                AvailableCategories = (selectCategoriesList is null) ? new List<SelectListItem>() : selectCategoriesList,

                // Etiketleri veritabanından veya servisten doldurun
                AvailableTags = (selectTagsList is null) ? new List<SelectListItem>() : selectTagsList
            };

            return View(model);
        }

        [HttpPost]

        public async Task<IActionResult> NewPost(CreatePostViewModel model)
        {
            if (ModelState.IsValid)
            {
                var mappedResult = _mapper.Map<CreatePostDto>(model);
                // Post ekleme işlemi
                var result = await _postService.CreatePostAsync(mappedResult);
                if (result.IsSuccess)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors!)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }


            var availableCategories = _categoryService.GetAllCategorySelectList();
            var availableTags = _tagService.GetAllTagSelectList();
            List<SelectListItem>? selectCategoriesList = availableCategories.Data?.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value,
                Selected = x.Selected
            }).ToList();
            List<SelectListItem>? selectTagsList = availableTags.Data?.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value,
                Selected = x.Selected
            }).ToList();

            // Kategorileri ve etiketleri tekrar doldurun
            model.AvailableCategories = (selectCategoriesList is null) ? new List<SelectListItem>() : selectCategoriesList;
            model.AvailableTags = (selectTagsList is null) ? new List<SelectListItem>() : selectTagsList;
            return View(model);
        }

        [HttpGet("Post/{id}")]
        public async Task<IActionResult> PostDetails(string id)
        {
            if (!Guid.TryParse(id, out var postId))
                return BadRequest("Geçersiz gönderi kimliği.");

            // Gönderiyi getir (DTO formatında, includeAuthor: true)
            var post = await _postService.GetPostByIdAsync(postId, true);
            if (post is null)
                return NotFound();

            // Önerilen gönderiler
            var recommendedPosts = await _postService.GetLatestPostsWithCount(3);

            // Kullanıcı bilgisi
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? username = User.Identity?.Name;

            // Beğenme kontrolü
            bool isLiked = false;
            if (isAuthenticated && userId is not null)
            {
                isLiked = await _postService.IsPostLikedByCurrentUserAsync(userId, id);
            }

            // Giriş yapan kullanıcının bilgisi (profil fotoğrafı için)
            CurrentUserDto? currentUser = null;
            if (isAuthenticated && username is not null)
            {
                currentUser = await _userService.FindByUsernameWithDto(username);
            }

            // Yorumları getir (DTO'ya projeksiyon yapmış halini varsayıyoruz)
            var comments = await _commentService.GetCommentsByPostIdAsync(id); // returns List<CommentViewModel>

            // ViewModel oluştur
            var model = new PostDetailsViewModel
            {
                Post = post,                             // PostDto
                RecommendedPosts = recommendedPosts,     // List<PostDto>
                Comments = comments,                     // List<CommentViewModel>
                CurrentUser = currentUser,               // CurrentUserDto
                IsLikedFromCurrentUser = isLiked
            };

            return View(model);
        }


        [HttpGet("Post/ListByCategory")]
        public IActionResult ListByCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike([FromBody] LikeToggleRequestViewModel request)
        {
            if (!Guid.TryParse(request.PostId, out var postId))
                return BadRequest("Geçersiz PostId");

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            bool liked;

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                liked = false;
            }
            else
            {
                _context.Likes.Add(new LikeEntity
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedTime = DateTime.UtcNow
                });
                liked = true;
            }

            await _context.SaveChangesAsync();

            var likeCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            return Json(new
            {
                liked,
                likeCount
            });
        }

    }
}
