using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.BL.Http
{
    internal class HttpResponse
    {
        private TcpClient socket;
        private StreamWriter writer;

        public string ResponseString
        {
            get;
            set;
        }

        public int ResponseCode
        {
            get;
            set;
        }

        public string ResponseCodeText
        {
            get;
            set;
        }

        public HttpResponse(TcpClient socket, StreamWriter writer)
        {
            this.socket = socket;
            this.writer = writer;
        }

        public void Send()
        {

            writer.WriteLine("HTTP/1.1 " + ResponseCode + " " + ResponseCodeText);
            writer.WriteLine("Content-Length: " + ResponseString.Length);
            writer.WriteLine("Content-Type: text/plain");
            writer.WriteLine();
            writer.WriteLine(ResponseString);

            //writer.Close();
            writer.Flush();

        }
    }
}
