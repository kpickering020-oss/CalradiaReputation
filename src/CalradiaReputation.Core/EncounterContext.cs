namespace CalradiaReputation
{
    public sealed class EncounterContext
    {
        public double Day { get; set; }
        public float PlayerStrength { get; set; }
        public float EnemyStrength { get; set; }
        public float PlayerInvolvement { get; set; }
        public bool PlayerCommanded { get; set; }
        public bool EnemyIsBandit { get; set; }
        public bool EnemyIsNobleArmy { get; set; }
        public bool IsTown { get; set; }
        public bool IsCastle { get; set; }
        public bool IsVillage { get; set; }
        public int Gold { get; set; }
        public int Profit { get; set; }
        public string RegionId { get; set; }
        public string Summary { get; set; }

        public EncounterContext()
        {
            PlayerStrength = 1f;
            PlayerInvolvement = 0.35f;
            PlayerCommanded = true;
            RegionId = string.Empty;
            Summary = string.Empty;
        }

        public float StrengthRatio()
        {
            float player = PlayerStrength < 1f ? 1f : PlayerStrength;
            return EnemyStrength / player;
        }

        public bool IsOutnumbered()
        {
            return PlayerStrength <= EnemyStrength * ReputationBalance.OutnumberedRatio;
        }

        public bool IsHeavilyOutnumbered()
        {
            return PlayerStrength <= EnemyStrength * ReputationBalance.HeavilyOutnumberedRatio;
        }

        public bool IsTrivialEnemy()
        {
            return EnemyIsBandit || EnemyStrength <= ReputationBalance.TrivialEnemyStrength;
        }
    }
}
