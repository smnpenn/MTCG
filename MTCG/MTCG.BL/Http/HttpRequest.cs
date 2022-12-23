using Newtonsoft.Json;
using Npgsql.Internal.TypeHandlers.GeometricHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Reflection.Metadata;
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

        public string RequestBodyString
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

            //get Request Body Params
            if (Headers.ContainsKey("Content-Length") && Headers.ContainsKey("Content-Type"))
            {
                if (Headers["Content-Type"] != "application/json")
                {
                    throw new HttpRequestException("Incorrect Content-Type");
                }
                var data = new StringBuilder(200);
                char[] buffer = new char[1024];
                int bytesReadTotal = 0;
                while (bytesReadTotal < Int16.Parse(Headers["Content-Length"]))
                {
                    try
                    {
                        var bytesRead = reader.Read(buffer, 0, 1024);
                        bytesReadTotal += bytesRead;
                        if (bytesRead == 0) break;
                        data.Append(buffer, 0, bytesRead);
                    }
                    catch (IOException)
                    {
                        break;
                    }
                }
                RequestBodyString = data.ToString();
                Params = new Dictionary<string, string>();
                try
                {
                    Params = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.ToString());
                }
                catch (JsonReaderException)
                {
                    throw new JsonReaderException("Invalid Json-Format");
                }
            }
            
        }
    }
}
