using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Codaxy.Dox.DoxZipFile
{
    public class DoxZipFileReader : IDoxProvider
    {
        Stream stream;

        public DoxZipFileReader(Stream inputStream)
        {
            stream = inputStream;
        }

        public void Process(IDoxBuilder builder)
        {
            var js = new JsonSerializer();
            using (ZipFile zf = new ZipFile(stream))
                foreach (ZipEntry ze in zf)
                {
                    using (var istream = zf.GetInputStream(ze))
                    using (var streamReader = new StreamReader(istream))
                    {
                        //uncomment for debugging
                        //var text = streamReader.ReadToEnd();
                        //using (var stringReader = new StringReader(text))
                        //using (var jtr = new JsonTextReader(stringReader))
                        using (var jtr = new JsonTextReader(streamReader))
                        {
                            var dox = js.Deserialize<DoxDocument>(jtr);
                            builder.Add(dox);
                        }
                    }
                }
        }
    }
}