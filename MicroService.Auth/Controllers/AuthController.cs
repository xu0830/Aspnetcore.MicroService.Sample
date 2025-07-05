using MicroService.Auth.Service.IService;
using MicroService.Models.DTOS;
using MicroService.Models.Entity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace MicroService.Auth.Controllers
{
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        private readonly IConfiguration _configuration;

        private readonly HttpClient _httpClient;

        public AuthController(ITokenService tokenService, IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _tokenService = tokenService;
            _configuration = configuration;
            _httpClient = clientFactory.CreateClient("DiscoveryRandom");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _httpClient.PostAsync("http://user-service/user/loginGetUser", new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));

            //var result = await _clientFactory.CreateClient("user").PostAsync("/user/loginGetUser", JsonContent.Create(model, new MediaTypeHeaderValue("application/json")));
            var user = new SysUser()
            {
                Name = "",
                Account = "",
                Password = ""
            };
            if (user == null)
                return Unauthorized("Invalid username or password");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(ClaimTypes.Role, user.Role)
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 保存 refresh token (实际项目中应持久化存储)
            //await _userService.SaveRefreshToken(user.Id, refreshToken);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = DateTime.UtcNow.AddMinutes(
                    _configuration.GetValue<double>("Jwt:AccessTokenExpiration"))
            });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] RefreshTokenModel model)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);
            var username = principal?.Identity?.Name;

            //var user = await _userService.GetUserByUsername(username);

            var user = new UserInfo()
            {
                Name = "xcj",
                Account = "xx@163.com",
                Password = "123456",
                //Phone = "1342",
                //Role = "normal"
            };

            if (user == null || user.RefreshToken != model.RefreshToken ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return BadRequest("Invalid token");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            //await _userService.SaveRefreshToken(user.Id, newRefreshToken);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //await _userService.RevokeRefreshToken(userId);
            return Ok("Logged out");
        }
    }
}
