
using Microsoft.AspNetCore.Mvc;
using ProjectManager.Api.DTOs;
using ProjectManager.Api.Services;

namespace ProjectManager.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            try
            {
                var res = await _auth.Register(dto);
                return Ok(res);
            }
            catch (Exception ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            try
            {
                var res = await _auth.Login(dto);
                return Ok(res);
            }
            catch (Exception ex) { return Unauthorized(new { error = ex.Message }); }
        }
    }
}
