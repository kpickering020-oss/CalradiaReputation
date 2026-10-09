namespace CalradiaReputation
{
    public sealed class NicknameClaim
    {
        public NicknameDefinition Nickname { get; }
        public float ReputationStrength { get; }
        public float DeedStrength { get; }
        public float HistoricalRelevance { get; }
        public float RecentRelevance { get; }
        public float Bonuses { get; }
        public float Score { get; }
        public bool Eligible { get; }

        public NicknameClaim(
            NicknameDefinition nickname,
            float reputationStrength,
            float deedStrength,
            float historicalRelevance,
            float recentRelevance,
            float bonuses,
            bool eligible)
        {
            Nickname = nickname;
            ReputationStrength = Clamp(reputationStrength);
            DeedStrength = Clamp(deedStrength);
            HistoricalRelevance = Clamp(historicalRelevance);
            RecentRelevance = Clamp(recentRelevance);
            Bonuses = bonuses;
            Eligible = eligible;
            Score = Clamp(
                (ReputationBalance.ClaimReputationWeight * ReputationStrength)
                + (ReputationBalance.ClaimDeedWeight * DeedStrength)
                + (ReputationBalance.ClaimHistoricalWeight * HistoricalRelevance)
                + (ReputationBalance.ClaimRecentWeight * RecentRelevance)
                + Bonuses);
        }

        private static float Clamp(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > ReputationBalance.ScoreCap ? ReputationBalance.ScoreCap : value;
        }
    }
}
