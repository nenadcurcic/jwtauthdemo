using JWTAuthDemo.Model;
using JWTAuthDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;

namespace JWTAuthDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IJWTTokenService _jwtService;

        public LoginController(IConfiguration config, IUserService userService, IJWTTokenService jwtService)
        {
            _config = config;
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpGet]
        public IActionResult Login(string username, string pass)
        {
            UserModel login = new UserModel()
            {
                UserName = username,
                Password = pass
            };

            IActionResult response = Unauthorized();

            //TODO: throw different exception if user is non existing or password not valid
            var user = _userService.AuthenticateUser(login);

            if (user != null)
            {
                var tokenStr = _jwtService.GenerateJSONWebToken(user);
                response = Ok(new { token = tokenStr });
            }

            return response;
        }

        [Authorize]
        [HttpPost("Post")]
        public string Post()
        {
            return "Welcome " + _jwtService.GetUserNameFromToken(HttpContext.User.Identity as ClaimsIdentity);
        }

        [HttpPost]
        [Route("adduser")]
        public IActionResult AddUser([FromBody] UserModel newUser)
        {
            IActionResult result;
            if (!string.IsNullOrEmpty(newUser.UserName) && !string.IsNullOrEmpty(newUser.Password) && !string.IsNullOrEmpty(newUser.EmailAddress))
            {
                try
                {
                    _userService.AddUser(newUser);
                    result = new StatusCodeResult(200);
                }
                catch (Exception)
                {
                    result = new StatusCodeResult(409);
                }
            }
            else
            {
                result = BadRequest();
            };

            return result;
        }

        [Authorize]
        [HttpGet]
        [Route("userinfo")]
        public ActionResult<UserModel> GetUserInfo()
        {
            var result = _userService.GetUserInfo(HttpContext.User.Identity as ClaimsIdentity);
            return Ok(result);
        }

        [Route("remove")]
        [HttpDelete]
        public ActionResult RemoveUser(string username, string password)
        {
            bool result = _userService.RemoveUser(username, password);
            if (result)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [Route("removethis")]
        [HttpDelete]
        public ActionResult RemoveThisUser()
        {
            bool res = _userService.RemoveUser(HttpContext.User.Identity as ClaimsIdentity);
            if (res)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("edituser")]
        public ActionResult<UserModel> EditUser([FromBody] UserModel updateInfo)
        {
            updateInfo.UserName = _jwtService.GetUserNameFromToken(HttpContext.User.Identity as ClaimsIdentity);

            UserModel userUpdated;
            try
            {
                userUpdated = _userService.TryUpdateUser(updateInfo);
            }
            catch (Exception)
            {
                return NotFound();
            }

            return userUpdated;
        }
    }
}