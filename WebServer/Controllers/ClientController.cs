using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using IntermediateData;
using Microsoft.Ajax.Utilities;
using WebServer.Models;


namespace WebServer.Controllers
{
    public class ClientController : ApiController
    {

        // Test Method
        public string Get(int id)
        {
            return "value";
        }

        //RegisterClient - Client calls this web service to register themselves.
        [Route("api/Client/register")]
        [HttpPost]
        public uint RegisterClient([FromBody]IntermediateClient client)
        {
            Debug.WriteLine("Post request received from a client w/: " + client.ipaddress + " " + client.port + " \n");
            //Register by adding them to the list!
            return Server.getWebServer().addClient(client.ipaddress, client.port);
        }

        //Get a list of other clients. Turn that into an intermediate client list cause we don't like globals!
        [Route("api/Client/GetClients/{id}")]
        [HttpGet]
        public List<IntermediateClient> getClientList(uint id)
        {
           List<Client> clientList = Server.getWebServer().getOtherClients(id);
            List<IntermediateClient> cList = new List<IntermediateClient>();
           foreach (var c in clientList)
           {
                IntermediateClient client = new IntermediateClient(c.getIP(), c.getPort(), c.getId());
                cList.Add(client);

           }
            return cList;
        }

        //Method to update score.
        [Route("api/Client/updateScore/")]
        [HttpPost]
        public void updateScore([FromBody] Score s)
        {
            Server.getWebServer().updateScores(s);
        }

        //Method to get scores.
        [Route("api/Client/getScores/")]
        [HttpGet]
        public List<Score> getScores()
        {
            return Server.getWebServer().getScores();
        }
    }
}