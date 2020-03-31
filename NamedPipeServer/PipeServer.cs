using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using NamedPipeShared;

namespace NamedPipeServer
{
    /// <summary>
    /// 1. Open the server side of a named pipe stream.
    /// 2. Read a number of strings from the pipe.
    /// 3. Close the connection when an empty string is received.
    /// </summary>
    public class PipeServer
    {
        public static void Main()
        {
            Console.WriteLine("*** Named Pipes Server ***");
            Console.WriteLine("Waiting for client to connect...");

            // Server can only receive input with this pipe, it's not duplex.
            var pipeServer = new NamedPipeServerStream("RfidTags", PipeDirection.In, 1);

            // Wait for a client to connect
            pipeServer.WaitForConnection();

            Console.WriteLine("Client connected\n");
            try
            {
                // For reading data from the client.
                var ss = new StreamString(pipeServer);

                while (true)
                {
                    string tagId = ss.ReadString();

                    // Empty string received from client is the trigger to close the connection.
                    if (string.IsNullOrEmpty(tagId))
                    {
                        break;
                    }

                    Console.WriteLine($"Received tag {tagId}");
                }
            }
            catch (IOException e)
            {
                // Catch the exception that is raised if the pipe is broken or disconnected.
                Console.WriteLine($"ERROR: {e.Message}");
            }

            pipeServer.Close();

            // Keep the console open for a while.
            Thread.Sleep(2000);
        }
    }
}
