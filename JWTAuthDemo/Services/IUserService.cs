using JWTAuthDemo.Model;

namespace JWTAuthDemo.Services
{
    public interface IUserService
    {
        UserModel AuthenticateUser(UserModel user);
        void AddUser(UserModel newUser);
    }
}