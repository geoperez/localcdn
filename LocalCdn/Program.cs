using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LocalCdn.Properties;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Log;
using Unosquare.Labs.EmbedIO.Modules;

namespace LocalCdn
{
    class Program
    {
        /// <summary>
        /// This app should run as Admin because it needs to open port 80
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            try
            {
                using (var server = new WebServer("http://*:80/", new SimpleConsoleLog()))
                {
                    var hostFile = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");

                    if (File.Exists(hostFile) == false)
                    {
                        server.Log.Error("Host file is missing");
                        return;
                    }

                    var originalHost = File.ReadAllText(hostFile);

                    var hostEntries =
                        File.ReadAllLines(hostFile)
                            .Where(x => x.StartsWith("127.0.0.1"))
                            .Select(x => x.Split('\t'))
                            .Where(x => x.Length == 2)
                            .Select(x => x[1]).ToList();

                    var tmpFolder = Path.Combine(Path.GetTempPath(), "localcdn");

                    if (Directory.Exists(tmpFolder) == false)
                    {
                        Directory.CreateDirectory(tmpFolder);
                    }

                    server.WithWebApiController<ApiController>();
                    server.RegisterModule(new StaticFilesModule(tmpFolder));

                    server.Module<StaticFilesModule>().UseRamCache = true;
                    server.Module<StaticFilesModule>().DefaultExtension = ".html";

                    File.WriteAllText(Path.Combine(tmpFolder, "index.html"), Resources.index);
                    File.WriteAllText(Path.Combine(tmpFolder, "app.js"), Resources.app);
                    File.WriteAllText(Path.Combine(tmpFolder, "app.jsx"), Resources.appjsx);
                    File.WriteAllText(Path.Combine(tmpFolder, "jquery.js"), Resources.jquery_2_1_4_min);
                    File.WriteAllText(Path.Combine(tmpFolder, "JSXTransformer.js"), Resources.JSXTransformer);
                    File.WriteAllText(Path.Combine(tmpFolder, "react.js"),
                        Resources.react_with_addons_min);

                    server.RunAsync();

                    while (true)
                    {
                        Console.WriteLine("Type an Url to add or press Enter to stop...");
                        var result = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(result)) break;

                        try
                        {
                            var url = new Uri(result);
                            var webClient = new WebClient();

                            var path = Path.Combine(tmpFolder,
                                url.AbsolutePath.Substring(1, url.AbsolutePath.Length - 1).Replace("/", "\\"));

                            Directory.CreateDirectory(path.Substring(0, path.LastIndexOf('\\')));

                            webClient.DownloadFile(url, path);

                            var host = url.Host;

                            if (hostEntries.Contains(host) == false)
                            {
                                hostEntries.Add(host);
                                File.AppendAllText(hostFile, $"\r\n127.0.0.1\t{host}");
                            }
                        }
                        catch (Exception ex)
                        {
                            server.Log.Error(ex.Message);
                        }
                    }

                    var currentHost = File.ReadAllText(hostFile);

                    // Restore
                    if (originalHost != currentHost) File.WriteAllText(hostFile, originalHost);
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
