using System.Diagnostics;
using BlogProject.Application.DTOs;
using BlogProject.Application.Interfaces;
using BlogProject.Domain.Entities;
using BlogProject.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Controllers
{
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;


        public HomeController(ILogger<HomeController> logger, IPostService postService, ITagService tagService, IUserService userService)
        {
            _logger = logger;
            _postService = postService;
            _tagService = tagService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            if (!Request.Cookies.ContainsKey("FirstVisit"))
            {
                ViewBag.ShowWelcomeMessage = true;
                Response.Cookies.Append("FirstVisit", "true", new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }
            else
            {
                ViewBag.ShowWelcomeMessage = false;
            }

            // Task'ları başlatıyoruz
            var mostViewedTask = _postService.GetMostViewedPostsWithCount(5);
            var latestPostsTask = _postService.GetLatestPostsWithCount();
            var popularTagsTask = _tagService.GetPopularTags(15);
            var mostContributorsTask = _userService.MostContributors(3);
            var mostReadThisWeekTask = _postService.GetMostViewedPostsWithCount(5, currentWeek: true);

            try
            {
                // Tüm sorguları paralel bekliyoruz
                await Task.WhenAll(mostViewedTask, latestPostsTask, popularTagsTask, mostContributorsTask, mostReadThisWeekTask);
            }
            catch (Exception ex)
            {
                // Hata varsa logluyoruz, ama işlemin devam etmesini sağlıyoruz
                // Burada dilersen ayrıntılı logging yapabilirsin
                Console.WriteLine($"Index metodu sorgu hatası: {ex.Message}");
            }

            // Task.Result çağırmadan önce Task tamamlandı, burası güvenli
            var model = new IndexViewModel
            {
                MostViewedPosts = mostViewedTask.IsCompletedSuccessfully ? mostViewedTask.Result : new List<PostDto>(),
                LatestPosts = latestPostsTask.IsCompletedSuccessfully ? latestPostsTask.Result : new List<PostDto>(),
                PopularTags = popularTagsTask.IsCompletedSuccessfully ? popularTagsTask.Result : new List<TagEntity>(),
                MostContributors = mostContributorsTask.IsCompletedSuccessfully ? mostContributorsTask.Result : new List<ContributorDto>(),
                MostReadPostsThisWeek = mostReadThisWeekTask.IsCompletedSuccessfully ? mostReadThisWeekTask.Result : new List<PostDto>()
            };

            return View(model);
        }



        [Route("/development-updates")]
        public async Task<IActionResult> DevelopmentUpdates()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult TestViewComponent()
        {
            return ViewComponent("SuggestedUsers");
        }

    }
}
