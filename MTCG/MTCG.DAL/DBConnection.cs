using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTCG.Model.Users;
using Npgsql;

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
            var cs = "Host=localhost;Username=postgres;Password=admin;Database=mtcg";

            connection = new NpgsqlConnection(cs);
            connection.Open();

            var sql = "SELECT version()";

            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);

            var version = cmd.ExecuteScalar().ToString();
            Console.WriteLine($"PostgreSQL version: {version}");

        }

        public User GetUserByID(string username)
        {
            if(connection != null)
            {
                var sql = $"SELECT uc.username, uc.password, ud.bio, ud.image, us.elo, us.wins, us.losses FROM mtcg.\"UserCredentials\" uc, mtcg.\"UserData\" ud, mtcg.\"UserStats\" us WHERE uc.username = \'{username}\' AND ud.username = uc.username AND us.username = uc.username";
                NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
                NpgsqlDataReader dr = cmd.ExecuteReader();

                dr.Read();
                UserCredentials uc = new UserCredentials((string)dr[0], (string)dr[1]);
                UserData ud = new UserData();
                ud.Bio = (string)dr[2];
                ud.Name = (string)dr[0];
                ud.Image = (string)dr[3];
                UserStats us = new UserStats();
                us.Elo = (int)dr[4];
                us.Wins = (int)dr[5];
                us.Losses = (int)dr[6];
                User user = new User(uc, ud, us);
                
                return user;
            }
            else
            {
                Console.WriteLine("Database not connected!");
                return null;
            }
        }
    }
}
