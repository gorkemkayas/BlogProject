using BlogProject.Application.DTOs;
using BlogProject.Application.Interfaces;
using BlogProject.Domain.Entities;
using BlogProject.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogProject.Controllers
{
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;

        public CategoryController(ICategoryService categoryService, IPostService postService)
        {
            _categoryService = categoryService;
            _postService = postService;
        }



        //[Route("{category:string}")]
        public IActionResult Index()
        {
            //if(string.IsNullOrEmpty(category))
            //{
            //    TempData["Failed"] = "Category not found.";
            //    return NotFound();
            //}
            //var selectedCategory = _context.Categories.FirstOrDefault(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
            //if(selectedCategory is null)
            //{
            //    TempData["Failed"] = "Category not found.";
            //    return NotFound();
            //}
            //var posts = _context.Posts
            //    .Where(p => p.CategoryId == selectedCategory.Id && p.IsDeleted!)
            //    .OrderByDescending(p => p.CreatedTime)
            //    .ToList();

            //return View(posts);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Category(string id)
        {
            try
            {
            int scrollPageSize = 3;

            var category = await _categoryService.GetByIdAsync(id);
            ICollection<PostListItemDto> posts = await _postService.GetByCategoryIdAsync(id) ?? new List<PostListItemDto>();
            ICollection<PostDto> mostLikedPosts = await _postService.GetCategorizedPostsByLikeCountsAsync(true,id) ?? new List<PostDto>();
            ICollection<PostWithCategoryNameDto> mostViewedPosts = await _postService.GetMostViewedPostsByCategoryAsync(category.Name, true) ?? new List<PostWithCategoryNameDto>();
            var relatedCategories = await _categoryService.GetRelatedCategoriesAsync(id);
            ViewBag.CategoryId = id;

            var viewModel = new CategoryViewModel
            {
                Category = category,
                Posts = posts.ToList(),
                RelatedCategories = relatedCategories,
                TotalViews = posts.Sum(p => p.ViewCount),
                AuthorCount = posts.Select(p => p.AuthorId).Distinct().Count(),
                MostViewedPosts = mostViewedPosts.ToList(),
                MostLikedPosts =  mostLikedPosts.ToList()
            };

            return View(viewModel);

            }
            catch (Exception ex)
            {

                throw new Exception($"HATA: {ex.Message}\nHATA STACKTRACE: {ex.StackTrace}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPostCountsByDate(string categoryId)
        {
            var chartData = await _categoryService.GetDailyPostCountsAsync(categoryId);
            return Json(chartData);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreMostLikedPostScrollPosts(int page, string? categoryId)
        {
            Console.WriteLine($"Page: {page}, CategoryId: {categoryId}");
            int pageSize = 3;

            var posts = await _postService.LoadMoreMostLikedPostScrollPosts(page, pageSize, categoryId);

            if (posts is null || !posts.Any())
                return Content("");

            return PartialView("_MostLikedPostsPartial", posts);

        }
        [HttpGet]
        public async Task<IActionResult> GetSliderPosts(int page, string categoryId)
        {
            int pageSize = 3;

            var posts = await _postService
                .GetMostViewedPostsByCategoryIdAsync(categoryId, true);

            Console.WriteLine("GetSliderPosts metodundan gelen gönderilerin bilgileri :\n");
            //foreach (var item in posts)
            //{
            //    Console.WriteLine($"PostId {item.Id}\n CategoryId {item.Category.Id}\n PostName {item.Title} \n CategoryName {item.Category.Name}");
            //}
            var sliderPosts = posts
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();


            if (!sliderPosts.Any())
                return Content(""); // sonuna geldiysek boş dön

            return PartialView("_SliderPostPartial", sliderPosts); // Razor partial view
        }

    }
}
