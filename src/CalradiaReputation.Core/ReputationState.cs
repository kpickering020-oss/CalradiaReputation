using System.Collections.Generic;

namespace CalradiaReputation
{
    public sealed class ReputationState
    {
        public int SchemaVersion { get; set; }
        public string CurrentNicknameId { get; set; }
        public string PreviousNicknameId { get; set; }
        public double NicknameEstablishedDay { get; set; }
        public string PendingChallengerId { get; set; }
        public int ChallengerStreakDays { get; set; }
        public bool IsFemale { get; set; }
        public float Age { get; set; }
        public double CurrentDay { get; set; }
        public CategoryScore[] Categories { get; }
        public LifetimeStatistics Stats { get; }
        public List<ReputationEvent> RecentEvents { get; }
        public List<LegacyEvent> LegacyEvents { get; }
        public List<NicknameHistoryEntry> NicknameHistory { get; }
        public List<AwarenessRecord> Awareness { get; }
        public CareerHistory Career { get; }

        public ReputationState()
        {
            SchemaVersion = ReputationBalance.SchemaVersion;
            CurrentNicknameId = string.Empty;
            PreviousNicknameId = string.Empty;
            PendingChallengerId = string.Empty;
            Age = 22f;
            Categories = new CategoryScore[ReputationBalance.CategoryCount];
            for (int i = 0; i < Categories.Length; i++)
            {
                Categories[i] = new CategoryScore();
            }

            Stats = new LifetimeStatistics();
            RecentEvents = new List<ReputationEvent>();
            LegacyEvents = new List<LegacyEvent>();
            NicknameHistory = new List<NicknameHistoryEntry>();
            Awareness = new List<AwarenessRecord>();
            Career = new CareerHistory();
        }

        public CategoryScore Score(ReputationCategory category)
        {
            return Categories[(int)category];
        }

        public bool HasLegacy(string id)
        {
            for (int i = 0; i < LegacyEvents.Count; i++)
            {
                if (LegacyEvents[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HeldNickname(string id)
        {
            if (CurrentNicknameId == id || PreviousNicknameId == id)
            {
                return true;
            }

            for (int i = 0; i < NicknameHistory.Count; i++)
            {
                if (NicknameHistory[i].NicknameId == id)
                {
                    return true;
                }
            }

            return false;
        }

        public ReputationCategory DominantCategory(bool recent)
        {
            int best = 0;
            float bestValue = -1f;
            for (int i = 0; i < Categories.Length; i++)
            {
                float value = recent ? Categories[i].Recent : Categories[i].Current;
                if (value > bestValue)
                {
                    bestValue = value;
                    best = i;
                }
            }

            return (ReputationCategory)best;
        }

        public void AddLegacy(string id, string summary, ReputationCategory category, float weight, double day)
        {
            if (HasLegacy(id))
            {
                return;
            }

            LegacyEvents.Add(new LegacyEvent
            {
                Id = id,
                Summary = summary,
                Category = category,
                Weight = weight,
                Day = day
            });

            if (LegacyEvents.Count > ReputationBalance.MaxLegacyEvents)
            {
                LegacyEvents.RemoveAt(0);
            }
        }

        public AwarenessRecord GetAwareness(string heroId)
        {
            if (string.IsNullOrEmpty(heroId))
            {
                return null;
            }

            for (int i = 0; i < Awareness.Count; i++)
            {
                if (Awareness[i].HeroId == heroId)
                {
                    return Awareness[i];
                }
            }

            AwarenessRecord created = new AwarenessRecord { HeroId = heroId };
            Awareness.Add(created);
            if (Awareness.Count > ReputationBalance.MaxMentionCooldowns)
            {
                Awareness.RemoveAt(0);
            }

            return created;
        }

        public void SetScore(ReputationCategory category, float current, float peak, float recent)
        {
            CategoryScore score = Score(category);
            score.Current = current;
            score.Peak = peak;
            score.Recent = recent;
            score.LastActivityDay = CurrentDay;
        }
    }
}
