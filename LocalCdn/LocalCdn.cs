using LocalCdn.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Unosquare.Labs.EmbedIO;
using Unosquare.Labs.EmbedIO.Log;
using Unosquare.Labs.EmbedIO.Modules;

namespace LocalCdn
{
    internal class LocalCdn
    {
        private static readonly string HostFile = Path.Combine(Environment.SystemDirectory, "drivers", "etc", "hosts");
        private static readonly string TmpFolder = Path.Combine(Path.GetTempPath(), "localcdn");
        private static readonly string DbFile = "db.json";

        private static readonly Regex cssUrl =
            new Regex(@"@import ([""'])(?<url>[^""']+)\1|url\(([""']?)(?<url>[^""')]+)\2\)", RegexOptions.IgnoreCase);

        public static readonly List<string> HostEntries = new List<string>();
        private static readonly string OriginalHostFile;
        private static WebServer _server;

        static LocalCdn()
        {
            // INIT
            if (Directory.Exists(TmpFolder) == false)
            {
                Directory.CreateDirectory(TmpFolder);
            }

            if (File.Exists(HostFile) == false)
            {
                _server.Log.Error("Host file is missing");
                return;
            }

            OriginalHostFile = File.ReadAllText(HostFile);

            HostEntries.AddRange(
                File.ReadAllLines(HostFile)
                    .Where(x => x.StartsWith("127.0.0.1"))
                    .Select(x => x.Split('\t'))
                    .Where(x => x.Length == 2)
                    .Select(x => x[1]).ToList());

            if (File.Exists(DbFile)) ParseLocalFile();
        }

        private static void ParseLocalFile()
        {
            var content = JsonConvert.DeserializeObject<EntryCollection>(File.ReadAllText(DbFile));

            if (content?.Entries == null || content.Entries.Count == 0) return;

            foreach (var c in content.Entries)
            {
                Console.WriteLine($"Checking {c.Name}");

                if (File.Exists(Path.Combine(TmpFolder, c.Name)) == false)
                {
                    AddEntry(c.Url);
                }
                else
                {
                    var url = new Uri(c.Url);

                    if (HostEntries.Contains(url.Host) == false)
                    {
                        HostEntries.Add(url.Host);
                    }

                    ReapplyAll();

                    EntryRepository.Add(new Entry { Name = c.Name, Url = url.ToString() });
                }
            }
        }

        public static void RunServer()
        {
            try
            {
                using (_server = new WebServer("http://*:80/", new SimpleConsoleLog()))
                {
                    _server.WithWebApiController<ApiController>();
                    _server.RegisterModule(new CorsModule());
                    _server.RegisterModule(new StaticFilesModule(TmpFolder));

                    _server.Module<StaticFilesModule>().UseRamCache = true;
                    _server.Module<StaticFilesModule>().DefaultExtension = ".html";

                    File.WriteAllText(Path.Combine(TmpFolder, "index.html"), Resources.index);
                    File.WriteAllText(Path.Combine(TmpFolder, "app.js"), Resources.app);
                    File.WriteAllText(Path.Combine(TmpFolder, "app.jsx"), Resources.appjsx);
                    File.WriteAllText(Path.Combine(TmpFolder, "jquery.js"), Resources.jquery_2_1_4_min);
                    File.WriteAllText(Path.Combine(TmpFolder, "JSXTransformer.js"), Resources.JSXTransformer);
                    File.WriteAllText(Path.Combine(TmpFolder, "react.js"),
                        Resources.react_with_addons_min);

                    _server.RunAsync();
                    
                    while (true)
                    {
                        Console.WriteLine("Type an Url to add or press Enter to stop...");
                        var result = Console.ReadLine();

                        if (string.IsNullOrWhiteSpace(result)) break;

                        AddEntry(result);
                    }

                    var currentHost = File.ReadAllText(HostFile);

                    // Restore
                    if (OriginalHostFile != currentHost) File.WriteAllText(HostFile, OriginalHostFile);
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
        }

        public static void AddEntry(string result)
        {
            try
            {
                Console.WriteLine($"New file: {result}");

                var url = new Uri(result);
                var host = url.Host;
                var webClient = new WebClient();
                var filePath = url.AbsolutePath.Substring(1, url.AbsolutePath.Length - 1).Replace("/", "\\");
                var path = Path.Combine(TmpFolder, filePath);
                var directoryPath = path.Substring(0, path.LastIndexOf('\\'));

                Directory.CreateDirectory(directoryPath);

                VerifyDomain(host);

                Retry.Do(() => webClient.DownloadFile(url, path), TimeSpan.FromSeconds(5));
                
                if (HostEntries.Contains(host) == false)
                {
                    HostEntries.Add(host);
                }

                ReapplyAll();

                EntryRepository.Add(new Entry {Name = filePath, Url = url.ToString()});

                if (filePath.EndsWith("css"))
                {
                    var contentFile = File.ReadAllText(path);

                    foreach (Match item in cssUrl.Matches(contentFile))
                    {
                        var value = item.Groups[3].Value;

                        if (string.IsNullOrWhiteSpace(value) || value.StartsWith("data:")) continue;

                        Console.WriteLine($"New CSS Import {value}");

                        if (value.StartsWith("."))
                        {
                            value = url.ToString().Substring(0, url.ToString().LastIndexOf("/")) + "/" + value;
                            Console.WriteLine($"New CSS Import {value}");
                        }

                        AddEntry(value);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.BackgroundColor = ConsoleColor.Black;
            }
        }

        private static void VerifyDomain(string host)
        {
            if (HostEntries.Contains(host))
            {
                // Revert HOST file before to continue
                File.WriteAllText(HostFile, OriginalHostFile);
            }
        }

        private static void ReapplyAll()
        {
            File.WriteAllText(HostFile, OriginalHostFile);
            File.AppendAllLines(HostFile, HostEntries.Select(host => $"\r\n127.0.0.1\t{host}"));
        }
    }
}