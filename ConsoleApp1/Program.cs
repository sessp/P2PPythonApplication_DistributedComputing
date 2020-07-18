using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using IntermediateData;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServiceHost host;
                NetTcpBinding tcp = new NetTcpBinding();

                host = new ServiceHost(typeof(JobServer));
                string url = String.Format("net.tcp://{0}:{1}/JobService", "0.0.0.0", "8101");
                host.AddServiceEndpoint(typeof(JobInterface), tcp, url);
                host.Open();
                Debug.WriteLine("\n Server on: " + url + " is fully operational \n");
                Console.WriteLine("\nSystem online\n");
                Console.ReadLine();


                host.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine("\n Exception: " + e.Message);
            }
        }
    }
}
