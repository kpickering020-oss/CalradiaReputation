using System;

namespace CalradiaReputation
{
    public sealed class ReputationScoringEngine
    {
        public ReputationEvent ApplyBattleVictory(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            ClassifyBattle(context, out bool trivial, out bool major, out bool outnumbered);
            ReputationEventKind kind = trivial
                ? ReputationEventKind.TrivialVictory
                : (outnumbered ? ReputationEventKind.OutnumberedVictory : (major ? ReputationEventKind.MajorBattleVictory : ReputationEventKind.BattleVictory));
            if (IsDuplicate(state, kind, context))
            {
                return LastEvent(state);
            }


            float difficulty = ComputeDifficulty(context);
            float importance = ComputeBattleImportance(context, major, trivial);
            float farming = FarmingMultiplier(state, trivial ? ReputationEventKind.TrivialVictory : ReputationEventKind.BattleVictory, trivial);
            float commandShare = CommandShare(context);
            float warriorShare = WarriorShare(context);

            float generalship = ReputationBalance.BaseVictory * difficulty * importance * commandShare * farming;
            float warrior = ReputationBalance.BaseVictory * difficulty * warriorShare * farming * 0.55f;
            if (major)
            {
                generalship += ReputationBalance.BaseMajorBonus * difficulty * commandShare;
            }

            if (outnumbered)
            {
                generalship *= 1.25f;
            }

            UpdateVictoryStats(state.Stats, trivial, major, outnumbered, context);
            bool legendary = !trivial && context.EnemyIsNobleArmy && context.IsHeavilyOutnumbered();
            Apply(state, context, kind, ReputationCategory.Generalship, generalship, difficulty, importance, trivial, legendary);
            ApplyDelta(state, ReputationCategory.Warrior, warrior, context.Day);

            if (context.EnemyIsBandit)
            {
                ApplyDelta(state, ReputationCategory.Outlaw, -0.4f * farming, context.Day);
            }

            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyBattleDefeat(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            ClassifyBattle(context, out bool trivial, out bool major, out _);

            float difficulty = ComputeDifficulty(context);
            float importance = ComputeBattleImportance(context, major, trivial);
            float loss = ReputationBalance.BaseDefeat * Math.Max(0.45f, importance) * CommandShare(context);
            if (major)
            {
                loss += ReputationBalance.BaseMajorBonus * 0.85f;
            }

            if (trivial)
            {
                loss *= 0.4f;
            }

            LifetimeStatistics stats = state.Stats;
            stats.CommandDefeats++;
            stats.ConsecutiveVictories = 0;
            stats.DaysSinceLastBattle = 0;
            if (major)
            {
                stats.MajorDefeats++;
                stats.ConsecutiveMajorDefeats++;
            }

            ReputationEventKind kind = major ? ReputationEventKind.MajorBattleDefeat : ReputationEventKind.BattleDefeat;
            Apply(state, context, kind, ReputationCategory.Generalship, -loss, difficulty, importance, trivial, false);
            if (context.PlayerInvolvement >= 0.5f)
            {
                ApplyDelta(state, ReputationCategory.Warrior, -loss * 0.25f, context.Day);
            }

            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyPersonalCombat(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            float difficulty = ComputeDifficulty(context);
            float farming = FarmingMultiplier(state, ReputationEventKind.PersonalCombatHit, context.IsTrivialEnemy());
            float amount = ReputationBalance.BasePersonalHit * Math.Max(0.35f, difficulty) * Clamp01(context.PlayerInvolvement) * farming * 2.2f;

            state.Stats.PersonalHits++;
            if (context.PlayerInvolvement >= 0.75f)
            {
                state.Stats.PersonalKills++;
            }

            Apply(state, context, ReputationEventKind.PersonalCombatHit, ReputationCategory.Warrior, amount, difficulty, 0.4f, context.IsTrivialEnemy(), difficulty >= 1.6f);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplySettlementCaptured(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            float importance = context.IsTown ? 1.55f : (context.IsCastle ? 1.25f : 0.7f);
            float amount = ReputationBalance.BaseSettlementCapture * importance * ComputeDifficulty(context);
            if (context.IsTown)
            {
                state.Stats.TownsCaptured++;
            }
            else if (context.IsCastle)
            {
                state.Stats.CastlesCaptured++;
            }

            int owned = state.Stats.TownsCaptured + state.Stats.CastlesCaptured;
            if (owned > state.Stats.SettlementsOwnedPeak)
            {
                state.Stats.SettlementsOwnedPeak = owned;
            }

            Apply(state, context, ReputationEventKind.SettlementCaptured, ReputationCategory.Conquest, amount, ComputeDifficulty(context), importance, false, context.IsTown);
            ApplyDelta(state, ReputationCategory.Rulership, amount * 0.35f, context.Day);
            ApplyDelta(state, ReputationCategory.Generalship, amount * 0.2f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplySettlementLost(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.SettlementsLost++;
            float amount = ReputationBalance.BaseSettlementLoss * (context.IsTown ? 1.4f : 1f);
            Apply(state, context, ReputationEventKind.SettlementLost, ReputationCategory.Conquest, -amount, 1f, 1.2f, false, false);
            ApplyDelta(state, ReputationCategory.Rulership, -amount * 0.4f, context.Day);
            ApplyDelta(state, ReputationCategory.Defender, -amount * 0.25f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplySettlementDefended(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.SettlementsDefended++;
            float amount = ReputationBalance.BaseSettlementDefense * ComputeDifficulty(context);
            Apply(state, context, ReputationEventKind.SettlementDefended, ReputationCategory.Defender, amount, ComputeDifficulty(context), 1.1f, false, context.IsHeavilyOutnumbered());
            ApplyDelta(state, ReputationCategory.Generalship, amount * 0.25f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyNobleReleased(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.NoblesReleased++;
            float amount = ReputationBalance.BaseMercy * FarmingMultiplier(state, ReputationEventKind.NobleReleased, true);
            Apply(state, context, ReputationEventKind.NobleReleased, ReputationCategory.Mercy, amount, 0.8f, 1f, true, false);
            ApplyDelta(state, ReputationCategory.Cruelty, -amount * ReputationBalance.OppositeBleed, context.Day);
            ApplyDelta(state, ReputationCategory.Charisma, amount * 0.3f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyNobleExecuted(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.NoblesExecuted++;
            float amount = ReputationBalance.BaseCruelty * FarmingMultiplier(state, ReputationEventKind.NobleExecuted, true);
            Apply(state, context, ReputationEventKind.NobleExecuted, ReputationCategory.Cruelty, amount, 0.9f, 1.1f, true, false);
            ApplyDelta(state, ReputationCategory.Mercy, -amount * ReputationBalance.OppositeBleed, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyVillageRaided(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.VillagesRaided++;
            float farming = FarmingMultiplier(state, ReputationEventKind.VillageRaided, true);
            float amount = ReputationBalance.BaseRaid * farming;
            Apply(state, context, ReputationEventKind.VillageRaided, ReputationCategory.Raider, amount, 0.7f, 0.85f, true, false);
            ApplyDelta(state, ReputationCategory.Cruelty, amount * 0.45f, context.Day);
            ApplyDelta(state, ReputationCategory.Outlaw, amount * 0.35f, context.Day);
            ApplyDelta(state, ReputationCategory.Mercy, -amount * 0.2f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyTradeProfit(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            int profit = Math.Max(0, context.Profit);
            state.Stats.TradeProfit += profit;
            state.Stats.DaysSinceLastTrade = 0;
            float farming = FarmingMultiplier(state, ReputationEventKind.TradeProfit, profit < 2000);
            float amount = ReputationBalance.BaseTrade * ProfitScale(profit) * farming;
            Apply(state, context, ReputationEventKind.TradeProfit, ReputationCategory.Trade, amount, 0.5f, ProfitScale(profit), profit < 2000, profit >= 15000);
            ApplyDelta(state, ReputationCategory.Wealth, amount * 0.55f, context.Day);
            ApplyDelta(state, ReputationCategory.Charisma, amount * 0.15f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyWealthSnapshot(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            int gold = Math.Max(0, context.Gold);
            state.Stats.CurrentGold = gold;
            if (gold > state.Stats.GoldPeak)
            {
                state.Stats.GoldPeak = gold;
            }

            float amount = WealthScale(gold) - state.Score(ReputationCategory.Wealth).Current * 0.15f;
            if (amount < 0.25f && gold < state.Stats.GoldPeak)
            {
                amount = 0f;
            }

            Apply(state, context, ReputationEventKind.WealthSnapshot, ReputationCategory.Wealth, amount, 0.4f, WealthScale(gold) / 12f, false, gold >= 200000);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyTournamentResult(ReputationState state, EncounterContext context, bool won)
        {
            EnsureDay(state, context.Day);
            if (won)
            {
                state.Stats.TournamentsWon++;
            }
            else
            {
                state.Stats.TournamentsLost++;
            }

            float amount = (won ? ReputationBalance.BaseTournament : -ReputationBalance.BaseTournament * 0.45f)
                * FarmingMultiplier(state, won ? ReputationEventKind.TournamentWin : ReputationEventKind.TournamentLoss, true);
            ReputationEventKind kind = won ? ReputationEventKind.TournamentWin : ReputationEventKind.TournamentLoss;
            Apply(state, context, kind, ReputationCategory.Tournament, amount, 0.75f, won ? 1f : 0.5f, true, false);
            if (won)
            {
                ApplyDelta(state, ReputationCategory.Warrior, amount * 0.35f, context.Day);
                ApplyDelta(state, ReputationCategory.Charisma, amount * 0.2f, context.Day);
            }

            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyFactionJoined(ReputationState state, EncounterContext context, bool defected)
        {
            EnsureDay(state, context.Day);
            if (defected)
            {
                state.Stats.FactionDefections++;
                Apply(state, context, ReputationEventKind.FactionDefected, ReputationCategory.Treachery, ReputationBalance.BaseTreachery, 1f, 1.2f, false, false);
                ApplyDelta(state, ReputationCategory.Loyalty, -ReputationBalance.BaseTreachery * ReputationBalance.OppositeBleed, context.Day);
            }
            else
            {
                state.Stats.FactionJoins++;
                Apply(state, context, ReputationEventKind.FactionJoined, ReputationCategory.Loyalty, ReputationBalance.BaseLoyalty, 0.8f, 1f, false, false);
            }

            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyPlayerCaptured(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.TimesCaptured++;
            state.Stats.ConsecutiveVictories = 0;
            Apply(state, context, ReputationEventKind.PlayerCaptured, ReputationCategory.Warrior, -ReputationBalance.BaseCaptureShame, 1f, 1f, false, false);
            ApplyDelta(state, ReputationCategory.Generalship, -ReputationBalance.BaseCaptureShame * 0.35f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyHideoutCleared(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            state.Stats.HideoutsCleared++;
            float amount = ReputationBalance.BaseHideout * ComputeDifficulty(context) * FarmingMultiplier(state, ReputationEventKind.HideoutCleared, true);
            Apply(state, context, ReputationEventKind.HideoutCleared, ReputationCategory.Warrior, amount, ComputeDifficulty(context), 0.7f, true, false);
            ApplyDelta(state, ReputationCategory.Outlaw, -amount * 0.2f, context.Day);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public ReputationEvent ApplyBecameRuler(ReputationState state, EncounterContext context)
        {
            EnsureDay(state, context.Day);
            Apply(state, context, ReputationEventKind.BecameRuler, ReputationCategory.Rulership, ReputationBalance.BaseRulership * 1.6f, 1f, 1.6f, false, true);
            RefreshCareer(state);
            return LastEvent(state);
        }

        public void DailyTick(ReputationState state, double day)
        {
            if (day < state.CurrentDay)
            {
                return;
            }

            double previous = state.CurrentDay;
            int steps = Math.Max(1, (int)Math.Round(day - previous));
            if (day == previous)
            {
                steps = 1;
            }

            for (int i = 0; i < steps; i++)
            {
                double tickDay = previous + i + (day == previous ? 0 : 1);
                state.CurrentDay = tickDay;
                state.Stats.DaysSinceLastBattle++;
                state.Stats.DaysSinceLastTrade++;
                if (state.Stats.SettlementsOwnedPeak > 0)
                {
                    state.Stats.DaysAsRuler++;
                }

                for (int c = 0; c < state.Categories.Length; c++)
                {
                    state.Categories[c].DailyTick(tickDay);
                }
            }

            TrimOldEvents(state, day);
            RefreshCareer(state);
        }

        public float FarmingMultiplier(ReputationState state, ReputationEventKind kind, bool lowRisk)
        {
            if (!lowRisk)
            {
                return 1f;
            }

            int repeats = CountRecent(state, kind, ReputationBalance.RecentWindowDays);
            float multiplier = 1f / (1f + (repeats * ReputationBalance.FarmingPenaltyPerRepeat));
            return multiplier < ReputationBalance.FarmingFloor ? ReputationBalance.FarmingFloor : multiplier;
        }

        public float ComputeDifficulty(EncounterContext context)
        {
            float ratio = context.StrengthRatio();
            float difficulty = ratio / 0.85f;
            if (context.IsTrivialEnemy())
            {
                difficulty *= 0.28f;
            }

            if (context.IsOutnumbered())
            {
                difficulty *= 1.2f;
            }

            if (context.IsHeavilyOutnumbered())
            {
                difficulty *= 1.25f;
            }

            return Clamp(difficulty, ReputationBalance.MinDifficulty, ReputationBalance.MaxDifficulty);
        }

        private void Apply(
            ReputationState state,
            EncounterContext context,
            ReputationEventKind kind,
            ReputationCategory category,
            float amount,
            float difficulty,
            float importance,
            bool lowRisk,
            bool legendary)
        {
            if (IsDuplicate(state, kind, context))
            {
                return;
            }

            ApplyDelta(state, category, amount, context.Day);

            ReputationEvent recorded = new ReputationEvent
            {
                Kind = kind,
                Day = context.Day,
                PrimaryCategory = category,
                Amount = amount,
                Difficulty = difficulty,
                Importance = Clamp(importance, ReputationBalance.MinImportance, ReputationBalance.MaxImportance),
                EnemyStrength = context.EnemyStrength,
                PlayerInvolvement = context.PlayerInvolvement,
                IsLowRisk = lowRisk,
                IsLegendary = legendary,
                RegionId = context.RegionId ?? string.Empty,
                Summary = string.IsNullOrEmpty(context.Summary) ? kind.ToString() : context.Summary
            };

            state.RecentEvents.Add(recorded);
            while (state.RecentEvents.Count > ReputationBalance.MaxRecentEvents)
            {
                state.RecentEvents.RemoveAt(0);
            }

            if (legendary)
            {
                state.AddLegacy(
                    kind + ":" + recorded.Day.ToString("0") + ":" + recorded.RegionId,
                    recorded.Summary,
                    category,
                    Math.Abs(amount),
                    context.Day);
            }
        }

        private static void ApplyDelta(ReputationState state, ReputationCategory category, float amount, double day)
        {
            if (Math.Abs(amount) < 0.0001f)
            {
                return;
            }

            state.Score(category).Add(amount, day);
        }

        private static void ClassifyBattle(EncounterContext context, out bool trivial, out bool major, out bool outnumbered)
        {
            trivial = context.IsTrivialEnemy() && !context.IsOutnumbered();
            outnumbered = context.IsOutnumbered() && !trivial;
            major = !trivial && (context.EnemyIsNobleArmy || context.EnemyStrength >= ReputationBalance.NobleArmyStrength || outnumbered);
        }

        private static void UpdateVictoryStats(LifetimeStatistics stats, bool trivial, bool major, bool outnumbered, EncounterContext context)
        {
            stats.CommandVictories++;
            stats.ConsecutiveVictories++;
            stats.ConsecutiveMajorDefeats = 0;
            stats.DaysSinceLastBattle = 0;
            if (trivial)
            {
                stats.TrivialVictories++;
                stats.BanditFights++;
            }

            if (major)
            {
                stats.MajorVictories++;
            }

            if (outnumbered)
            {
                stats.OutnumberedVictories++;
            }

            if (context.EnemyIsNobleArmy)
            {
                stats.NobleArmyFights++;
            }
        }

        private static float ComputeBattleImportance(EncounterContext context, bool major, bool trivial)
        {
            float importance = 0.55f;
            if (trivial)
            {
                importance = 0.22f;
            }
            else if (major)
            {
                importance = 1.15f;
            }

            if (context.EnemyIsNobleArmy)
            {
                importance += 0.35f;
            }

            if (context.EnemyIsBandit)
            {
                importance -= 0.18f;
            }

            return Clamp(importance, ReputationBalance.MinImportance, ReputationBalance.MaxImportance);
        }

        private static float CommandShare(EncounterContext context)
        {
            if (!context.PlayerCommanded)
            {
                return 0.15f;
            }

            float share = ReputationBalance.CommandInvolvementFloor + ((1f - Clamp01(context.PlayerInvolvement)) * 0.72f);
            return Clamp(share, ReputationBalance.CommandInvolvementFloor, 1f);
        }

        private static float WarriorShare(EncounterContext context)
        {
            return Math.Max(ReputationBalance.WarriorInvolvementFloor, Clamp01(context.PlayerInvolvement));
        }

        private static float ProfitScale(int profit)
        {
            return Clamp((float)Math.Log10(profit + 10) / 2.2f, 0.2f, 1.6f);
        }

        private static float WealthScale(int gold)
        {
            return Clamp((float)Math.Log10(gold + 10) * 6.5f - 8f, 0f, 28f);
        }

        private static int CountRecent(ReputationState state, ReputationEventKind kind, int windowDays)
        {
            int count = 0;
            double minDay = state.CurrentDay - windowDays;
            for (int i = 0; i < state.RecentEvents.Count; i++)
            {
                ReputationEvent item = state.RecentEvents[i];
                if (item.Kind == kind && item.Day >= minDay)
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsDuplicate(ReputationState state, ReputationEventKind kind, EncounterContext context)
        {
            for (int i = state.RecentEvents.Count - 1; i >= 0; i--)
            {
                ReputationEvent item = state.RecentEvents[i];
                if (item.Day < context.Day)
                {
                    return false;
                }

                string summary = string.IsNullOrEmpty(context.Summary) ? kind.ToString() : context.Summary;
                if (item.Kind == kind && item.Day == context.Day && item.RegionId == (context.RegionId ?? string.Empty) && item.Summary == summary)
                {
                    return true;
                }
            }

            return false;
        }

        private static void TrimOldEvents(ReputationState state, double day)
        {
            double minDay = day - ReputationBalance.RecentWindowDays;
            state.RecentEvents.RemoveAll(item => !item.IsLegendary && item.Day < minDay);
            while (state.RecentEvents.Count > ReputationBalance.MaxRecentEvents)
            {
                int drop = state.RecentEvents.FindIndex(item => !item.IsLegendary);
                if (drop < 0)
                {
                    break;
                }

                state.RecentEvents.RemoveAt(drop);
            }
        }

        private static void RefreshCareer(ReputationState state)
        {
            state.Career.LifetimeDominant = state.DominantCategory(false);
            state.Career.RecentDominant = state.DominantCategory(true);
        }

        private static void EnsureDay(ReputationState state, double day)
        {
            if (day > state.CurrentDay)
            {
                state.CurrentDay = day;
            }
        }

        private static ReputationEvent LastEvent(ReputationState state)
        {
            return state.RecentEvents.Count == 0 ? new ReputationEvent() : state.RecentEvents[state.RecentEvents.Count - 1];
        }

        private static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
