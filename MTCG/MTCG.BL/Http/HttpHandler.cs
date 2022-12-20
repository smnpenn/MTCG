using MTCG.DAL;
using MTCG.Model.Cards;
using MTCG.Model.Users;
using System.Net.Sockets;
using System.Text.Json;

namespace MTCG.BL.Http
{
    public class HttpHandler
    {
        DatabaseHandler db = DatabaseHandler.Instance;
        public HttpHandler()
        {
            db.Connect();
        }

        public void HandleRequest(TcpClient socket)
        {
            StreamReader reader = new StreamReader(socket.GetStream());
            HttpRequest request = new HttpRequest(socket, reader);

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
                        if (userData != null)
                        {
                            SendResponse(socket, 200, "OK", JsonSerializer.Serialize(userData));
                        }
                        else
                        {
                            SendResponse(socket, 404, "Not found", "User not found");
                        }

                    }
                }
                else if (strParams[0] == "cards")
                {
                    SendResponse(socket, 200, "OK", "Shows a user's cards");
                }
                else if (strParams[0] == "deck")
                {
                    SendResponse(socket, 200, "OK", "Shows the user's currently configured deck");
                }
                else if (strParams[0] == "stats")
                {
                    SendResponse(socket, 200, "OK", "Retrieves the stats for an individual user");
                }
                else if (strParams[0] == "scoreboard")
                {
                    SendResponse(socket, 200, "OK", "Retrieves the user scoreboard ordered by the user's ELO.");
                }
                else if (strParams[0] == "tradings")
                {
                    SendResponse(socket, 200, "OK", "Retrieves the currently available trading deals.");
                }

            }

            if (request.HttpMethod == "POST")
            {
                Console.WriteLine("Received a POST request");

                if (strParams[0] == "users")
                {
                    SendResponse(socket, 200, "OK", "Register a new user");
                }
                else if (strParams[0] == "sessions")
                {
                    int authToken = db.LoginUser(request.Params["username"], request.Params["password"]);
                    SendResponse(socket, 200, "OK", "Login with existing user");
                }
                else if (strParams[0] == "packages")
                {
                    SendResponse(socket, 200, "OK", "Create new card packages (requires admin)");
                }
                else if (strParams[0] == "transactions")
                {
                    if (strParams[1] == "packages")
                    {
                        SendResponse(socket, 200, "OK", "Acquire a package");
                    }
                }
                else if (strParams[0] == "battles")
                {
                    SendResponse(socket, 200, "OK", "Enters the lobby to start a battle");
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
                        SendResponse(socket, 200, "OK", $"Carry out a trade for the deal with the provided card. ID ({strParams[1]})");
                    }
                    else
                    {
                        SendResponse(socket, 200, "OK", "Creates a new trading deal.");
                    }
                }
            }

            if (request.HttpMethod == "PUT")
            {
                Console.WriteLine("Received a PUT request");

                if (strParams[0] == "users")
                {
                    SendResponse(socket, 200, "OK", "Updates the user data for the given username.");
                }
                else if (strParams[0] == "deck")
                {
                    SendResponse(socket, 200, "OK", "Configures the deck with four provided cards");
                }
            }

            if (request.HttpMethod == "DELETE")
            {
                if (strParams[0] == "tradings")
                {
                    if (strParams.Length > 1)
                    {
                        SendResponse(socket, 200, "OK", $"Deletes an existing trading deal. ID: {strParams[1]}");
                    }
                }
            }

            //reader.Close();
        }

        public void SendResponse(TcpClient socket, int code, string codeText, string responseString)
        {
            StreamWriter writer = new StreamWriter(socket.GetStream()) { AutoFlush = true };
            HttpResponse response = new HttpResponse(socket, writer);
            response.ResponseCode = code;
            response.ResponseCodeText = codeText;
            response.ResponseString = responseString;

            response.Send();
            //writer.Close();

        }
    }
}
