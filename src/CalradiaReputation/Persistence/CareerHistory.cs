using System;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class ReputationEvent
    {
        public string Metric { get; set; }
        public float Value { get; set; }
        public float Significance { get; set; }
        public string Source { get; set; }
        public DateTime TimestampUtc { get; set; }

        public ReputationEvent()
        {
        }

        public ReputationEvent(string metric, float value, float significance, string source, DateTime timestampUtc)
        {
            Metric = metric;
            Value = value;
            Significance = significance;
            Source = source;
            TimestampUtc = timestampUtc;
        }
    }
}
