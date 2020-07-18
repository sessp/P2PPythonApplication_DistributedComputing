using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestSharp;
using IntermediateData;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Threading;
using System.Security.Cryptography;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private RestClient client;
        private uint id = 1;
        private string ipAddress = "0.0.0.0";
        private string portNumber;
        private uint initPort = 8000;
        private JobInterface jServer;
        private ServiceHost host;
        private int jobCount = 0;
        private int jobCompletedCount = 0;
        private Boolean available;
        public MainWindow()
        {
            InitializeComponent();
            runButton.IsEnabled = false;
        }

        //Method executed by delegate to perform jobs.
        public void performJobs()
        {
            Debug.WriteLine("\n =============== Perform Jobs ================ \n");

            Debug.WriteLine("\n Waiting for another client/peer to join! \n");
            client = new RestClient("https://localhost:44322/");

            //Infinite Loop aka until client closes.
            while(true)
            {
                    RestRequest request = new RestRequest("api/Client/GetClients/" + id);
                    //Look for new jobs/query web service for client list.
                    IRestResponse response = client.Get(request);
                    List<IntermediateClient> otherClients = JsonConvert.DeserializeObject<List<IntermediateClient>>(response.Content);
                //If the list is null? no other clients? Just make us do the computation.
                foreach (var c in otherClients)
                {
                    try {
                        /*Reset the JobInterface so it's the server of this client and not some other random server
                        we connected to prior when running this loop.*/
                        connect(c.ipaddress, c.port);
                        //Check if available jobs > 0
                        if (jServer.getJobsAvailable() > 0)
                        {
                            Debug.WriteLine(" Attempting to connect to Client: " + c.id + " at " + c.ipaddress + " on port: " + c.port);
                            Debug.WriteLine(" Jobs Available: " + jServer.getJobsAvailable().ToString() + "\n");

                            //Update GUI to display running job.
                            Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                currentJobField.Text = "Running Job";
                            })
                            );
                            //Get job, validate it and complete it :D
                            Job jobToComplete = jServer.getJob();
                            if (isValidHash(jobToComplete.pythonString, jobToComplete.hash))
                            {
                                completeJob(jobToComplete, c);
                            }


                        }
                        else
                        {
                            //Just log an attempt to connect to any client, for debugging and feedback purposes.
                            Debug.WriteLine("\n An attempt to connect to Client: " + c.id + " at " + c.ipaddress + " on port: " + c.port + " was made but no jobs are currently available from the client." + "\n");

                        }
                    }
                    catch (System.ServiceModel.EndpointNotFoundException exception)
                    {
                       /*If a client disonnects then just move to next item in foreach
                       This isn't an optimal solution, the optimal one would be for P2P Webserver to remove
                       them from the list, but I ran out of time to implement that.*/
                    }
            }
                    
            }
        }

        private void completeJob(Job jobToComplete, IntermediateClient c)
        {
            //python 2
            try
            {
                //Tested using PREDEFINED/BUILT IN PYTHON FUNCTIONs like round(x),pow(x,y) ect.
                ScriptEngine engine = Python.CreateEngine();
                ScriptScope scope = engine.CreateScope();
                dynamic pythonCode = engine.Execute(decodeBase64(jobToComplete.pythonString), scope);
                var result = pythonCode;
                string s = result.ToString();
                Debug.WriteLine("\n " + s + " \n");
                jobToComplete.resultString = s;
                connect(c.ipaddress, c.port);
                jServer.setSolution(jobToComplete);
                jobCompletedCount++;

                //Update client's score via a post to the P2P Server.
                RestRequest r = new RestRequest("api/Client/updateScore/");
                Score score = new Score();
                score.id = id;
                score.numJobsCompleted = (uint)jobCompletedCount;
                r.AddJsonBody(score);
                IRestResponse response = client.Post(r);

                //Tell GUI to update in their thread.
                Dispatcher.BeginInvoke(
                    new Action(() => {
                        currentJobField.Text = "Not Working";
                        jobCompletedField.Text = jobCompletedCount.ToString();
                  })
               );
                //Update scoreboard.
                updateScoreboard();
            }
            catch (Exception e)
            {
                //Catch all these exceptions, if issue stop execution of a thread/delegate.
                 Debug.WriteLine("\n Exception: " + e.Message);
            }
        }

        //Method to update scoreboard.
        private void updateScoreboard()
        {

            //Get List of scores
            RestRequest r1 = new RestRequest("api/Client/getScores/");
            IRestResponse response1 = client.Get(r1);
            List<Score> scoreList = JsonConvert.DeserializeObject<List<Score>>(response1.Content);

            //Tell the GUI to update in it's thread.
            Dispatcher.BeginInvoke(
                new Action(() => {
                    scoreboard.Items.Clear();
                    foreach (var sc in scoreList)
                    {
                        scoreboard.Items.Add("Client: " + sc.id + " Score: " + sc.numJobsCompleted);
                    }
                })
           );

        }

        //Method to encode a base 64 String.
        public string encodeBase64(String pythonString)
        {
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(pythonString);
            return Convert.ToBase64String(textBytes);
        }
        //Method to decode a base 64 String.
        public string decodeBase64(String base64Text)
        {
            byte[] encodedBytes = Convert.FromBase64String(base64Text);
            return System.Text.Encoding.UTF8.GetString(encodedBytes);
        }

        //Method to register with the P2P WebServer.
        public void register()
        {
            Debug.WriteLine("\n =============== Register Client ================ \n");
            client = new RestClient("https://localhost:44322/");
            IntermediateClient interClient = new IntermediateClient(ipAddress, initPort, 0);
            RestRequest r = new RestRequest("api/Client/register");
            r.AddJsonBody(interClient);
            IRestResponse response = client.Post(r);
            Debug.WriteLine("\n Received Client ID from web service: " + response.Content);
            id = UInt32.Parse(response.Content);
            //P2P Web server returns this clients ID, which this client will use later for various things.
            
        }

        //Delegates
        public delegate void createS(string one, uint two);
        public delegate void performJ();
        
        //Method used to create a server. 
        public void createServer(string ip, uint port)
        {
            uint p;
            p = port;
            while (!available)
            {
                try
                {
                    //Find an open port.
                    p = initPort;
                   
                    NetTcpBinding tcp = new NetTcpBinding();

                    //attempt to create a remote server on the port.
                    host = new ServiceHost(typeof(JobServer));
                    string url = String.Format("net.tcp://0.0.0.0:{0}/jService", p);
                    host.AddServiceEndpoint(typeof(JobInterface), tcp, url);
                    available = true;
                    initPort = p;
                    host.Open();

                }
                catch (Exception e)
                {
                    //If the port we attempted to connect to isn't available, increase port and try next one.
                    Debug.WriteLine("\n Exception: " + e.Message);
                    available = false;
                    initPort++;
                }
            }
        }

        //Method used to connect to any clients remote server, including this clients remote server.
        public void connect(string ip, uint port)
        {
            try
            { 
                ChannelFactory<JobInterface> jInterface;
                NetTcpBinding tcp = new NetTcpBinding();
                string url = String.Format("net.tcp://localhost:{0}/jService", port);
                jInterface = new ChannelFactory<JobInterface>(tcp, url);
                jServer = jInterface.CreateChannel();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("\n Exception: " + exception.Message);
            }
        }

        //Method that gets a job from the job server, decodes it and then performs it.
        public void perform()
        {
            connect(ipAddress, initPort);
            Job jRetrieved = jServer.getJob();
            Debug.WriteLine("\n" + decodeBase64(jRetrieved.pythonString) + "\n");
        }

        //Method used to validate a hash.
        public bool isValidHash(string encodedPython, byte[] hash1)
        {
            /*If hash is valid then return true else false.
            Convert to int hash when comparing, just cause easier. */
            bool flag = false;
            SHA256 sha256Hash = SHA256.Create();
            byte[] hash2 = sha256Hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(encodedPython));
            int intHash1 = BitConverter.ToInt32(hash1, 0);
            int intHash2 = BitConverter.ToInt32(hash2, 0);
            if (intHash1 == intHash2)
            {
                flag = true;
            }

            return flag;
        }

        //Method executed when run button is clicked.
        private void Run_Click(object sender, RoutedEventArgs e)
        {
            /*Get python, create a job with it, encode it hash it and then connect and add job to the remote server
            associated with this client. */
            String inputtedJob = pythonInput.Text;
            Job j = new Job();
            j.pythonString = encodeBase64(inputtedJob);
            SHA256 sha256Hash = SHA256.Create();
            j.hash = sha256Hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(j.pythonString));
            j.jobID = (jobCount + 1);
            jobCount++;
            connect(ipAddress, initPort);
            //^ should only be adding jobs to current list.
            jServer.addJob(j);
            

        }

        private void testPerform()
        {
            jServer.test();
        }

        //Method to add a job to the job list and thus run it.
        private void GetJob_Click(object sender, RoutedEventArgs e)
        {
            performJ p;
            p = performJobs;
            currentJobField.Text = " Completing Job ";
            p.BeginInvoke(null,null);

        }

        //Method run by the job delegate to perform jobs.
        private void performJobThread()
        {
            //Create and run the delegate.
            performJ p;
            p = performJobs;
            currentJobField.Text = " Searching for Job ";
            p.BeginInvoke(null, null);
        }

        //Method called when server create button is interacted with.
        private void createServerButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("\n ID of current thread: " + Thread.CurrentThread.ManagedThreadId + " \n");

            createServer(ipAddress,initPort);

            //Register the client to the web service
            register();
            createServerButton.IsEnabled = false;
            runButton.IsEnabled = true;
            performJobThread();
        }

        //Method called when closing to make sure the host obj closes.
        private void W_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (host != null)
            {
                host.Close();
            }
            Debug.WriteLine("\n Window Closing :( \n");
        }
    }
}
