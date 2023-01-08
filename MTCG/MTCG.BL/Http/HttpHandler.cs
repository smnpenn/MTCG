using MTCG.BL.Battle;
using MTCG.DAL;
using MTCG.Model.Cards;
using MTCG.Model.Users;
using Newtonsoft.Json;
using Npgsql;
using System.Net.Sockets;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

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
            try
            {
                StreamReader reader = new StreamReader(socket.GetStream());
                HttpRequest request = new HttpRequest(socket, reader);
                string[] strParams = request.UrlSegments;
                if (request.Headers.ContainsKey("Authorization"))
                {
                    db.Token = request.Headers["Authorization"];
                }

                if (request.HttpMethod == "GET")
                {
                    Console.WriteLine("Received a GET request");

                    if (strParams[0] == "users")
                    {
                        if (strParams.Length > 1)
                        {
                            if (db.AuthorizeToken())
                            {
                                //Retrieves the user data for the given username;
                                UserData? userData = db.GetUserByID(strParams[1]);
                                if (userData != null)
                                {
                                    SendResponse(socket, 200, "OK", System.Text.Json.JsonSerializer.Serialize(userData));
                                }
                                else
                                {
                                    SendResponse(socket, 404, "Not found", "User not found");
                                }
                            }
                            else
                            {
                                SendUnauthorizedError(socket);
                            }
                        }
                        else
                        {
                            SendBadRequest(socket);
                        }
                    }
                    else if (strParams[0] == "cards")
                    {
                        if (db.AuthorizeToken())
                        {
                            string? res = db.GetCards();

                            if(res != null)
                            {
                                SendResponse(socket, 200, "OK", res);
                            }
                            else
                            {
                                SendResponse(socket, 204, "No content", "User has no cards");
                            }
                        }
                        else
                        {
                            SendUnauthorizedError(socket);
                        }
                        
                    }
                    else if (strParams[0] == "deck")
                    {
                        if (db.AuthorizeToken())
                        {
                            string? res;
                            if(strParams.Length > 1)
                            {
                                if (strParams[1] == "format=plain")
                                {
                                    res = db.GetDeck("plain");
                                }
                                else
                                {
                                    res = db.GetDeck();
                                }
                            }
                            else
                            {
                                res = db.GetDeck();
                            }

                            if(res != null)
                            {
                                if(strParams.Length > 1)
                                {
                                    if (strParams[1] == "format=plain")
                                    {
                                        SendResponse(socket, 200, "OK", res, "text/plain");
                                    }
                                    else
                                    {
                                        SendResponse(socket, 200, "OK", res);
                                    }
                                }
                                else
                                {
                                    SendResponse(socket, 200, "OK", res);
                                }
                            }
                            else
                            {
                                SendResponse(socket, 204, "No content", "The request was fine, but the deck doesn't have any cards");
                            }
                        }
                        else
                        {
                            SendUnauthorizedError(socket);
                        }
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
                        //Register a new user
                        //TODO: Add JSONSchemas for RequestBodys und ResponseBodys
                        
                        if (request.Params.ContainsKey("Username") && request.Params.ContainsKey("Password"))
                        {
                            int code = db.RegisterUser(request.Params["Username"], request.Params["Password"]);
                            if (code == 0)
                            {
                                SendResponse(socket, 201, "OK", "User successfully created");
                            }
                            else if(code == 1)
                            {
                                SendResponse(socket, 409, "Conflict", "User with same username already registered");
                            }
                            else
                            {
                                SendBadRequest(socket);
                            }
                        }
                        else
                        {
                            SendBadRequest(socket);
                        }
                    }
                    else if (strParams[0] == "sessions")
                    {
                        if (request.Params.ContainsKey("Username") && request.Params.ContainsKey("Password"))
                        {
                            string? authToken = db.LoginUser(request.Params["Username"], request.Params["Password"]);
                            if (authToken != null)
                            {
                                SendResponse(socket, 200, "OK", "{\"authToken\":\"" + authToken.ToString() + "\"}");
                            }
                            else
                            {
                                SendResponse(socket, 401, "Unauthorized", "Invalid username/password provided");
                            }
                        }
                        else
                        {
                            SendBadRequest(socket, "400 Bad Request");
                        }
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
                        if (db.AuthorizeToken())
                        {
                            List<Card>? deck = db.GetDeckAsList();
                            if(deck != null)
                            {
                                BattleLobby.Instance.EnterLobby(new Player(deck));
                            }
                            else
                            {
                                SendBadRequest(socket, "User has no active deck");
                            }
                        }
                        else
                        {
                            SendUnauthorizedError(socket);
                        }
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
                        if (strParams.Length > 1)
                        {
                            if (db.AuthorizeToken())
                            {
                                if (request.Params.ContainsKey("Name") && request.Params.ContainsKey("Bio") && request.Params.ContainsKey("Image"))
                                {

                                    int code = db.UpdateUserData(strParams[1], request.Params["Name"], request.Params["Bio"], request.Params["Image"]);
                                    if (code == 0)
                                    {
                                        SendResponse(socket, 200, "OK", "User succesfully updated.");
                                    }
                                    else if(code == 1)
                                    {
                                        SendResponse(socket, 404, "Not found", "User not found.");
                                    }
                                    else
                                    {
                                        SendBadRequest(socket);
                                    }
                                }
                            }
                            else
                            {
                                SendUnauthorizedError(socket);
                            }
                        }
                        
                    }
                    else if (strParams[0] == "deck")
                    {
                        if (db.AuthorizeToken())
                        {
                            if(request.Params.Count == 4 && request.Params.ContainsKey("card1") && request.Params.ContainsKey("card2") && request.Params.ContainsKey("card3") && request.Params.ContainsKey("card4"))
                            {
                                List<string> cards = new List<string>();
                                cards.Add(request.Params["card1"]);
                                cards.Add(request.Params["card2"]);
                                cards.Add(request.Params["card3"]);
                                cards.Add(request.Params["card4"]);
                                if (db.ConfigureDeck(cards))
                                {
                                    SendResponse(socket, 200, "OK", "The deck has been successfully configured");
                                }
                                else
                                {
                                    SendResponse(socket, 403, "Forbidden", "At least one of the provided cards does not belong to the user or is not available.");
                                }
                            }
                            else
                            {
                                SendBadRequest(socket, "The provided deck did not include the required amount of cards");
                            }
                        }
                        else
                        {
                            SendUnauthorizedError(socket);
                        }
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

                reader.Close();
            }
            catch (JsonReaderException ex)
            {
                SendBadRequest(socket, ex.Message);
            }
            catch(HttpRequestException ex)
            {
                SendBadRequest(socket, ex.Message);
            }catch(PostgresException ex)
            {
                SendBadRequest(socket, ex.Message);
            }
        }

        public void SendResponse(TcpClient socket, int code, string codeText, string responseBody, string contentType = "application/json")
        {
            StreamWriter writer = new StreamWriter(socket.GetStream()) { AutoFlush = true };
            HttpResponse response = new HttpResponse(socket, writer);
            response.ResponseCode = code;
            response.ResponseCodeText = codeText;
            response.ResponseBody = responseBody;
            response.ContentType = contentType;

            response.Send();
            writer.Close();

        }

        public void SendBadRequest(TcpClient socket, string message = "400 Bad Request")
        {
            StreamWriter writer = new StreamWriter(socket.GetStream()) { AutoFlush = true };
            HttpResponse response = new HttpResponse(socket, writer);
            response.ResponseCode = 400;
            response.ResponseCodeText = "Bad Request";
            response.ResponseBody = message;

            response.Send();
            writer.Close();
        }

        public void SendUnauthorizedError(TcpClient socket)
        {
            StreamWriter writer = new StreamWriter(socket.GetStream()) { AutoFlush = true };
            HttpResponse response = new HttpResponse(socket, writer);
            response.ResponseCode = 401;
            response.ResponseCodeText = "Unauthorized";
            response.ResponseBody = "Access token is missing or invalid";

            response.Send();
            writer.Close();
        }
    }
}
