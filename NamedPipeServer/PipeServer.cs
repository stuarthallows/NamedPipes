using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using NamedPipeShared;

namespace NamedPipeServer
{
    /// <summary>
    /// This example demonstrates how to create a named pipe by using the NamedPipeServerStream class. The server process creates four threads.
    /// Each thread can accept a client connection. The connected client process then supplies the server with a file name.
    /// If the client has sufficient permissions, the server process opens the file and sends its contents back to the client.
    /// </summary>
    public class PipeServer
    {
        private const int NumThreads = 4;

        public static void Main()
        {
            int i;
            var servers = new Thread[NumThreads];

            Console.WriteLine("\n*** Named pipe server stream with impersonation example ***\n");
            Console.WriteLine("Waiting for client connect...\n");
            
            for (i = 0; i < NumThreads; i++)
            {
                servers[i] = new Thread(ServerThread);
                servers[i].Start();
            }

            Thread.Sleep(250);
            
            while (i > 0)
            {
                for (int j = 0; j < NumThreads; j++)
                {
                    if (servers[j] != null)
                    {
                        if (servers[j].Join(250))
                        {
                            Console.WriteLine("Server thread[{0}] finished.", servers[j].ManagedThreadId);
                            servers[j] = null;
                            i--;    // decrement the thread watch count
                        }
                    }
                }
            }
            
            Console.WriteLine("\nServer threads exhausted, exiting.");
        }

        private static void ServerThread(object data)
        {
            var pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, NumThreads);

            int threadId = Thread.CurrentThread.ManagedThreadId;

            // Wait for a client to connect
            pipeServer.WaitForConnection();

            Console.WriteLine("Client connected on thread[{0}].", threadId);
            try
            {
                // Read the request from the client. Once the client has written to the pipe its security token will be available.
                var ss = new StreamString(pipeServer);

                // Verify our identity to the connected client using a string that the client anticipates.
                ss.WriteString("I am the one true server!");

                string filename = ss.ReadString();

                // Read in the contents of the file while impersonating the client.
                var fileReader = new ReadFileToStream(ss, filename);

                // Display the name of the user we are impersonating.
                Console.WriteLine("Reading file: {0} on thread[{1}] as user: {2}.", filename, threadId, pipeServer.GetImpersonationUserName());
                
                pipeServer.RunAsClient(fileReader.Start);
            }
            catch (IOException e)
            {
                // Catch the IOException that is raised if the pipe is broken or disconnected.
                Console.WriteLine("ERROR: {0}", e.Message);
            }

            pipeServer.Close();
        }
    }
}
