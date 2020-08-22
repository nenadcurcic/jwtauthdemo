using JWTAuthDemo.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JWTAuthDemo.Services
{
    public interface IUserService
    {
        UserModel AuthenticateUser(UserModel user);
        void AddUser(UserModel newUser);
        ActionResult<UserModel> GetUserInfo(ClaimsIdentity claimsIdentity);
        void RemoveUser(string username, string password);
        void RemoveUser(ClaimsIdentity claimsIdentity);
        UserModel TryUpdateUser(object updateInfo);
    }
}