using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB
{
    public struct EdgeDBConnection
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }


        public EdgeDBConnection(string username, string password, string hostname, int port, string database)
        {
            Username = username;
            Password = password;
            Hostname = hostname;
            Port = port;
            Database = database;
        }

        public override string ToString()
        {
            return $"edgedb://{Username}:{Password}@{Hostname}:{Port}/{Database}";
        }
    }
}
