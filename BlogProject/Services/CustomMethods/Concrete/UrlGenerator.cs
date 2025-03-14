﻿using BlogProject.Services.CustomMethods.Abstract;
using BlogProject.src.Infra.Entitites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace BlogProject.Services.CustomMethods.Concrete
{
    public class UrlGenerator : IUrlGenerator
    {
        private readonly IUrlHelper _urlHelper;

        private readonly IHttpContextAccessor _httpContextAccessor;
        public UrlGenerator(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IHttpContextAccessor httpContextAccessor)
        {
            _urlHelper = urlHelperFactory.GetUrlHelper(actionContextAccessor.ActionContext);
            _httpContextAccessor = httpContextAccessor;
        }
        public string GenerateResetPasswordUrl(AppUser user, string _passwordResetToken)
        {
            var passwordResetLink = _urlHelper.Action("ResetPassword","User",new { userId = user.Id, token = _passwordResetToken}, _httpContextAccessor.HttpContext.Request.Scheme);

            return passwordResetLink!;
        }
    }
}

//new UrlActionContext
//{
//    Action = "ResetPassword",
//    Controller = "User",
//    Values = new { userId = user.Id, token = _passwordResetToken },
//    Protocol = "https"
//}
