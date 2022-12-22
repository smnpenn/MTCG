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
                    //var sql = $"SELECT uc.username, uc.password, ud.bio, ud.image, us.elo, us.wins, us.losses FROM mtcg.\"UserCredentials\" uc, mtcg.\"UserData\" ud, mtcg.\"UserStats\" us WHERE uc.username = \'?\' AND ud.username = uc.username AND us.username = uc.username";
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT bio, image, username FROM mtcg.\"UserData\" WHERE username = @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = username;

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        UserData ud = new UserData();
                        ud.Bio = (string)dr[0];
                        ud.Image = (string)dr[1];
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

        public int LoginUser(string username, string password)
        {
            if(username == null || password == null)
            {
                Console.WriteLine("Invalid parameters");
                return 0;
            }

            if(connection != null)
            {
                NpgsqlCommand cmd = new NpgsqlCommand("SELECT password FROM mtcg.\"UserCredentials\" WHERE username = @p1;", connection);
                cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                cmd.Prepare();
                cmd.Parameters["p1"].Value = username;

                //TODO: Npgsql.NpgsqlOperationInProgressException
                NpgsqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    if(password == (string)dr[0])
                    {
                        Console.WriteLine("Login successful");
                        string usernameWithSalt = username + GetRandomSalt(5);
                        int token = usernameWithSalt.GetHashCode();
                        return token;
                    }
                    else
                    {
                        Console.WriteLine("Incorrect password");
                    }
                }
                else
                {
                    dr.Close();
                    Console.WriteLine("No such user!");
                }
            }
            return 0;
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
    }
}
