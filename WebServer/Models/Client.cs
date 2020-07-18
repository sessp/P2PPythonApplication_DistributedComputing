using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServer.Models
{
    public class Client
    {
        //Class represents a client.

        private string ipaddress;
        private uint port;
        private uint id;

        public Client(uint identificationNum,string ip, uint p)
        {
            id = identificationNum;
            ipaddress = ip;
            port = p;
        }
        public string getIP()
        {
            return this.ipaddress;
        }

        public uint getPort()
        {
            return this.port;
        }

        public uint getId()
        {
            return this.id;
        }

        public void setIP(string ip)
        {
            ipaddress = ip;
        }

        public void setPort(uint p)
        {
            port = p;
        }

        public override string ToString()
        {
            return "\n Client: " + id + ", located at: " + ipaddress + " on Port: " + port + "\n";
        }
    }
}