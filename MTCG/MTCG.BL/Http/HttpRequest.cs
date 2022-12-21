using Npgsql.Internal.TypeHandlers.GeometricHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.BL.Http
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
            get
            {
                return Url.Split("/").Skip(1).ToArray();
            }
        }

        public Dictionary<string, string> Headers
        {
            get;
            private set;
        }

        public Dictionary<string, string> Params
        {
            get;
            private set;
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

            Headers = new Dictionary<string, string>();
            while ((line = reader.ReadLine()) != "")
            {
                string[] parts = line.Split(": ");
                Headers[parts[0]] = parts[1];
            }

            if (Headers.ContainsKey("Content-Type"))
            {
                if (Headers["Content-Type"] != "application/json")
                {
                    throw new HttpRequestException("Incorrect Content-Type");
                }
            }


            Params = new Dictionary<string, string>();

            if (reader.ReadLine() == "{")
            {
                while ((line = reader.ReadLine()) != "}")
                {
                    string[] parts = line.Split(": ");
                    Params[parts[0].Trim(new char[] { ' ', '"' })] = parts[1].Trim(new char[] { '"', ',', ' ' });
                }
            }
        }
    }
}
