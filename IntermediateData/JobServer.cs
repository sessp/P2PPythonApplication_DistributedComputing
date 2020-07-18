using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntermediateData
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext
= false)]
    public class JobServer : JobInterface
    {
        private static List<Job> jobList = new List<Job>();

        public JobServer()
        {
            
        }
        //Method to get jobs.
        public Job getJob()
        {
            Job jobNeedingCompletion = new Job();
            //Iterate through jobs and find one that ins't completed, then return it.
            foreach (var j in jobList)
            {
                if (j.jobCompleted == false)
                {
                    jobNeedingCompletion = j;
                }
            }

            return jobNeedingCompletion;
        }

        //Set the solution of a job.
        public void setSolution(Job j)
        {
            foreach (var j1 in jobList)
            {
                if (j1.jobID == j.jobID)
                {
                    Debug.WriteLine("\n" + "Job: " + j1.jobID + " : " + " " + j1.pythonString + " " + " has been completed! " +"\n");
                    j1.jobCompleted = true;
                    j1.resultString = j.resultString;
                }
            }
        }

        //Get the number of AVAILABLE jobs.
        public int getJobsAvailable()
        {
            int count = 0;
            foreach (var j in jobList)
            {
                if (j.jobCompleted == false)
                {
                    count++;
                }
            }

            return count;
        }

        //For testing purposes display num of jobs.
        private void displayJobs()
        {
            Debug.WriteLine("\n" + "Current Jobs available in the job server are: ");
            foreach (var j in jobList)
            {
                Debug.WriteLine("\n" + " ID: " + j.jobID + " {" + j.pythonString + "} " + " Result: " + j.resultString + ", completed: " + j.jobCompleted + "\n");
            }
        }

        //Add a job to the joblist
        public void addJob(Job j)
        {
            Debug.WriteLine("\n Job: " + j.jobID + " was added to the job q of a server. \n");
            jobList.Add(j);
            //displayJobs();
        }

        //A ping test, to make sure no initial errors when client connecting to a jobserver.
        public void test()
        {
            Debug.WriteLine("\n test \n");

        }
    }
}
