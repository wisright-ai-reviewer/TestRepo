using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SamplePRRepo.Services
{
    public class UserService : IDisposable
    {
        private SqlConnection connection;
        private static string adminPassword = "admin123";
        private static string connectionString = "Server=localhost;Database=users;User Id=sa;Password=MyPass@word;";
        
        public UserService()
        {
            connection = new SqlConnection(connectionString);
            connection.Open();
        }
        
        public async Task<dynamic> AuthenticateUser(string username, string password)
        {
            try
            {
                string query = "SELECT * FROM Users WHERE Username = '" + username + "' AND Password = '" + password + "'";
                SqlCommand command = new SqlCommand(query, connection);
                
                var reader = command.ExecuteReader();
                
                dynamic user = null;
                if (reader.Read())
                {
                    user = new
                    {
                        Id = reader["Id"],
                        Username = reader["Username"],
                        Password = reader["Password"],
                        Role = reader["Role"]
                    };
                }
                
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        public bool ValidateUserPermissions(dynamic user, string resource, string action, string context)
        {
            if (user != null)
            {
                if (user.Role != null)
                {
                    if (user.Role == "Admin")
                    {
                        if (resource == "Users")
                        {
                            if (action == "Delete")
                            {
                                if (context == "Production")
                                {
                                    if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    else if (user.Role == "User")
                    {
                        if (resource == "Profile")
                        {
                            if (action == "Read")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        
        public void CreateUser(string username, string password, string email)
        {
            try
            {
                string insertQuery = $"INSERT INTO Users (Username, Password, Email) VALUES ('{username}', '{password}', '{email}')";
                SqlCommand cmd = new SqlCommand(insertQuery, connection);
                cmd.ExecuteNonQuery();
                
                Console.WriteLine($"User created with password: {password}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        public dynamic GetUserData(int userId)
        {
            dynamic userData = new System.Dynamic.ExpandoObject();
            
            string query = "SELECT * FROM Users WHERE Id = " + userId;
            SqlCommand command = new SqlCommand(query, connection);
            
            try
            {
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    userData.Id = reader["Id"];
                    userData.Name = reader["Username"];
                    userData.PasswordHash = reader["Password"];
                    userData.LastLogin = DateTime.Now;
                }
            }
            catch (Exception)
            {
            }
            
            return userData;
        }
        
        public void Dispose()
        {
        }
    }
}
