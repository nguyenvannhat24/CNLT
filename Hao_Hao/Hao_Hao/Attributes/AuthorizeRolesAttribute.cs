using Hao_Hao.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Text.Json;

namespace Hao_Hao.Attributes
{
    public class AuthorizeRolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRolesAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userInfoJson = context.HttpContext.Request.Cookies["UserInfo"];

            if (string.IsNullOrEmpty(userInfoJson))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            try
            {
                var user = JsonSerializer.Deserialize<UserInfo>(userInfoJson);

                if (user == null || user.UserRole == null || !_roles.Intersect(user.UserRole).Any())
                {
                    context.Result = new RedirectToActionResult("NotRolers", "Home", null);
                }
            }
            catch (JsonException)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
            }
        }
    }
}
    