using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Model.Users;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MTCG.DAL
{
    public class DatabaseHandler
    {
        NpgsqlConnection connection;

        private static DatabaseHandler instance = null;
        private static readonly object padlock = new object();

        public static DatabaseHandler Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new DatabaseHandler();
                    }
                    return instance;
                }
            }
        }

        public string Token = null;
        public void Connect()
        {
            string config = AppDomain.CurrentDomain.BaseDirectory + "/ressources/dbConfig.json";
            try
            {
                if (File.Exists(config))
                {
                    var pConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(config));
                    if (pConfig == null || pConfig["host"] == null || pConfig["username"] == null || pConfig["password"] == null || pConfig["database"] == null)
                    {
                        throw new IOException("DbConfig is invalid");
                    }

                    string cs = $"Host={pConfig["host"]};Username={pConfig["username"]};Password={pConfig["password"]};Database={pConfig["database"]}";
                    connection = new NpgsqlConnection(cs);
                    connection.Open();

                    Console.WriteLine("Database connection established!");
                }
                else
                {
                    Console.WriteLine("Database config is missing!");
                    System.Environment.Exit(-1);
                }
            }
            catch (NpgsqlException)
            {
                Console.WriteLine("Failed to connect to Database");
                System.Environment.Exit(-1);
            }

        }

        public UserData GetUserByID(string username)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT bio, image, username FROM mtcg.\"UserData\" WHERE username = @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = username;

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        UserData ud = new UserData();
                        if (dr.IsDBNull(0))
                        {
                            ud.Bio = null;
                        }
                        else
                        {
                            ud.Bio = (string)dr[0];
                        }

                        if (dr.IsDBNull(1))
                        {
                            ud.Image = null;
                        }
                        else
                        {
                            ud.Image = (string)dr[1];
                        }
                        
                        ud.Name = (string)dr[2];

                        dr.Close();
                        return ud;
                    }
                    else
                    {
                        dr.Close();
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return null;
                }
            }
        }

        public string LoginUser(string username, string password)
        {
            if (username == null || password == null)
            {
                Console.WriteLine("Invalid parameters");
                return null;
            }

            lock (padlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT password FROM mtcg.\"UserCredentials\" WHERE username = @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = username;

                    //TODO: Npgsql.NpgsqlOperationInProgressException
                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        if (password == (string)dr[0])
                        {
                            Console.WriteLine("Login successful");
                            string usernameWithSalt = username + GetRandomSalt(5);
                            string token = usernameWithSalt.GetHashCode().ToString();
                            dr.Close();
                            insertNewToken(username, token);
                            return token;
                        }
                        else
                        {
                            dr.Close();
                            Console.WriteLine("Incorrect password");
                        }
                    }
                    else
                    {
                        dr.Close();
                        Console.WriteLine("No such user!");
                    }
                }
                return null;
            }
        }

        public int RegisterUser(string username, string password)
        {
            if (username == null || password == null)
            {
                Console.WriteLine("Invalid parameters");
                return -1;
            }

            lock (padlock)
            {
                if (connection != null)
                {
                    try
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO mtcg.\"UserCredentials\"(username, password) VALUES(@p1, @p2);", connection);
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.String));
                        cmd.Prepare();
                        cmd.Parameters["p1"].Value = username;
                        cmd.Parameters["p2"].Value = password;

                        NpgsqlCommand cmd2 = new NpgsqlCommand("INSERT INTO mtcg.\"UserData\"(username, bio, image) VALUES(@p1, null, null);", connection);
                        cmd2.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd2.Prepare();
                        cmd2.Parameters["p1"].Value = username;

                        NpgsqlCommand cmd3 = new NpgsqlCommand("INSERT INTO mtcg.\"UserStats\"(username, elo, wins, losses) VALUES(@p1, 0, 0, 0);", connection);
                        cmd3.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd3.Prepare();
                        cmd3.Parameters["p1"].Value = username;

                        cmd.ExecuteNonQuery();
                        cmd2.ExecuteNonQuery();
                        cmd3.ExecuteNonQuery();

                        return 0;
                    }
                    catch (PostgresException ex)
                    {
                        return 1;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        private string GetRandomSalt(int length)
        {
            StringBuilder sb = new StringBuilder();
            Random rand = new Random();
            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = rand.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                sb.Append(letter);
            }
            return sb.ToString();
        }

        public bool AuthorizeToken(string username)
        {
            if (Token == null)
            {
                return false;
            }
            //TODO: Introduce admin user
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT token FROM mtcg.\"UserCredentials\" WHERE username = @p1;", connection);
            cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
            cmd.Prepare();
            cmd.Parameters["p1"].Value = username;

            NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                if (dr.IsDBNull(0))
                {
                    dr.Close();
                    return false;
                }

                if ((string)dr[0] == Token)
                {
                    dr.Close();
                    return true;
                }
                else
                {
                    dr.Close();
                    return false;
                }
            }
            else
            {
                dr.Close();
                return false;
            }
        }

        private void insertNewToken(string username, string token)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand("UPDATE mtcg.\"UserCredentials\" SET token = @p1 WHERE username = @p2;", connection);
                cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.String));
                cmd.Prepare();
                cmd.Parameters["p1"].Value = token;
                cmd.Parameters["p2"].Value = username;

                cmd.ExecuteNonQuery();
            }
            catch (PostgresException ex)
            {
                Console.WriteLine("Could not insert token, user does not exist");
            }
        }
    }
}
