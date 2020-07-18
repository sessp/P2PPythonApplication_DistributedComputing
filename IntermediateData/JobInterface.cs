using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.CompilerServices;

namespace IntermediateData
{
    /*Job interface/service contract for the job remote server functionality.*/
    [ServiceContract]
    public interface JobInterface
    {
        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        int getJobsAvailable();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        Job getJob();

        [OperationContract]
        [MethodImpl(MethodImplOptions.Synchronized)]
        void setSolution(Job j);

        [OperationContract]
        void addJob(Job j);

        [OperationContract]
        void test();
    }
}
