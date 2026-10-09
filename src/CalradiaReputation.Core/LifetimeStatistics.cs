namespace CalradiaReputation
{
    public sealed class LifetimeStatistics
    {
        public int PersonalKills { get; set; }
        public int PersonalHits { get; set; }
        public int CommandVictories { get; set; }
        public int CommandDefeats { get; set; }
        public int MajorVictories { get; set; }
        public int MajorDefeats { get; set; }
        public int OutnumberedVictories { get; set; }
        public int TrivialVictories { get; set; }
        public int NoblesReleased { get; set; }
        public int NoblesExecuted { get; set; }
        public int VillagesRaided { get; set; }
        public int TownsCaptured { get; set; }
        public int CastlesCaptured { get; set; }
        public int SettlementsLost { get; set; }
        public int SettlementsDefended { get; set; }
        public int TournamentsWon { get; set; }
        public int TournamentsLost { get; set; }
        public int GoldPeak { get; set; }
        public int CurrentGold { get; set; }
        public int TradeProfit { get; set; }
        public int FactionJoins { get; set; }
        public int FactionDefections { get; set; }
        public int TimesCaptured { get; set; }
        public int HideoutsCleared { get; set; }
        public int CaravansOwnedPeak { get; set; }
        public int SettlementsOwnedPeak { get; set; }
        public int DaysAsRuler { get; set; }
        public int BanditFights { get; set; }
        public int NobleArmyFights { get; set; }
        public int ConsecutiveMajorDefeats { get; set; }
        public int ConsecutiveVictories { get; set; }
        public int DaysSinceLastBattle { get; set; }
        public int DaysSinceLastTrade { get; set; }

        public LifetimeStatistics Clone()
        {
            return (LifetimeStatistics)MemberwiseClone();
        }
    }
}
