using BlogProject.Application.Interfaces;
using BlogProject.Domain.Entities;
using BlogProject.Web.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;

namespace BlogProject.Controllers
{
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class CommentController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ICommentService _commentService;

        public CommentController(UserManager<AppUser> userManager, ICommentService commentService)
        {
            _userManager = userManager;
            _commentService = commentService;
        }
        [HttpPost("Comment/AddComment")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddComment([FromBody] AddCommentViewModel model)
        {

            var commentEntity = new CommentEntity
            {
                AuthorId = Guid.Parse(model.AuthorId),
                PostId = Guid.Parse(model.PostId!),
                Content = model.Comment,
                ParentCommentId = string.IsNullOrEmpty(model.ParentCommentId) ? null : Guid.Parse(model.ParentCommentId)
            };

            var addedComment = await _commentService.AddCommentAsync(commentEntity);
            if (!addedComment.IsSuccess)
                throw new Exception("Comment could not be added. Please try again later.");
            var author = await _userManager.FindByIdAsync(model.AuthorId);
            var profilePhotoUrl = author.ProfilePicture;
            var url = $"/img/userPhotos/{author.UserName}/{profilePhotoUrl}";
            return Json(new
            {
                Success = true,
                Comment = model.Comment,
                AuthorName = author.FullName,
                AuthorProfileUrl = url,
                CreatedDate = DateTime.Now.ToString("dddd d, yyyy", CultureInfo.InvariantCulture)
            });
        }
    }
}
