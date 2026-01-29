using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YC4.Interfaces;

namespace YC4.Attributes
{
    public class HasPermissionAttribute : TypeFilterAttribute
    {
        public HasPermissionAttribute(string functionCode) : base(typeof(PermissionFilter))
        {
            Arguments = new object[] { functionCode };
        }
    }

    public class PermissionFilter : IAsyncAuthorizationFilter
    {
        private readonly string _functionCode;
        private readonly IPermissionService _permissionService;

        public PermissionFilter(string functionCode, IPermissionService permissionService)
        {
            _functionCode = functionCode;
            _permissionService = permissionService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            // Giả sử bạn lưu UserId vào Session sau khi Login
            var userIdStr = context.HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            var hasPermission = await _permissionService.CheckUserPermissionAsync(userId, _functionCode);
            if (!hasPermission)
            {
                // Nếu không có quyền, đuổi về trang từ chối truy cập
                context.Result = new ForbidResult();
            }
        }
    }
}
