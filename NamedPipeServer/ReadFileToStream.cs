using System.IO;
using NamedPipeShared;

namespace NamedPipeServer
{
    // Contains the method executed in the context of the impersonated user
    public class ReadFileToStream
    {
        private readonly string _fn;
        private readonly StreamString _ss;

        public ReadFileToStream(StreamString str, string filename)
        {
            _fn = filename;
            _ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(_fn);
            _ss.WriteString(contents);
        }
    }
}