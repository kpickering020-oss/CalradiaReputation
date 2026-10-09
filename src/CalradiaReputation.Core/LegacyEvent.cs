namespace CalradiaReputation
{
    public sealed class LegacyEvent
    {
        public string Id { get; set; }
        public string Summary { get; set; }
        public double Day { get; set; }
        public ReputationCategory Category { get; set; }
        public float Weight { get; set; }

        public LegacyEvent()
        {
            Id = string.Empty;
            Summary = string.Empty;
        }
    }
}
