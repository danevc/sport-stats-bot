using System;
using System.Configuration;
using System.Data.SqlClient;

using Stats.Models;

namespace Stats.Sql
{
    public static class SqlHelper
    {
        private static readonly string conString = ConfigurationManager.AppSettings["ConnectionString"];

        private static readonly string getUser_sqlquery = @"SELECT TOP 1 Id, UserName, FirstName FROM UserTelegram WHERE Id = @Id;";

        private static readonly string addUser_sqlquery = @"INSERT INTO UserTelegram (Id, UserName, FirstName, CreatedOn)
                                                            VALUES (@Id, @UserName, @FirstName, GETDATE());";

        public static User GetUser(long Id)
        {
            User user = null;

            try 
            { 
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(getUser_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user = new User()
                            {
                                Id = (int)reader[0],
                                UserName = (string)reader[1],
                                FirstName = (string)reader[2],
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            return user;
        }

        public static void AddUser(User user)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(conString))
                {
                    connection.Open();
                    var command = new SqlCommand(addUser_sqlquery, connection);

                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@UserName", user.UserName);
                    command.Parameters.AddWithValue("@FirstName", user.FirstName);

                    int rowsAffected = command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            
        }
    }
}
