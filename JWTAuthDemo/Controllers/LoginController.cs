using JWTAuthDemo.Model;
using JWTAuthDemo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        [Authorize]
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
        [HttpGet("GetValue")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value 1", "value 2", "value 3" };
        }


        [Authorize]
        [HttpGet]
        public ActionResult<UserModel> GetUserInfo()
        {
            return _userService.GetUserInfo(HttpContext.User.Identity as ClaimsIdentity);
        }

        [HttpDelete]
        public ActionResult RemoveUser(string username, string password)
        {
            ActionResult result = Ok();
            try
            {
                _userService.RemoveUser(username, password);
            }
            catch (Exception)
            {
                result = NotFound();
            }

            return result;
        }

        [Authorize]
        [HttpDelete]
        public ActionResult RemoveThisUser()
        {
            ActionResult result = Ok();
            try
            {
                _userService.RemoveUser(HttpContext.User.Identity as ClaimsIdentity);
            }
            catch (Exception)
            {
                result = NotFound();
            }

            return result;
        }

        [Authorize]
        [HttpPatch]
        public ActionResult<UserModel> EditUser(UserModel updatInfo)
        {
            ActionResult result = null;
            UserModel userUpdated = null;
            try
            {
                userUpdated = _userService.TryUpdateUser(updateInfo);
            }
            catch (Exception)
            {
                result = NotFound();
            }

            return result;
        }


    }
}