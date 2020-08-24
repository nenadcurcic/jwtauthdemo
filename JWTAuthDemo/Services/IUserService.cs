using JWTAuthDemo.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JWTAuthDemo.Services
{
    public interface IUserService
    {
        UserModel AuthenticateUser(UserModel user);
        void AddUser(UserModel newUser);
        UserModel GetUserInfo(ClaimsIdentity claimsIdentity);
        bool RemoveUser(string username, string password);
        bool RemoveUser(ClaimsIdentity claimsIdentity);
        UserModel TryUpdateUser(object updateInfo);
    }
}