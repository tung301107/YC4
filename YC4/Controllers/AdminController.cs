using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YC4.DTOs;
using YC4.Interfaces;

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Thay thế HasPermission cũ bằng Policy đã khai báo trong Program.cs
    [Authorize(Policy = "CanManageUsers")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }

        // 1. Lấy danh sách người dùng
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
            => Ok(await _userService.GetAllUsersAsync());

        // 2. Xóa người dùng
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
            => await _userService.DeleteUserAsync(id) ? Ok(new { Message = "Đã xóa thành công" }) : NotFound();

        // 3. Reset mật khẩu
        [HttpPost("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
            => await _userService.ResetPasswordAsync(id, newPassword) ? Ok(new { Message = "Đã reset mật khẩu" }) : BadRequest();

        // 4. Lấy danh sách tất cả quyền (Functions) hiện có trong hệ thống
        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions()
            => Ok(await _userService.GetAllFunctionsAsync());

        // 5. Gán quyền trực tiếp cho một người dùng (User-Specific Permission)
        [HttpPost("users/{id}/assign-permission")]
        public async Task<IActionResult> AssignToUser(int id, [FromBody] PermissionAssignmentDto req)
            => await _userService.AssignPermissionToUserAsync(id, req.PermissionId, req.PermissionCode)
                ? Ok(new { Message = "Gán quyền cho người dùng thành công" })
                : BadRequest();

        // 6. Gán quyền cho một vai trò (Role Permission)
        [HttpPost("roles/{roleId}/assign-permission")]
        public async Task<IActionResult> AssignToRole(int roleId, [FromBody] PermissionAssignmentDto req)
            => await _userService.AssignPermissionToRoleAsync(roleId, req.PermissionId, req.PermissionCode)
                ? Ok(new { Message = "Gán quyền cho Role thành công" })
                : BadRequest();

        // 7. Gán Role cho người dùng
        [HttpPost("users/{id}/assign-role/{roleId}")]
        public async Task<IActionResult> AssignRole(int id, int roleId)
            => await _userService.AssignRoleToUserAsync(id, roleId)
                ? Ok(new { Message = "Gán vai trò thành công" })
                : BadRequest();
    }
}