using System;
using System.IO.Pipes;
using System.Threading;
using NamedPipeShared;

namespace NamedPipeClient
{
    /// <summary>
    /// 1. Open up the client side of a named pipe connection.
    /// 2. Send a number of strings to the server (each representing a tag).
    /// 3. Send an empty string to indicate data is complete.
    /// </summary>
    public class PipeClient
    {
        public static void Main()
        {
            Console.WriteLine("*** Named Pipes Client ***");
            Console.WriteLine("Enter pipe name and press ENTER");

            var pipeName = Console.ReadLine();
            if (string.IsNullOrEmpty(pipeName))
            {
                pipeName = "ChipID";
                Console.WriteLine($"Using default pipe name of \"{pipeName}\"");
            }

            Console.WriteLine("Connecting to server...\n");

            var pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
            pipeClient.Connect();

            var ss = new StreamString(pipeClient);
            
            for (var n = 0; n < 10; n++)
            {
                var tagId = RandomTagId();

                ss.WriteString(tagId);
                Console.WriteLine($"Sent tag {tagId}");

                Thread.Sleep(1000);
            }

            // Indicates to the server that the client has no more data to send.
            ss.WriteString(string.Empty);

            pipeClient.Close();

            // Keep the console open for a while.
            Thread.Sleep(2000);
        }

        private static readonly Random Random = new Random();

        private static string RandomTagId()
        {
            const string candidateChars = "0123456789ABCDEF";

            var randomChars = new char[24];
            var prefix = "E2801160";

            for (var i = 0; i < prefix.Length; ++i)
            {
                randomChars[i] = prefix[i];
            }

            for (var i = prefix.Length; i < randomChars.Length; i++)
            {
                randomChars[i] = candidateChars[Random.Next(candidateChars.Length)];
            }

            return new string(randomChars);
        }
    }
}
