namespace CalradiaReputation
{
    public sealed class ReputationEvent
    {
        public ReputationEventKind Kind { get; set; }
        public double Day { get; set; }
        public ReputationCategory PrimaryCategory { get; set; }
        public float Amount { get; set; }
        public float Difficulty { get; set; }
        public float Importance { get; set; }
        public float EnemyStrength { get; set; }
        public float PlayerInvolvement { get; set; }
        public bool IsLowRisk { get; set; }
        public bool IsLegendary { get; set; }
        public string RegionId { get; set; }
        public string Summary { get; set; }

        public ReputationEvent()
        {
            RegionId = string.Empty;
            Summary = string.Empty;
        }
    }
}
