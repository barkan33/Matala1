//using Matala1.Models.Entitys;
//using Microsoft.Data.SqlClient;
//using System.Data;
//using System.Security.Cryptography;
//using System.Text;

//namespace First.Models
//{
//    public class UsersDBServices
//    {
//        //static string consStr = @"Data Source=FirstSQL.mssql.somee.com;Initial Catalog=FirstSQL;Persist Security Info=True;User ID=barkan33_SQLLogin_1;Password=u6lnbqjhb7;TrustServerCertificate=True";


//        static SqlConnection connection;
//        public User Login(string email, string password)
//        {
//            User user = null;
//            connection = new SqlConnection(consStr);

//            SqlCommand command = new SqlCommand("Select * from Users where Username = @username and PasswordHash = @password ", connection);
//            command.Parameters.AddWithValue("@username", email);
//            command.Parameters.AddWithValue("@password", PasswordHasher(password));

//            try
//            {
//                connection.Open();
//                SqlDataReader reader = command.ExecuteReader();
//                if (reader.Read())
//                {
//                    user = new User
//                    {
//                        Id = (int)reader["UserID"],
//                        Username = (string)reader["Username"]
//                    };
//                }

//            }
//            catch (Exception)
//            {
//                throw;
//            }
//            finally
//            {
//                connection.Close();
//            }

//            return user;
//        }
//        public User Login(int id, string password)
//        {
//            User user = null;
//            connection = new SqlConnection(consStr);

//            SqlCommand command = new SqlCommand("Select * from Users where Username = @username and PasswordHash = @password ", connection);
//            command.Parameters.AddWithValue("@id", id);
//            command.Parameters.AddWithValue("@password", PasswordHasher(password));

//            try
//            {
//                connection.Open();
//                SqlDataReader reader = command.ExecuteReader();
//                if (reader.Read())
//                {
//                    user = new User
//                    {
//                        Id = (int)reader["UserID"],
//                        Username = (string)reader["Username"]
//                    };
//                }

//            }
//            catch (Exception)
//            {
//                throw;
//            }
//            finally
//            {
//                connection.Close();
//            }

//            return user;
//        }

//        public int Registration(User user)
//        {
//            connection = new SqlConnection(consStr);


//            SqlCommand command = new SqlCommand("AddUser", connection);
//            command.CommandType = CommandType.StoredProcedure;
//            command.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username });
//            command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.VarBinary, 255) { Value = PasswordHasher(password) });

//            try
//            {

//                connection.Open();

//                int userId = (int)command.ExecuteScalar();
//                Console.WriteLine(userId);
//                return userId;

//            }
//            catch (Exception e)
//            {
//                throw new Exception("User Already Exists");
//            }
//            finally
//            {
//                connection.Close();
//            }

//        }

//        internal List<User> GetUsers()
//        {
//            List<User> users = new List<User>();
//            connection = new SqlConnection(consStr);

//            SqlCommand command = new SqlCommand("Select * from Users", connection);

//            try
//            {

//                connection.Open();
//                SqlDataReader reader = command.ExecuteReader();
//                while (reader.Read())
//                {
//                    users.Add(new User
//                    {
//                        Id = (int)reader["UserID"],
//                        Username = (string)reader["Username"]
//                    });
//                }

//                return users;
//            }
//            catch (Exception e)
//            {
//                throw;

//            }
//            finally
//            {
//                connection.Close();
//            }

//        }

//        internal string DeleteUser(int id)
//        {
//            connection = new SqlConnection(consStr);
//            SqlCommand command = new SqlCommand("delete Users where UserID = @id", connection);
//            command.Parameters.AddWithValue("@id", id);
//            try
//            {
//                connection.Open();
//                command.ExecuteNonQuery();
//                return "deleted";
//            }
//            catch (Exception e)
//            {
//                return e.Message;
//            }
//            finally
//            {
//                connection.Close();
//            }
//        }

//        internal User GetUserById(int id)
//        {
//            connection = new SqlConnection(consStr);
//            SqlCommand command = new SqlCommand("Select * from Users where UserID = @id", connection);
//            command.Parameters.AddWithValue("@id", id);
//            User user = null;
//            try
//            {
//                connection.Open();
//                command.ExecuteNonQuery();
//                SqlDataReader reader = command.ExecuteReader();
//                if (reader.Read())
//                {
//                    user = new User
//                    {
//                        Id = (int)reader["UserID"],
//                        Username = (string)reader["Username"]
//                    };
//                }

//                return user;
//            }
//            catch (Exception e)
//            {

//                throw;
//            }
//        }

//        private byte[] PasswordHasher(string password)
//        {
//            MD5 md5 = MD5.Create();

//            byte[] inputBytes = Encoding.UTF8.GetBytes(password);

//            byte[] hashBytes = md5.ComputeHash(inputBytes);

//            StringBuilder sb = new StringBuilder();

//            for (int i = 0; i < hashBytes.Length; i++)
//            {
//                sb.Append(hashBytes[i].ToString("x2"));
//            }
//            //return sb.ToString();
//            return hashBytes;

//        }

//        internal bool UpdateUser(User user)
//        {
//            throw new NotImplementedException();
//        }

//        internal bool UserExists(int id)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}

///*

//1) нужны классы под структуры данных
//2) нужны следующие функции:
//[put] 
//int Login(int id, password)
//int Login(string email, pass)

//string UpdateStudent(Student stu)
//string UpdateLecturer(Lecturer lec)
//string UpdateStaff(Staff stf)
//string ChangePassword(User user, newPassword)

//[post]
//string RegStudent(Student stu)
//string RegLecturer(Lecturer lec)
//string RegStaff(Staff stf)

//не ленись, пиши полноценный код
// */