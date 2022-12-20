using MTCG.BL;
using MTCG.Model.Cards;
using MTCG.Model.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MTCG.DAL;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.BL
{
    public class HttpServer
    {
        private TcpListener tcpListener;
        DatabaseHandler db = DatabaseHandler.Instance;

        public HttpServer(string uri)
        {
            tcpListener = new TcpListener(IPAddress.Any, 10001);
            db.Connect();
        }

        public void StartServer()
        {
            tcpListener.Start(5);
            while (true)
            {
                Console.WriteLine($"Listening...");

                TcpClient socket = tcpListener.AcceptTcpClient();

                Task.Run(() =>
                {
                    StreamReader reader = new StreamReader(socket.GetStream());
                    StreamWriter writer = new StreamWriter(socket.GetStream());

                    HttpRequest request = new HttpRequest(socket, reader);
                    HttpResponse response = new HttpResponse(socket, writer);

                    string[] strParams = request.UrlSegments;

                    if (request.HttpMethod == "GET")
                    {
                        Console.WriteLine("Received a GET request");

                        if (strParams[0] == "users")
                        {
                            if (strParams.Length > 1)
                            {
                                //Retrieves the user data for the given username;
                                UserData userData = db.GetUserByID(strParams[1]);
                                if(userData != null)
                                {
                                    response.ResponseCode = 200;
                                    response.ResponseCodeText = "OK";
                                    response.ResponseString = JsonSerializer.Serialize(userData);
                                }
                                else
                                {
                                    response.ResponseCode = 404;
                                    response.ResponseCodeText = "Not Found";
                                    response.ResponseString = "User not found";
                                }
                                
                            }
                        }
                        else if (strParams[0] == "cards")
                        {
                            response.ResponseString = "Shows a user's cards";
                        }
                        else if (strParams[0] == "deck")
                        {
                            response.ResponseString = "Shows the user's currently configured deck";
                        }
                        else if (strParams[0] == "stats")
                        {
                            response.ResponseString = "Retrieves the stats for an individual user";
                        }
                        else if (strParams[0] == "scoreboard")
                        {
                            response.ResponseString = "Retrieves the user scoreboard ordered by the user's ELO.";
                        }
                        else if (strParams[0] == "tradings")
                        {
                            response.ResponseString = "Retrieves the currently available trading deals.";
                        }

                    }

                    if (request.HttpMethod == "POST")
                    {
                        Console.WriteLine("Received a POST request");

                        if (strParams[0] == "users")
                        {
                            response.ResponseString = "Register a new user";
                        }
                        else if (strParams[0] == "sessions")
                        {
                            int authToken = db.LoginUser(request.Params["username"], request.Params["password"]);
                            response.ResponseString = "Login with existing user";
                        }
                        else if (strParams[0] == "packages")
                        {
                            response.ResponseString = "Create new card packages (requires admin)";
                        }
                        else if (strParams[0] == "transactions")
                        {
                            if (strParams[1] == "packages")
                            {
                                response.ResponseString = "Acquire a package";
                            }
                        }
                        else if (strParams[0] == "battles")
                        {
                            response.ResponseString = "Enters the lobby to start a battle";
                            User user1 = new User(new UserCredentials("Simon", "Hallo"));
                            User user2 = new User(new UserCredentials("Max", "Hallo"));
                            Card[] cards = new Card[4];

                            cards[0] = new MonsterCard(35, ElementType.Fire, CardType.Ork);
                            cards[1] = new SpellCard(30, ElementType.Water);
                            cards[2] = new MonsterCard(15, ElementType.Normal, CardType.Knight);
                            cards[3] = new MonsterCard(30, ElementType.Normal, CardType.Dragon);

                            user1.ChooseDeck(cards);
                            user2.ChooseDeck(cards);

                            Battle battle = new Battle(user1, user2);
                            battle.executeBattle();

                        }
                        else if (strParams[0] == "tradings")
                        {
                            if (strParams.Length > 1)
                            {
                                response.ResponseString = $"Carry out a trade for the deal with the provided card. ID ({strParams[1]})";
                            }
                            else
                            {
                                response.ResponseString = "Creates a new trading deal.";
                            }
                        }
                    }

                    if (request.HttpMethod == "PUT")
                    {
                        Console.WriteLine("Received a PUT request");

                        if (strParams[0] == "users")
                        {
                            response.ResponseString = "Updates the user data for the given username.";
                        }
                        else if (strParams[0] == "deck")
                        {
                            response.ResponseString = "Configures the deck with four provided cards";
                        }
                    }

                    if (request.HttpMethod == "DELETE")
                    {
                        if (strParams[0] == "tradings")
                        {
                            if (strParams.Length > 1)
                            {
                                response.ResponseString = $"Deletes an existing trading deal. ID: {strParams[1]}";
                            }
                        }
                    }

                    response.Send();
                    writer.Close();
                    reader.Close();
                });

            }

        }

    }
}
