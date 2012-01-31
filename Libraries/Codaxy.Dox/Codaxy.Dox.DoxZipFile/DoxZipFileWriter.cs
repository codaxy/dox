using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Codaxy.Dox.DoxZipFile
{
    public class DoxZipFileWriter : IDoxBuilder, IDisposable
    {
        public DoxZipFileWriter(Stream stream)
        {
            zos = new ZipOutputStream(stream) { UseZip64 = ICSharpCode.SharpZipLib.Zip.UseZip64.Off };
            streamWriter = new StreamWriter(zos);
            jw = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };
        }

        ZipOutputStream zos;
        JsonTextWriter jw;
        StreamWriter streamWriter;

        public void Add(DoxDocument dox)
        {
            var js = new JsonSerializer();
            zos.PutNextEntry(new ZipEntry(dox.FullName + ".json"));
            js.Serialize(jw, dox);
            jw.Flush();
        }

        public void Dispose()
        {
            //if (zos != null)
            //{
            //    zos.Finish();
            //    zos.Close();
            //}
            //if (streamWriter != null)
            //    streamWriter.Dispose();
            if (zos != null)
            {
                zos.Finish();
                zos.Dispose();
            }
        }
    }
}