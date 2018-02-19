using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Funq;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.FileSystem;
using ServiceStack.Auth;
using System.Threading.Tasks;
using Raven.Abstractions.Extensions;
using Raven.Json.Linq;
using System.IO;
using System.Reflection;
using System.IO.Compression;

namespace Raven_SS_FileDownloadTesting_Sandbox
{
    public class Global : System.Web.HttpApplication
    {

        public class AppHost : AppHostBase
        {
            public AppHost() : base("Sandbox", typeof(DocumentService).Assembly) { }

            public override void Configure(Container container)
            {
                Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[] 
                {
                    new AspNetWindowsAuthProvider(this)
                    {
                        AllowAllWindowsAuthUsers = true
                    }
                }));
            }
        }


        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }
        
        [Route("/s/download-adhoc-file")]
        public class AdhocDocumentRequest
        {

        }

        public class DocumentService : Service
        {
            public object Get(AdhocDocumentRequest request)
            {
                var stream = LoadAdhocFile("pdf-sample.pdf");

                Response.AddHeader("Content-Type", "application/pdf");
                Response.AddHeader("Content-Disposition", "attachment; filename=test.pdf");

                return stream;
            }

            private Stream LoadAdhocFile(string fileName)
            {
                var readFrom = Path.Combine(AssemblyDirectory, fileName);
                var target = string.Concat(readFrom, ".gz");

                Compress(readFrom, target);

                var bs = new BufferedStream(File.OpenRead(target), 8192);

                return new GZipStream(bs, CompressionMode.Decompress);
            }

            private void Compress(string readFrom, string writeTo)
            {
                byte[] b;
                using (FileStream f = new FileStream(readFrom, FileMode.Open))
                {
                    b = new byte[f.Length];
                    f.Read(b, 0, (int)f.Length);
                }

                using (var fs = new FileStream(writeTo, FileMode.OpenOrCreate))
                using (var gz = new GZipStream(fs, CompressionMode.Compress, false))
                {
                    gz.Write(b, 0, b.Length);
                }
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}