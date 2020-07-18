using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApplication
{
    class ClientConnectionFailedException : Exception
    {
        public ClientConnectionFailedException()
        {
        }

        public ClientConnectionFailedException(string message)
            : base(message)
        {
        }

        public ClientConnectionFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
