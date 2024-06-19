using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace First.Models
{
    public class UsersDBServices
    {
        static string consStr = @"Data Source=FirstSQL.mssql.somee.com;Initial Catalog=FirstSQL;Persist Security Info=True;User ID=barkan33_SQLLogin_1;Password=u6lnbqjhb7;TrustServerCertificate=True";
        static SqlConnection connection;
        public static User Login(string username, string password)
        {
            User user = null;
            connection = new SqlConnection(consStr);

            SqlCommand command = new SqlCommand("Select * from Users where Username = @username and PasswordHash = @password ", connection);
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", PasswordHasher(password));

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new User
                    {
                        Id = (int)reader["UserID"],
                        Username = (string)reader["Username"]
                    };
                }

            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                connection.Close();
            }

            return user;
        }

        public static int Registration(string username, string password)
        {
            connection = new SqlConnection(consStr);

            //SqlCommand command = new SqlCommand("INSERT INTO Users (Id, Name, Password) VALUES (@Id, @Name, @Password)", connection);

            //command.Parameters.AddWithValue("@Id", id);
            //command.Parameters.AddWithValue("@Name", username);
            //command.Parameters.AddWithValue("@Password", password);

            SqlCommand command = new SqlCommand("AddUser", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username });
            command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarBinary, 255) { Value = PasswordHasher(password) });

            try
            {

                connection.Open();

                int userId = (int)command.ExecuteScalar();
                Console.WriteLine(userId);
                return userId;

            }
            catch (Exception e)
            {
                throw new Exception("User Already Exists");
            }
            finally
            {
                connection.Close();
            }

        }

        internal static List<User> AllUsers()
        {
            List<User> users = new List<User>();
            connection = new SqlConnection(consStr);

            SqlCommand command = new SqlCommand("Select * from Users", connection);

            try
            {

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        Id = (int)reader["UserID"],
                        Username = (string)reader["Username"]
                    });
                }

                return users;
            }
            catch (Exception e)
            {
                throw;

            }
            finally
            {
                connection.Close();
            }

        }

        internal static string DeleteUser(int id)
        {
            connection = new SqlConnection(consStr);
            SqlCommand command = new SqlCommand("delete Users where UserID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                return "deleted";
            }
            catch (Exception e)
            {
                return e.Message;
            }
            finally
            {
                connection.Close();
            }
        }

        internal static User GetUserById(int id)
        {
            connection = new SqlConnection(consStr);
            SqlCommand command = new SqlCommand("Select * from Users where UserID = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            User user = null;
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    user = new User
                    {
                        Id = (int)reader["UserID"],
                        Username = (string)reader["Username"]
                    };
                }

                return user;
            }
            catch (Exception e)
            {

                throw;
            }
        }

        private static byte[] PasswordHasher(string password)
        {
            MD5 md5 = MD5.Create();

            byte[] inputBytes = Encoding.UTF8.GetBytes(password);

            byte[] hashBytes = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            //return sb.ToString();
            return hashBytes;

        }
    }
}
