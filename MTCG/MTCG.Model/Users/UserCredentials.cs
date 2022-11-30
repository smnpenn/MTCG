using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Model.Users
{
    public class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public UserCredentials(string username, string password)
        {
            Username = username;
            Password = password;
        }
    }
}
