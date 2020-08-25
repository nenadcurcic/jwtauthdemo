using JWTAuthDemo.Helpers;
using JWTAuthDemo.Model;
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
                    cmd.CommandText = $"SELECT Id FROM Users WHERE Username = '{newUser.UserName}'";
                    int newUserID = (int)cmd.ExecuteScalar();

                    cmd.CommandText = "INSERT INTO Roles (Role, UserId)" +
                        $"VALUES('{newUser.Role}', '{newUserID}')";

                    cmd.ExecuteNonQuery();
                    con.Close();
                }
            }
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
                    result = GetCurrentUserData(username, cmd);
                    con.Close();
                }
            }
            return result;
        }

        public bool RemoveUser(string username, string password)
        {
            bool res = false;
            UserModel userToDelete = new UserModel()
            {
                Password = password,
                UserName = username,
            };

            if (AuthenticateUser(userToDelete) != null)
            {
                res = DeleteUserByUsername(username) > 0;
            }
            return res;
        }

        public bool RemoveUser(ClaimsIdentity claimsIdentity)
        {
            string username = _tokenService.GetUserNameFromToken(claimsIdentity);
            return DeleteUserByUsername(username) > 0;
        }

        public UserModel TryUpdateUser(UserModel updateInfo)
        {
            UserModel updatedData;
            UserModel oldData;
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                con.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                   // SqlTransaction transaction = con.BeginTransaction();
                    cmd.Connection = con;

                    oldData = GetCurrentUserData(updateInfo.UserName, cmd);

                    con.Close();
                }
            }
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                con.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    updatedData = UpdateUserDataDifs(updateInfo, oldData);

                    CommitUpdatedDatainDb(updatedData, cmd);


                    con.Close();
                }
            }
            return updatedData;
        }

        private void CommitUpdatedDatainDb(UserModel updatedData, SqlCommand cmd)
        {
            SqlTransaction transaction = cmd.Connection.BeginTransaction();

            cmd.Transaction = transaction;

            try
            {
                cmd.CommandText = $"UPDATE Users SET  Email = '{updatedData.EmailAddress}', FirstName = '{updatedData.FirstName}', LastName = '{updatedData.LastName}' WHERE Id = {updatedData.Id}";
                cmd.ExecuteNonQuery();
                cmd.CommandText = $"UPDATE Roles SET Role = '{updatedData.Role}' WHERE UserID = '{updatedData.Id}'";
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
            }
        }

        private UserModel GetCurrentUserData(string username, SqlCommand cmd)
        {
            UserModel result = null;
            cmd.CommandText = $"SELECT * FROM UserInfo WHERE Username='{username}'";
            SqlDataReader rd = cmd.ExecuteReader();
            if (rd.Read())
            {
                result = new UserModel()
                {
                    Id = rd.GetInt32(0),
                    UserName = rd["Username"].ToString().Trim(),
                    EmailAddress = rd["Email"].ToString().Trim(),
                    FirstName = rd["FirstName"].ToString().Trim(),
                    LastName = rd["LastName"].ToString().Trim(),
                    Role = rd["Role"].ToString().Trim(),
                };
            }

            return result;
        }

        private UserModel UpdateUserDataDifs(UserModel newUserData, UserModel oldUserData)
        {
            UserModel res = new UserModel()
            {
                Id = oldUserData.Id,
                UserName = oldUserData.UserName,
                EmailAddress = newUserData.EmailAddress == null ? oldUserData.EmailAddress : newUserData.EmailAddress,
                FirstName = newUserData.FirstName == null ? oldUserData.FirstName : newUserData.FirstName,
                LastName = newUserData.LastName == null ? oldUserData.LastName : newUserData.LastName,
                Role = newUserData.Role == null ? oldUserData.Role : newUserData.Role,
            };

            return res;
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

        private int DeleteUserByUsername(string username)
        {
            int affected = 0;
            using (SqlConnection con = new SqlConnection())
            {
                con.ConnectionString = _connString;
                con.Open();
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = $"DELETE Users WHERE Username = '{username}'";
                    affected = cmd.ExecuteNonQuery();
                    con.Close();
                }
            }

            return affected;
        }
    }
}