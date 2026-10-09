using System;
using System.Collections.Generic;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class CareerHistory
    {
        public List<LegacyEvent> LifelongEvents { get; set; } = new List<LegacyEvent>();
        public List<string> CareerEras { get; set; } = new List<string>();

        public void Record(string title, string summary, string category)
        {
            LifelongEvents.Add(new LegacyEvent(title, summary, category, DateTime.UtcNow));
        }
    }
}
