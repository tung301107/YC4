using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using YC4.DTOs;
using YC4.Interfaces;

namespace YC4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // Bảo vệ toàn bộ các endpoint bằng quyền ADMIN
    [Authorize(Policy = "CanManageUsers")]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;
        public AdminController(IUserService userService) => _userService = userService;

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers() => Ok(await _userService.GetAllUsersAsync());

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
            => await _userService.DeleteUserAsync(id) ? Ok("Đã xóa") : NotFound();

        [HttpPost("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] string newPassword)
            => await _userService.ResetPasswordAsync(id, newPassword) ? Ok("Thành công") : BadRequest();

        [HttpGet("permissions")]
        public async Task<IActionResult> GetPermissions() => Ok(await _userService.GetAllFunctionsAsync());

        // Các phương thức gán quyền cũng được bảo vệ bởi Policy "CanManageUsers"
        [HttpPost("users/{id}/assign-permission")]
        public async Task<IActionResult> AssignToUser(int id, [FromBody] PermissionAssignmentDto req)
            => await _userService.AssignPermissionToUserAsync(id, req.PermissionId, req.PermissionCode) ? Ok("Thành công") : BadRequest();

        [HttpPost("roles/{roleId}/assign-permission")]
        public async Task<IActionResult> AssignToRole(int roleId, [FromBody] PermissionAssignmentDto req)
            => await _userService.AssignPermissionToRoleAsync(roleId, req.PermissionId, req.PermissionCode) ? Ok("Thành công") : BadRequest();

        [HttpPost("users/{id}/assign-role/{roleId}")]
        public async Task<IActionResult> AssignRole(int id, int roleId)
            => await _userService.AssignRoleToUserAsync(id, roleId) ? Ok("Thành công") : BadRequest();
    }
}