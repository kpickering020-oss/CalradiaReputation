namespace CalradiaReputation
{
    public sealed class ReputationSaveData
    {
        public int SchemaVersion;
        public string CurrentNicknameId;
        public string PreviousNicknameId;
        public double NicknameEstablishedDay;
        public string PendingChallengerId;
        public int ChallengerStreakDays;
        public bool IsFemale;
        public float Age;
        public double CurrentDay;
        public float[] CurrentScores;
        public float[] PeakScores;
        public float[] RecentScores;
        public double[] LastActivityDays;
        public int[] Stats;
        public int[] EventKinds;
        public double[] EventDays;
        public int[] EventCategories;
        public float[] EventAmounts;
        public float[] EventDifficulties;
        public float[] EventImportances;
        public float[] EventEnemyStrengths;
        public float[] EventInvolvements;
        public int[] EventFlags;
        public string[] EventRegions;
        public string[] EventSummaries;
        public string[] LegacyIds;
        public string[] LegacySummaries;
        public double[] LegacyDays;
        public int[] LegacyCategories;
        public float[] LegacyWeights;
        public string[] HistoryIds;
        public double[] HistoryAcquired;
        public double[] HistoryLost;
        public float[] HistoryScores;
        public string[] EraNames;
        public double[] EraStarts;
        public double[] EraEnds;
        public int[] EraCategories;
        public string DominantEraName;
        public int LifetimeDominant;
        public int RecentDominant;
        public string[] MentionHeroIds;
        public double[] MentionDays;
        public string[] AwareHeroIds;

        public ReputationSaveData()
        {
            CurrentNicknameId = string.Empty;
            PreviousNicknameId = string.Empty;
            PendingChallengerId = string.Empty;
            DominantEraName = "Unknown";
            CurrentScores = new float[0];
            PeakScores = new float[0];
            RecentScores = new float[0];
            LastActivityDays = new double[0];
            Stats = new int[0];
            EventKinds = new int[0];
            EventDays = new double[0];
            EventCategories = new int[0];
            EventAmounts = new float[0];
            EventDifficulties = new float[0];
            EventImportances = new float[0];
            EventEnemyStrengths = new float[0];
            EventInvolvements = new float[0];
            EventFlags = new int[0];
            EventRegions = new string[0];
            EventSummaries = new string[0];
            LegacyIds = new string[0];
            LegacySummaries = new string[0];
            LegacyDays = new double[0];
            LegacyCategories = new int[0];
            LegacyWeights = new float[0];
            HistoryIds = new string[0];
            HistoryAcquired = new double[0];
            HistoryLost = new double[0];
            HistoryScores = new float[0];
            EraNames = new string[0];
            EraStarts = new double[0];
            EraEnds = new double[0];
            EraCategories = new int[0];
            MentionHeroIds = new string[0];
            MentionDays = new double[0];
            AwareHeroIds = new string[0];
        }
    }
}
