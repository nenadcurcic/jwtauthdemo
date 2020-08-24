using JWTAuthDemo.Helpers;
using JWTAuthDemo.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace JWTAuthDemo.Services
{
    public class UserService : IUserService
    {
        private readonly IConfiguration _config;
        private readonly string _connString;
        public IJWTTokenService _tokenService { get; set; }

        public UserService(IConfiguration config, IJWTTokenService tokenService)
        {
            _config = config;
            _connString = _config.GetConnectionString("DefaultConnection");
            _tokenService = tokenService;
        }

        public UserModel AuthenticateUser(UserModel user)
        {
            UserModel result = null;
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                con.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = $"SELECT * FROM Users WHERE Username='{user.UserName}'";
                    SqlDataReader rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        result = new UserModel()
                        {
                            UserName = rd["Username"].ToString(),
                            EmailAddress = rd["Email"].ToString(),
                            Password = rd["Password"].ToString()
                        };
                        if (!PaasswordHashing.Verify(result.Password, user.Password))
                        {
                            result = null;
                        }
                    }
                    con.Close();
                }
            }
            return result;
        }

        public void AddUser(UserModel newUser)
        {
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                using (SqlCommand cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                    if (CheckIfUserExists(newUser.UserName, cmd))
                    {
                        throw new DuplicateNameException();
                    }
                    con.Close();
                }
                using (SqlCommand cmd = new SqlCommand())
                {
                    con.Open();
                    cmd.Connection = con;
                  
                    cmd.CommandText = "INSERT INTO Users (Username, Email, Password, FirstName, LastName)" +
                    $" VALUES ('{newUser.UserName}', '{newUser.EmailAddress}', '{PaasswordHashing.HashPassword(newUser.Password)}', '{newUser.FirstName}', '{newUser.LastName}')";
                      
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        private bool CheckIfUserExists(string userName, SqlCommand cmd)
        {
            bool result = false;
            
            cmd.CommandText = $"SELECT * FROM Users WHERE Username='{userName}'";
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                result = true;
            }
                          
            return result;
        }

        public UserModel GetUserInfo(ClaimsIdentity claimsIdentity)
        {
            string username = _tokenService.GetUserNameFromToken(claimsIdentity);

            UserModel result = null;
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                con.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = $"SELECT * FROM UserInfo WHERE Username='{username}'";
                    SqlDataReader rd = cmd.ExecuteReader();
                    if (rd.Read())
                    {
                        result = new UserModel()
                        {
                            UserName = rd["Username"].ToString().Trim(),
                            EmailAddress = rd["Email"].ToString().Trim(),
                            FirstName = rd["FirstName"].ToString().Trim(),
                            LastName = rd["LastName"].ToString().Trim(),
                            Role = rd["Role"].ToString().Trim(),
                        };
                      
                    }
                    con.Close();
                }
            }
            return result;
        }

        public void RemoveUser(string username, string password)
        {
            UserModel userToDelete = new UserModel()
            {
                Password = password,
                UserName = username,
            };

            if (AuthenticateUser(userToDelete) != null)
            {
                DeleteUserByUsername(username);
            }

        }


        public void RemoveUser(ClaimsIdentity claimsIdentity)
        {
            string username = _tokenService.GetUserNameFromToken(claimsIdentity);
            DeleteUserByUsername(username);
        }

        public UserModel TryUpdateUser(object updateInfo)
        {
            throw new NotImplementedException();
        }

        private void DeleteUserByUsername(string username)
        {
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                con.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = $"DELETE Users WHERE Username = '{username}''";
                    cmd.ExecuteNonQuery();

                    con.Close();
                }
            }
        }
    }
}