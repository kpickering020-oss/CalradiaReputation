using System;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class LegacyEvent
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Category { get; set; }
        public DateTime TimestampUtc { get; set; }

        public LegacyEvent()
        {
        }

        public LegacyEvent(string title, string summary, string category, DateTime timestampUtc)
        {
            Title = title;
            Summary = summary;
            Category = category;
            TimestampUtc = timestampUtc;
        }
    }
}
