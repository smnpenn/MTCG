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
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using MTCG.Model.Cards;

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

        public string? Token = null;
        public string? AuthorizedUser = null;
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

        public UserData? GetUserByID(string username)
        {
            lock (padlock)
            {
                if (connection != null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT username, name, bio, image FROM mtcg.\"UserData\" WHERE username = @p1;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();
                    cmd.Parameters["p1"].Value = username;

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        UserData ud = new UserData();

                        ud.Username = (string)dr[0];

                        if (dr.IsDBNull(1))
                        {
                            ud.Name = null;
                        }
                        else
                        {
                            ud.Name = (string)dr[1];
                        }

                        if (dr.IsDBNull(2))
                        {
                            ud.Bio = null;
                        }
                        else
                        {
                            ud.Bio = (string)dr[1];
                        }

                        if (dr.IsDBNull(3))
                        {
                            ud.Image = null;
                        }
                        else
                        {
                            ud.Image = (string)dr[1];
                        }

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

        public string? LoginUser(string username, string password)
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

                        NpgsqlCommand cmd2 = new NpgsqlCommand("INSERT INTO mtcg.\"UserData\"(username, name, bio, image) VALUES(@p1, null, null, null);", connection);
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
                    catch (PostgresException)
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

        public int UpdateUserData(string username, string name, string bio, string image)
        {
            lock (padlock)
            {
                if(connection != null)
                {
                    try
                    {
                        NpgsqlCommand cmd = new NpgsqlCommand("UPDATE mtcg.\"UserData\" SET name = @p1, bio = @p2, image = @p3 WHERE username = @p4;", connection);
                        
                        cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.String));
                        cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.String));
                        cmd.Prepare();

                        if(name != null)
                        {
                            cmd.Parameters["p1"].Value = name;
                        }
                        else
                        {
                            cmd.Parameters["p1"].Value = DBNull.Value;
                        }

                        if (bio != null)
                        {
                            cmd.Parameters["p2"].Value = bio;
                        }
                        else
                        {
                            cmd.Parameters["p2"].Value = DBNull.Value;
                        }

                        if (image != null)
                        {
                            cmd.Parameters["p3"].Value = image;
                        }
                        else
                        {
                            cmd.Parameters["p3"].Value = DBNull.Value;
                        }

                        cmd.Parameters["p4"].Value = username;

                        cmd.ExecuteNonQuery();
                        return 0;
                    }
                    catch (PostgresException)
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

        public string? GetCards()
        {
            lock (padlock)
            {
                if (connection != null || AuthorizedUser == null)
                {
                    
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT type, element, damage FROM mtcg.\"Card\" WHERE username = @p1;", connection);

                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();

                    cmd.Parameters["p1"].Value = AuthorizedUser;

                    NpgsqlDataReader dr = cmd.ExecuteReader();

                    return GetJSONCardsString(dr);
                }
                else
                {
                    Console.WriteLine("Database not connected!");
                    return null;
                }
            }
        }

        public string? GetDeck(string format = "json")
        {
            lock (padlock)
            {
                if(connection != null || AuthorizedUser == null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT card1, card2, card3, card4 FROM mtcg.\"Deck\" WHERE username = @p1;", connection);

                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();

                    cmd.Parameters["p1"].Value = AuthorizedUser;

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    List<Guid> cardIDs = new List<Guid>();

                    if (dr.Read())
                    {
                        cardIDs.Add((Guid)dr[0]);
                        cardIDs.Add((Guid)dr[1]);
                        cardIDs.Add((Guid)dr[2]);
                        cardIDs.Add((Guid)dr[3]);
                        dr.Close();
                    }
                    else
                    {
                        dr.Close();
                        return null;
                    }

                    NpgsqlCommand cmd2 = new NpgsqlCommand("SELECT type, element, damage FROM mtcg.\"Card\" WHERE id = @p1 OR id = @p2 OR id = @p3 OR id = @p4;", connection);
                    cmd2.Parameters.Add(new NpgsqlParameter("p1", DbType.Guid));
                    cmd2.Parameters.Add(new NpgsqlParameter("p2", DbType.Guid));
                    cmd2.Parameters.Add(new NpgsqlParameter("p3", DbType.Guid));
                    cmd2.Parameters.Add(new NpgsqlParameter("p4", DbType.Guid));

                    cmd2.Prepare();
                    cmd2.Parameters["p1"].Value = cardIDs[0];
                    cmd2.Parameters["p2"].Value = cardIDs[1];
                    cmd2.Parameters["p3"].Value = cardIDs[2];
                    cmd2.Parameters["p4"].Value = cardIDs[3];

                    NpgsqlDataReader dr2 = cmd2.ExecuteReader();

                    if(format == "json")
                    {
                        return GetJSONCardsString(dr2);
                    }else if(format == "plain")
                    {
                        return GetPlainCardsString(dr2);
                    }else
                    {
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

        public bool ConfigureDeck(List<string> cards)
        {
            lock (padlock)
            {
                if(connection != null || AuthorizedUser == null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("UPDATE mtcg.\"Deck\" SET card1=@p1, card2=@p2, card3=@p3, card4=@p4 WHERE username=@p5;", connection);
                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.Guid));
                    cmd.Parameters.Add(new NpgsqlParameter("p2", DbType.Guid));
                    cmd.Parameters.Add(new NpgsqlParameter("p3", DbType.Guid));
                    cmd.Parameters.Add(new NpgsqlParameter("p4", DbType.Guid));
                    cmd.Parameters.Add(new NpgsqlParameter("p5", DbType.String));
                    cmd.Prepare();
                    try
                    {
                        cmd.Parameters["p1"].Value = Guid.Parse(cards[0]);
                        cmd.Parameters["p2"].Value = Guid.Parse(cards[1]);
                        cmd.Parameters["p3"].Value = Guid.Parse(cards[2]);
                        cmd.Parameters["p4"].Value = Guid.Parse(cards[3]);
                    }
                    catch (FormatException)
                    {
                        return false;
                    }
                    
                    cmd.Parameters["p5"].Value = AuthorizedUser;

                    cmd.ExecuteNonQuery();

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public List<Card>? GetDeckAsList()
        {
            lock (padlock)
            {
                if (connection != null || AuthorizedUser == null)
                {
                    NpgsqlCommand cmd = new NpgsqlCommand("SELECT card1, card2, card3, card4 FROM mtcg.\"Deck\" WHERE username = @p1;", connection);

                    cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
                    cmd.Prepare();

                    cmd.Parameters["p1"].Value = AuthorizedUser;

                    NpgsqlDataReader dr = cmd.ExecuteReader();
                    List<Guid> cardIDs = new List<Guid>();

                    if (dr.Read())
                    {
                        cardIDs.Add((Guid)dr[0]);
                        cardIDs.Add((Guid)dr[1]);
                        cardIDs.Add((Guid)dr[2]);
                        cardIDs.Add((Guid)dr[3]);
                        dr.Close();
                    }
                    else
                    {
                        dr.Close();
                        return null;
                    }

                    NpgsqlCommand cmd2 = new NpgsqlCommand("SELECT type, element, damage FROM mtcg.\"Card\" WHERE id = @p1 OR id = @p2 OR id = @p3 OR id = @p4;", connection);
                    cmd2.Parameters.Add(new NpgsqlParameter("p1", DbType.Guid));
                    cmd2.Parameters.Add(new NpgsqlParameter("p2", DbType.Guid));
                    cmd2.Parameters.Add(new NpgsqlParameter("p3", DbType.Guid));
                    cmd2.Parameters.Add(new NpgsqlParameter("p4", DbType.Guid));

                    cmd2.Prepare();
                    cmd2.Parameters["p1"].Value = cardIDs[0];
                    cmd2.Parameters["p2"].Value = cardIDs[1];
                    cmd2.Parameters["p3"].Value = cardIDs[2];
                    cmd2.Parameters["p4"].Value = cardIDs[3];

                    NpgsqlDataReader dr2 = cmd2.ExecuteReader();

                    List<Card> cards = new List<Card>();
                    while (dr2.Read())
                    {
                        ElementType element = (ElementType)(Int16)dr2[1];
                        CardType type = (CardType)(Int16)dr2[0];
                        if (type == CardType.Spell)
                        {
                            cards.Add(new SpellCard((double)dr2[2], element));
                        }
                        else
                        {
                            cards.Add(new MonsterCard((double)dr2[2], element, type));
                        }
                    }

                    if (cards.Count > 0)
                    {
                        return cards;
                    }
                    else
                    {
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

        private string? GetJSONCardsString(NpgsqlDataReader dr)
        {
            List<Card> cards = new List<Card>();
            while (dr.Read())
            {
                ElementType element = (ElementType)(Int16)dr[1];
                CardType type = (CardType)(Int16)dr[0];
                if (type == CardType.Spell)
                {
                    cards.Add(new SpellCard((double)dr[2], element));
                }
                else
                {
                    cards.Add(new MonsterCard((double)dr[2], element, type));
                }
            }

            if (cards.Count > 0)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(new
                {
                    cards = cards
                });
                return json;
            }
            else
            {
                return null;
            }
        }

        private string? GetPlainCardsString(NpgsqlDataReader dr)
        {
            List<Card> cards = new List<Card>();
            while (dr.Read())
            {
                ElementType element = (ElementType)(Int16)dr[1];
                CardType type = (CardType)(Int16)dr[0];
                if (type == CardType.Spell)
                {
                    cards.Add(new SpellCard((double)dr[2], element));
                }
                else
                {
                    cards.Add(new MonsterCard((double)dr[2], element, type));
                }
            }

            if (cards.Count > 0)
            {
                string plainString = "";
                int i = 1;
                foreach(Card card in cards)
                {
                    plainString = plainString + "Card " + i + ": " + card.ToString();
                    ++i;
                }
                return plainString;
            }
            else
            {
                return null;
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

        public bool AuthorizeToken()
        {
            if (Token == null)
            {
                return false;
            }
            //TODO: Introduce admin user
            NpgsqlCommand cmd = new NpgsqlCommand("SELECT username FROM mtcg.\"UserCredentials\" WHERE token = @p1;", connection);
            cmd.Parameters.Add(new NpgsqlParameter("p1", DbType.String));
            cmd.Prepare();
            cmd.Parameters["p1"].Value = Token;

            NpgsqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                AuthorizedUser = (string)dr[0];
                dr.Close();
                return true;
            }
            else
            {
                AuthorizedUser = null;
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
            catch (PostgresException)
            {
                Console.WriteLine("Could not insert token, user does not exist");
            }
        }
    }
}
