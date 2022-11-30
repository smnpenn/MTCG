using Npgsql.Internal.TypeHandlers.GeometricHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.BL
{
    internal class HttpRequest
    {
        private TcpClient socket;

        public string Url
        {
            get;
            private set;
        }

        public string HttpMethod
        {
            get;
            private set;
        }

        public string[] UrlSegments
        {
            get {
                return Url.Split("/").Skip(1).ToArray();
            }
        }

        private StreamReader reader;

        public HttpRequest(TcpClient socket, StreamReader reader)
        {
            this.socket = socket;
            this.reader = reader;
            string line = reader.ReadLine();
            string[] httpParts = line.Split(" ");
            HttpMethod = httpParts[0];
            Url = httpParts[1];
        }
    }
}
