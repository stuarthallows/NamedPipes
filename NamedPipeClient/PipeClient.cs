using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using NamedPipeShared;

namespace NamedPipeClient
{
    /// <summary>
    /// The following example shows the client process, which uses the NamedPipeClientStream class. The client connects to the server process and sends a file name to the server.
    /// The example uses impersonation, so the identity that is running the client application must have permission to access the file. The server then sends the contents of
    /// the file back to the client. The file contents are then displayed to the console.
    /// </summary>
    public class PipeClient
    {
        private static int numClients = 4;

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("\n*** Named pipe client stream with impersonation example ***\n");
             
                StartClients();
                return;
            }

            if (args[0] == "spawnclient")
            {
                var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);

                Console.WriteLine("Connecting to server...\n");
                pipeClient.Connect();

                var ss = new StreamString(pipeClient);
                
                // Validate the server's signature string.
                if (ss.ReadString() == "I am the one true server!")
                {
                    // The client security token is sent with the first write. Send the name of the file whose contents are returned by the server.
                    ss.WriteString("D:\\textfile.txt");

                    // Print the file to the screen.
                    Console.Write(ss.ReadString());
                }
                else
                {
                    Console.WriteLine("Server could not be verified.");
                }

                pipeClient.Close();

                // Give the client process some time to display results before exiting.
                Thread.Sleep(4000);
            }
        }

        // Helper function to create pipe client processes
        private static void StartClients()
        {
            Console.WriteLine("Spawning client processes...\n");

            string currentProcessName = GetCurrentProcessName();
            var plist = new Process[numClients];

            int i;
            for (i = 0; i < numClients; i++)
            {
                // Start 'this' program but spawn a named pipe client.
                plist[i] = Process.Start(currentProcessName, "spawnclient");
            }

            while (i > 0)
            {
                for (int j = 0; j < numClients; j++)
                {
                    if (plist[j] != null)
                    {
                        if (plist[j].HasExited)
                        {
                            Console.WriteLine($"Client process[{plist[j].Id}] has exited.");
                            plist[j] = null;
                            i--;    // decrement the process watch count
                        }
                        else
                        {
                            Thread.Sleep(250);
                        }
                    }
                }
            }

            Console.WriteLine("\nClient processes finished, exiting.");
        }

        private static string GetCurrentProcessName()
        {
            string currentProcessName = Environment.CommandLine;

            if (currentProcessName.Contains(Environment.CurrentDirectory))
            {
                currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, string.Empty);
            }

            // Remove extra characters when launched from Visual Studio
            currentProcessName = currentProcessName.Replace("\\", string.Empty);
            currentProcessName = currentProcessName.Replace("\"", string.Empty);

            return currentProcessName;
        }
    }
}
