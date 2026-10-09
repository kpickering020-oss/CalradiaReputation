using System;
using System.Collections.Generic;
using System.Linq;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class ReputationState
    {
        public const int SchemaVersionValue = 1;

        public int SchemaVersion { get; set; } = SchemaVersionValue;
        public Dictionary<string, float> Scores { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, float> HistoricalPeaks { get; set; } = new Dictionary<string, float>();
        public Dictionary<string, int> LifetimeCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RecentActivity { get; set; } = new Dictionary<string, int>();
        public List<ReputationEvent> RecentEvents { get; set; } = new List<ReputationEvent>();
        public List<LegacyEvent> LegendaryEvents { get; set; } = new List<LegacyEvent>();
        public CareerHistory CareerHistory { get; set; } = new CareerHistory();
        public string CurrentNicknameId { get; set; }
        public string PreviousNicknameId { get; set; }
        public List<NicknameHistoryEntry> NicknameHistory { get; set; } = new List<NicknameHistoryEntry>();
        public int LastKnownCampaignDay { get; set; }
        public int Age { get; set; }
        public int LastClaimedTurn { get; set; }

        public void EnsureInitialized()
        {
            if (Scores == null)
            {
                Scores = new Dictionary<string, float>();
            }

            if (HistoricalPeaks == null)
            {
                HistoricalPeaks = new Dictionary<string, float>();
            }

            if (LifetimeCounts == null)
            {
                LifetimeCounts = new Dictionary<string, int>();
            }

            if (RecentActivity == null)
            {
                RecentActivity = new Dictionary<string, int>();
            }

            if (RecentEvents == null)
            {
                RecentEvents = new List<ReputationEvent>();
            }

            if (LegendaryEvents == null)
            {
                LegendaryEvents = new List<LegacyEvent>();
            }

            if (CareerHistory == null)
            {
                CareerHistory = new CareerHistory();
            }

            if (NicknameHistory == null)
            {
                NicknameHistory = new List<NicknameHistoryEntry>();
            }

            if (SchemaVersion <= 0)
            {
                SchemaVersion = SchemaVersionValue;
            }

            foreach (var metricName in ReputationMetrics.AllMetricNames)
            {
                if (!Scores.ContainsKey(metricName))
                {
                    Scores[metricName] = 0f;
                }

                if (!HistoricalPeaks.ContainsKey(metricName))
                {
                    HistoricalPeaks[metricName] = 0f;
                }

                if (!LifetimeCounts.ContainsKey(metricName))
                {
                    LifetimeCounts[metricName] = 0;
                }

                if (!RecentActivity.ContainsKey(metricName))
                {
                    RecentActivity[metricName] = 0;
                }
            }
        }

        public void AddScore(string metric, float delta, float significance = 1f, string source = null)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(metric))
            {
                return;
            }

            var normalized = delta * Math.Max(0.1f, significance);
            Scores[metric] = Scores.ContainsKey(metric) ? Scores[metric] + normalized : normalized;
            HistoricalPeaks[metric] = Math.Max(HistoricalPeaks.ContainsKey(metric) ? HistoricalPeaks[metric] : 0f, Scores[metric]);
            LifetimeCounts[metric] = LifetimeCounts.ContainsKey(metric) ? LifetimeCounts[metric] + 1 : 1;
            RecentActivity[metric] = RecentActivity.ContainsKey(metric) ? RecentActivity[metric] + 1 : 1;
            RecentEvents.Add(new ReputationEvent(metric, normalized, significance, source, DateTime.UtcNow));

            while (RecentEvents.Count > 64)
            {
                RecentEvents.RemoveAt(0);
            }
        }

        public void TrackDailyDecay()
        {
            EnsureInitialized();

            foreach (var metricName in ReputationMetrics.AllMetricNames)
            {
                if (RecentActivity.TryGetValue(metricName, out var activityCount) && activityCount <= 0)
                {
                    var current = Scores[metricName];
                    Scores[metricName] = Math.Max(0f, current - 0.05f);
                }
                else
                {
                    RecentActivity[metricName] = Math.Max(0, activityCount - 1);
                }
            }
        }

        public float GetMetric(string metric)
        {
            EnsureInitialized();
            return Scores.ContainsKey(metric) ? Scores[metric] : 0f;
        }
    }

    public static class ReputationMetrics
    {
        public const string Warrior = "Warrior";
        public const string Generalship = "Generalship";
        public const string Conquest = "Conquest";
        public const string Defender = "Defender";
        public const string Mercy = "Mercy";
        public const string Cruelty = "Cruelty";
        public const string Raider = "Raider";
        public const string Wealth = "Wealth";
        public const string Trade = "Trade";
        public const string Tournament = "Tournament";
        public const string Loyalty = "Loyalty";
        public const string Treachery = "Treachery";
        public const string Outlaw = "Outlaw";
        public const string Rulership = "Rulership";
        public const string Charisma = "Charisma";

        public static readonly string[] AllMetricNames =
        {
            Warrior,
            Generalship,
            Conquest,
            Defender,
            Mercy,
            Cruelty,
            Raider,
            Wealth,
            Trade,
            Tournament,
            Loyalty,
            Treachery,
            Outlaw,
            Rulership,
            Charisma
        };
    }
}
