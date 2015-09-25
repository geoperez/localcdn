using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LocalCdn
{
    public class Entry
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public static class EntryRepository
    {
        private static readonly List<Entry> Entries = new List<Entry>();

        static EntryRepository()
        {
#if DEBUG
            Add(new Entry {Name = "Test", Url = "http://localhost/app.js"});
            Add(new Entry {Name = "Jquery", Url = "http://localhost/jquery.js"});
#endif
        }

        public static void Add(Entry entry)
        {
            Entries.Add(entry);
        }

        public static IQueryable<Entry> GetAll()
        {
            return Entries.AsQueryable();
        }
    }
}