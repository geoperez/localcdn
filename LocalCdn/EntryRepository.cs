using System.Collections.Generic;
using System.Linq;
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

    public class EntryCollection
    {
        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }
    }

    public static class EntryRepository
    {
        private static readonly List<Entry> Entries = new List<Entry>();
        
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