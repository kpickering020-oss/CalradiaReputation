using System;

namespace CalradiaReputation
{
    public static class ReputationSaveCodec
    {
        public static ReputationSaveData Pack(ReputationState state)
        {
            ReputationSaveData data = new ReputationSaveData();
            if (state == null)
            {
                return data;
            }

            data.SchemaVersion = state.SchemaVersion;
            data.CurrentNicknameId = state.CurrentNicknameId ?? string.Empty;
            data.PreviousNicknameId = state.PreviousNicknameId ?? string.Empty;
            data.NicknameEstablishedDay = state.NicknameEstablishedDay;
            data.PendingChallengerId = state.PendingChallengerId ?? string.Empty;
            data.ChallengerStreakDays = state.ChallengerStreakDays;
            data.IsFemale = state.IsFemale;
            data.Age = state.Age;
            data.CurrentDay = state.CurrentDay;
            data.CurrentScores = new float[ReputationBalance.CategoryCount];
            data.PeakScores = new float[ReputationBalance.CategoryCount];
            data.RecentScores = new float[ReputationBalance.CategoryCount];
            data.LastActivityDays = new double[ReputationBalance.CategoryCount];
            for (int i = 0; i < ReputationBalance.CategoryCount; i++)
            {
                CategoryScore score = state.Categories[i];
                data.CurrentScores[i] = score.Current;
                data.PeakScores[i] = score.Peak;
                data.RecentScores[i] = score.Recent;
                data.LastActivityDays[i] = score.LastActivityDay;
            }

            data.Stats = PackStats(state.Stats);
            PackEvents(state, data);
            PackLegacy(state, data);
            PackHistory(state, data);
            PackEras(state, data);
            PackAwareness(state, data);
            data.DominantEraName = state.Career.DominantEraName ?? "Unknown";
            data.LifetimeDominant = (int)state.Career.LifetimeDominant;
            data.RecentDominant = (int)state.Career.RecentDominant;
            return data;
        }

        public static void Unpack(ReputationState state, ReputationSaveData data)
        {
            if (state == null || data == null)
            {
                return;
            }

            state.SchemaVersion = data.SchemaVersion == 0 ? ReputationBalance.SchemaVersion : data.SchemaVersion;
            state.CurrentNicknameId = data.CurrentNicknameId ?? string.Empty;
            state.PreviousNicknameId = data.PreviousNicknameId ?? string.Empty;
            state.NicknameEstablishedDay = data.NicknameEstablishedDay;
            state.PendingChallengerId = data.PendingChallengerId ?? string.Empty;
            state.ChallengerStreakDays = data.ChallengerStreakDays;
            state.IsFemale = data.IsFemale;
            state.Age = data.Age;
            state.CurrentDay = data.CurrentDay;
            for (int i = 0; i < ReputationBalance.CategoryCount; i++)
            {
                CategoryScore score = state.Categories[i];
                score.Current = ValueAt(data.CurrentScores, i);
                score.Peak = ValueAt(data.PeakScores, i);
                score.Recent = ValueAt(data.RecentScores, i);
                score.LastActivityDay = ValueAt(data.LastActivityDays, i);
            }

            UnpackStats(state.Stats, data.Stats);
            UnpackEvents(state, data);
            UnpackLegacy(state, data);
            UnpackHistory(state, data);
            UnpackEras(state, data);
            UnpackAwareness(state, data);
            state.Career.DominantEraName = data.DominantEraName ?? "Unknown";
            state.Career.LifetimeDominant = (ReputationCategory)data.LifetimeDominant;
            state.Career.RecentDominant = (ReputationCategory)data.RecentDominant;
        }

        private static int[] PackStats(LifetimeStatistics stats)
        {
            return new[]
            {
                stats.PersonalKills, stats.PersonalHits, stats.CommandVictories, stats.CommandDefeats,
                stats.MajorVictories, stats.MajorDefeats, stats.OutnumberedVictories, stats.TrivialVictories,
                stats.NoblesReleased, stats.NoblesExecuted, stats.VillagesRaided, stats.TownsCaptured,
                stats.CastlesCaptured, stats.SettlementsLost, stats.SettlementsDefended, stats.TournamentsWon,
                stats.TournamentsLost, stats.GoldPeak, stats.CurrentGold, stats.TradeProfit,
                stats.FactionJoins, stats.FactionDefections, stats.TimesCaptured, stats.HideoutsCleared,
                stats.CaravansOwnedPeak, stats.SettlementsOwnedPeak, stats.DaysAsRuler, stats.BanditFights,
                stats.NobleArmyFights, stats.ConsecutiveMajorDefeats, stats.ConsecutiveVictories,
                stats.DaysSinceLastBattle, stats.DaysSinceLastTrade
            };
        }

        private static void UnpackStats(LifetimeStatistics stats, int[] values)
        {
            if (values == null || values.Length < 33)
            {
                return;
            }

            stats.PersonalKills = values[0];
            stats.PersonalHits = values[1];
            stats.CommandVictories = values[2];
            stats.CommandDefeats = values[3];
            stats.MajorVictories = values[4];
            stats.MajorDefeats = values[5];
            stats.OutnumberedVictories = values[6];
            stats.TrivialVictories = values[7];
            stats.NoblesReleased = values[8];
            stats.NoblesExecuted = values[9];
            stats.VillagesRaided = values[10];
            stats.TownsCaptured = values[11];
            stats.CastlesCaptured = values[12];
            stats.SettlementsLost = values[13];
            stats.SettlementsDefended = values[14];
            stats.TournamentsWon = values[15];
            stats.TournamentsLost = values[16];
            stats.GoldPeak = values[17];
            stats.CurrentGold = values[18];
            stats.TradeProfit = values[19];
            stats.FactionJoins = values[20];
            stats.FactionDefections = values[21];
            stats.TimesCaptured = values[22];
            stats.HideoutsCleared = values[23];
            stats.CaravansOwnedPeak = values[24];
            stats.SettlementsOwnedPeak = values[25];
            stats.DaysAsRuler = values[26];
            stats.BanditFights = values[27];
            stats.NobleArmyFights = values[28];
            stats.ConsecutiveMajorDefeats = values[29];
            stats.ConsecutiveVictories = values[30];
            stats.DaysSinceLastBattle = values[31];
            stats.DaysSinceLastTrade = values[32];
        }

        private static void PackEvents(ReputationState state, ReputationSaveData data)
        {
            int count = state.RecentEvents.Count;
            data.EventKinds = new int[count];
            data.EventDays = new double[count];
            data.EventCategories = new int[count];
            data.EventAmounts = new float[count];
            data.EventDifficulties = new float[count];
            data.EventImportances = new float[count];
            data.EventEnemyStrengths = new float[count];
            data.EventInvolvements = new float[count];
            data.EventFlags = new int[count];
            data.EventRegions = new string[count];
            data.EventSummaries = new string[count];
            for (int i = 0; i < count; i++)
            {
                ReputationEvent item = state.RecentEvents[i];
                data.EventKinds[i] = (int)item.Kind;
                data.EventDays[i] = item.Day;
                data.EventCategories[i] = (int)item.PrimaryCategory;
                data.EventAmounts[i] = item.Amount;
                data.EventDifficulties[i] = item.Difficulty;
                data.EventImportances[i] = item.Importance;
                data.EventEnemyStrengths[i] = item.EnemyStrength;
                data.EventInvolvements[i] = item.PlayerInvolvement;
                data.EventFlags[i] = (item.IsLowRisk ? 1 : 0) | (item.IsLegendary ? 2 : 0);
                data.EventRegions[i] = item.RegionId ?? string.Empty;
                data.EventSummaries[i] = item.Summary ?? string.Empty;
            }
        }

        private static void UnpackEvents(ReputationState state, ReputationSaveData data)
        {
            state.RecentEvents.Clear();
            int count = data.EventKinds == null ? 0 : data.EventKinds.Length;
            for (int i = 0; i < count; i++)
            {
                int flags = ValueAt(data.EventFlags, i);
                state.RecentEvents.Add(new ReputationEvent
                {
                    Kind = (ReputationEventKind)ValueAt(data.EventKinds, i),
                    Day = ValueAt(data.EventDays, i),
                    PrimaryCategory = (ReputationCategory)ValueAt(data.EventCategories, i),
                    Amount = ValueAt(data.EventAmounts, i),
                    Difficulty = ValueAt(data.EventDifficulties, i),
                    Importance = ValueAt(data.EventImportances, i),
                    EnemyStrength = ValueAt(data.EventEnemyStrengths, i),
                    PlayerInvolvement = ValueAt(data.EventInvolvements, i),
                    IsLowRisk = (flags & 1) != 0,
                    IsLegendary = (flags & 2) != 0,
                    RegionId = ValueAt(data.EventRegions, i),
                    Summary = ValueAt(data.EventSummaries, i)
                });
            }
        }

        private static void PackLegacy(ReputationState state, ReputationSaveData data)
        {
            int count = state.LegacyEvents.Count;
            data.LegacyIds = new string[count];
            data.LegacySummaries = new string[count];
            data.LegacyDays = new double[count];
            data.LegacyCategories = new int[count];
            data.LegacyWeights = new float[count];
            for (int i = 0; i < count; i++)
            {
                LegacyEvent item = state.LegacyEvents[i];
                data.LegacyIds[i] = item.Id ?? string.Empty;
                data.LegacySummaries[i] = item.Summary ?? string.Empty;
                data.LegacyDays[i] = item.Day;
                data.LegacyCategories[i] = (int)item.Category;
                data.LegacyWeights[i] = item.Weight;
            }
        }

        private static void UnpackLegacy(ReputationState state, ReputationSaveData data)
        {
            state.LegacyEvents.Clear();
            int count = data.LegacyIds == null ? 0 : data.LegacyIds.Length;
            for (int i = 0; i < count; i++)
            {
                state.LegacyEvents.Add(new LegacyEvent
                {
                    Id = ValueAt(data.LegacyIds, i),
                    Summary = ValueAt(data.LegacySummaries, i),
                    Day = ValueAt(data.LegacyDays, i),
                    Category = (ReputationCategory)ValueAt(data.LegacyCategories, i),
                    Weight = ValueAt(data.LegacyWeights, i)
                });
            }
        }

        private static void PackHistory(ReputationState state, ReputationSaveData data)
        {
            int count = state.NicknameHistory.Count;
            data.HistoryIds = new string[count];
            data.HistoryAcquired = new double[count];
            data.HistoryLost = new double[count];
            data.HistoryScores = new float[count];
            for (int i = 0; i < count; i++)
            {
                NicknameHistoryEntry item = state.NicknameHistory[i];
                data.HistoryIds[i] = item.NicknameId ?? string.Empty;
                data.HistoryAcquired[i] = item.AcquiredDay;
                data.HistoryLost[i] = item.LostDay;
                data.HistoryScores[i] = item.ClaimScore;
            }
        }

        private static void UnpackHistory(ReputationState state, ReputationSaveData data)
        {
            state.NicknameHistory.Clear();
            int count = data.HistoryIds == null ? 0 : data.HistoryIds.Length;
            for (int i = 0; i < count; i++)
            {
                state.NicknameHistory.Add(new NicknameHistoryEntry
                {
                    NicknameId = ValueAt(data.HistoryIds, i),
                    AcquiredDay = ValueAt(data.HistoryAcquired, i),
                    LostDay = ValueAt(data.HistoryLost, i),
                    ClaimScore = ValueAt(data.HistoryScores, i)
                });
            }
        }

        private static void PackEras(ReputationState state, ReputationSaveData data)
        {
            int count = state.Career.Eras.Count;
            data.EraNames = new string[count];
            data.EraStarts = new double[count];
            data.EraEnds = new double[count];
            data.EraCategories = new int[count];
            for (int i = 0; i < count; i++)
            {
                CareerEra era = state.Career.Eras[i];
                data.EraNames[i] = era.Name ?? string.Empty;
                data.EraStarts[i] = era.StartDay;
                data.EraEnds[i] = era.EndDay;
                data.EraCategories[i] = (int)era.DominantCategory;
            }
        }

        private static void UnpackEras(ReputationState state, ReputationSaveData data)
        {
            state.Career.Eras.Clear();
            int count = data.EraNames == null ? 0 : data.EraNames.Length;
            for (int i = 0; i < count; i++)
            {
                state.Career.Eras.Add(new CareerEra
                {
                    Name = ValueAt(data.EraNames, i),
                    StartDay = ValueAt(data.EraStarts, i),
                    EndDay = ValueAt(data.EraEnds, i),
                    DominantCategory = (ReputationCategory)ValueAt(data.EraCategories, i)
                });
            }
        }

        private static void PackAwareness(ReputationState state, ReputationSaveData data)
        {
            int count = state.Awareness.Count;
            data.AwareHeroIds = new string[count];
            data.MentionHeroIds = new string[count];
            data.MentionDays = new double[count];
            for (int i = 0; i < count; i++)
            {
                AwarenessRecord item = state.Awareness[i];
                data.AwareHeroIds[i] = item.KnowsNickname ? (item.HeroId ?? string.Empty) : string.Empty;
                data.MentionHeroIds[i] = item.HeroId ?? string.Empty;
                data.MentionDays[i] = item.LastMentionDay;
            }
        }

        private static void UnpackAwareness(ReputationState state, ReputationSaveData data)
        {
            state.Awareness.Clear();
            int count = data.MentionHeroIds == null ? 0 : data.MentionHeroIds.Length;
            for (int i = 0; i < count; i++)
            {
                string id = ValueAt(data.MentionHeroIds, i);
                bool knows = data.AwareHeroIds != null && i < data.AwareHeroIds.Length && data.AwareHeroIds[i] == id && id.Length > 0;
                state.Awareness.Add(new AwarenessRecord
                {
                    HeroId = id,
                    KnowsNickname = knows,
                    LastMentionDay = ValueAt(data.MentionDays, i)
                });
            }
        }

        private static float ValueAt(float[] values, int index)
        {
            return values != null && index < values.Length ? values[index] : 0f;
        }

        private static double ValueAt(double[] values, int index)
        {
            return values != null && index < values.Length ? values[index] : 0d;
        }

        private static int ValueAt(int[] values, int index)
        {
            return values != null && index < values.Length ? values[index] : 0;
        }

        private static string ValueAt(string[] values, int index)
        {
            if (values == null || index >= values.Length || values[index] == null)
            {
                return string.Empty;
            }

            return values[index];
        }
    }
}
