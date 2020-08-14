using JWTAuthDemo.Model;
using Microsoft.Extensions.Configuration;

namespace JWTAuthDemo.Services
{
    public class MockUserService
    {
        private readonly IConfiguration _config;
        private readonly string _connString;
        public MockUserService(IConfiguration config)
        {
            _config = config;
            _connString = _config.GetConnectionString("DefaultConnection");
        }

        public UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;

            if (login.UserName == "Marko" && login.Password == "1234")
            {
                user = new UserModel()
                {
                    UserName = "Marko",
                    EmailAddress = "marko@markovic.com",
                    Password = "1234",
                };
            }

            return user;
        }
    }
}