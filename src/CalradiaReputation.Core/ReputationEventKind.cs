namespace CalradiaReputation
{
    public enum ReputationEventKind
    {
        Unknown = 0,
        PersonalCombatHit = 1,
        BattleVictory = 2,
        BattleDefeat = 3,
        MajorBattleVictory = 4,
        MajorBattleDefeat = 5,
        OutnumberedVictory = 6,
        TrivialVictory = 7,
        SettlementCaptured = 8,
        SettlementLost = 9,
        SettlementDefended = 10,
        NobleReleased = 11,
        NobleExecuted = 12,
        VillageRaided = 13,
        RaidDefended = 14,
        TradeProfit = 15,
        WealthSnapshot = 16,
        TournamentWin = 17,
        TournamentLoss = 18,
        FactionJoined = 19,
        FactionDefected = 20,
        PlayerCaptured = 21,
        HideoutCleared = 22,
        CrimeIncreased = 23,
        BecameRuler = 24,
        CaravanIncome = 25,
        WorkshopIncome = 26,
        PrisonersRecruited = 27,
        MercyShown = 28
    }
}
