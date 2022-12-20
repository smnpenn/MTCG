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

namespace MTCG.BL.Http
{
    public class HttpServer
    {
        private TcpListener tcpListener;
        private HttpHandler httpHandler;
        //DatabaseHandler db = DatabaseHandler.Instance;

        public HttpServer(string uri)
        {
            tcpListener = new TcpListener(IPAddress.Any, 10001);
            httpHandler = new HttpHandler();
            //db.Connect();
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
                    httpHandler.HandleRequest(socket);
                });

            }

        }

    }
}
