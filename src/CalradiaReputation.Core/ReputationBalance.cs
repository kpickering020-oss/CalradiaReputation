namespace CalradiaReputation
{
    public static class ReputationBalance
    {
        public const int SchemaVersion = 1;
        public const int CategoryCount = 15;
        public const int MaxRecentEvents = 64;
        public const int MaxLegacyEvents = 48;
        public const int MaxNicknameHistory = 24;
        public const int MaxMentionCooldowns = 64;
        public const int RecentWindowDays = 84;
        public const int InactivityDaysBeforeDecay = 21;
        public const float DailyRecentDecay = 0.97f;
        public const float DailyInactiveScoreDecay = 0.995f;
        public const float PeakFloorRatio = 0.35f;
        public const float ReplacementMargin = 15f;
        public const float ClaimReputationWeight = 0.35f;
        public const float ClaimDeedWeight = 0.25f;
        public const float ClaimHistoricalWeight = 0.20f;
        public const float ClaimRecentWeight = 0.20f;
        public const float TrivialEnemyStrength = 18f;
        public const float NobleArmyStrength = 180f;
        public const float OutnumberedRatio = 0.65f;
        public const float HeavilyOutnumberedRatio = 0.45f;
        public const float FarmingPenaltyPerRepeat = 0.12f;
        public const float FarmingFloor = 0.08f;
        public const float CommonResistance = 0f;
        public const float UncommonResistance = 5f;
        public const float RareResistance = 12f;
        public const float LegendaryResistance = 20f;
        public const int CommonSustainDays = 2;
        public const int UncommonSustainDays = 3;
        public const int RareSustainDays = 5;
        public const int LegendarySustainDays = 7;
        public const int MentionCooldownDays = 7;
        public const float AwarenessKnowsThreshold = 0.55f;
        public const float ScoreCap = 100f;
        public const float LegendaryMinScore = 72f;
        public const float RareMinScore = 52f;
        public const float UncommonMinScore = 32f;
        public const float CommonMinScore = 12f;
        public const float BaseVictory = 4.6f;
        public const float BaseDefeat = 6.2f;
        public const float BaseMajorBonus = 5.5f;
        public const float BasePersonalHit = 0.42f;
        public const float BaseSettlementCapture = 8.5f;
        public const float BaseSettlementDefense = 6.0f;
        public const float BaseSettlementLoss = 7.0f;
        public const float BaseRaid = 5.2f;
        public const float BaseMercy = 4.4f;
        public const float BaseCruelty = 5.0f;
        public const float BaseTrade = 3.2f;
        public const float BaseTournament = 4.0f;
        public const float BaseHideout = 3.6f;
        public const float BaseLoyalty = 4.8f;
        public const float BaseTreachery = 6.0f;
        public const float BaseRulership = 5.5f;
        public const float BaseCaptureShame = 4.0f;
        public const float MinDifficulty = 0.12f;
        public const float MaxDifficulty = 2.35f;
        public const float MinImportance = 0.18f;
        public const float MaxImportance = 1.85f;
        public const float CommandInvolvementFloor = 0.28f;
        public const float WarriorInvolvementFloor = 0.08f;
        public const float OppositeBleed = 0.22f;

        public static float ResistanceFor(NicknameRarity rarity)
        {
            switch (rarity)
            {
                case NicknameRarity.Legendary:
                    return LegendaryResistance;
                case NicknameRarity.Rare:
                    return RareResistance;
                case NicknameRarity.Uncommon:
                    return UncommonResistance;
                default:
                    return CommonResistance;
            }
        }

        public static int SustainDaysFor(NicknameRarity rarity)
        {
            switch (rarity)
            {
                case NicknameRarity.Legendary:
                    return LegendarySustainDays;
                case NicknameRarity.Rare:
                    return RareSustainDays;
                case NicknameRarity.Uncommon:
                    return UncommonSustainDays;
                default:
                    return CommonSustainDays;
            }
        }
    }
}
