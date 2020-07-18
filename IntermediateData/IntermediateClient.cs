using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntermediateData
{
    public class IntermediateClient
    {
        /*Represents an intermediate client. */
        public string ipaddress;
        public uint port;
        public uint id;

        public IntermediateClient(string ip, uint p, uint identification)
        {
            ipaddress = ip;
            port = p;
            id = identification;
        }
    }
}
