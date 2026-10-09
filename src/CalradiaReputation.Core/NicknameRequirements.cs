namespace CalradiaReputation
{
    public sealed class NicknameRequirements
    {
        public ReputationCategory FocusCategory { get; set; }
        public float MinFocusScore { get; set; }
        public ReputationCategory? MustOutrankCategory { get; set; }
        public float MinPeakFocus { get; set; }
        public float MinRecentFocus { get; set; }
        public float MinAge { get; set; }
        public float MaxAge { get; set; }
        public int MinCommandVictories { get; set; }
        public int MinMajorVictories { get; set; }
        public int MinMajorDefeats { get; set; }
        public int MinOutnumberedVictories { get; set; }
        public int MaxMajorDefeats { get; set; }
        public int MinPersonalHits { get; set; }
        public int MinTrivialVictories { get; set; }
        public int MaxTrivialVictorySharePercent { get; set; }
        public int MinNoblesReleased { get; set; }
        public int MinNoblesExecuted { get; set; }
        public int MinVillagesRaided { get; set; }
        public int MinTownsCaptured { get; set; }
        public int MinTournamentsWon { get; set; }
        public int MinGold { get; set; }
        public int MinTradeProfit { get; set; }
        public int MinTimesCaptured { get; set; }
        public int MinConsecutiveMajorDefeats { get; set; }
        public int MinSettlementsLost { get; set; }
        public int MinSettlementsOwnedPeak { get; set; }
        public int MinDaysSinceLastBattle { get; set; }
        public string[] RequiredLegacyIds { get; set; }
        public string[] RequiredPreviousIds { get; set; }
        public string[] EvolvesFromIds { get; set; }

        public NicknameRequirements()
        {
            MaxAge = 0f;
            MaxMajorDefeats = int.MaxValue;
            MaxTrivialVictorySharePercent = 100;
            RequiredLegacyIds = new string[0];
            RequiredPreviousIds = new string[0];
            EvolvesFromIds = new string[0];
        }
    }
}
