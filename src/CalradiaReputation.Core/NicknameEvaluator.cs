using System;

namespace CalradiaReputation
{
    public sealed class NicknameEvaluator
    {
        private readonly NicknameRegistry _registry;

        public NicknameEvaluator(NicknameRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public NicknameClaim Score(NicknameDefinition definition, ReputationState state)
        {
            bool eligible = IsEligible(definition, state);
            NicknameRequirements req = definition.Requirements;
            CategoryScore focus = state.Score(req.FocusCategory);

            float reputationStrength = Average(
                focus.Current,
                state.Score(state.DominantCategory(false)).Current);
            float deedStrength = ComputeDeedStrength(definition, state);
            float historical = Average(focus.Peak, MatchingLegacyWeight(definition, state));
            float recent = focus.Recent;
            float bonuses = 0f;

            if (IsEvolutionFromCurrent(definition, state))
            {
                bonuses += 8f;
            }

            if (state.HeldNickname(definition.Id))
            {
                bonuses += 3f;
            }

            if (definition.Rarity == NicknameRarity.Legendary && state.Age >= 45f)
            {
                bonuses += 4f;
            }

            return new NicknameClaim(definition, reputationStrength, deedStrength, historical, recent, bonuses, eligible);
        }

        public NicknameDefinition? Evaluate(ReputationState state)
        {
            NicknameClaim? bestEligible = null;
            NicknameDefinition[] all = _registry.All;
            for (int i = 0; i < all.Length; i++)
            {
                NicknameClaim claim = Score(all[i], state);
                if (!claim.Eligible)
                {
                    continue;
                }

                if (bestEligible == null
                    || claim.Score > bestEligible.Score
                    || (Math.Abs(claim.Score - bestEligible.Score) < 0.01f
                        && claim.Nickname.Priority > bestEligible.Nickname.Priority))
                {
                    bestEligible = claim;
                }
            }

            NicknameDefinition? current = _registry.Find(state.CurrentNicknameId);
            NicknameClaim? currentClaim = current == null ? null : Score(current, state);

            if (bestEligible == null)
            {
                state.PendingChallengerId = string.Empty;
                state.ChallengerStreakDays = 0;
                return current;
            }

            if (current == null)
            {
                return ConfirmChallenger(state, bestEligible);
            }

            if (bestEligible.Nickname.Id == current.Id)
            {
                state.PendingChallengerId = string.Empty;
                state.ChallengerStreakDays = 0;
                return current;
            }

            float margin = ReputationBalance.ReplacementMargin + current.ReplacementResistance();
            float heldScore = 0f;
            if (currentClaim != null)
            {
                heldScore = currentClaim.Eligible ? currentClaim.Score : currentClaim.Score * 0.2f;
            }

            if (currentClaim != null && !currentClaim.Eligible)
            {
                margin *= 0.4f;
            }

            if (bestEligible.Score < heldScore + margin)
            {
                state.PendingChallengerId = string.Empty;
                state.ChallengerStreakDays = 0;
                return current;
            }

            return ConfirmChallenger(state, bestEligible);
        }

        public void ApplyEvaluation(ReputationState state, double day)
        {
            NicknameDefinition? next = Evaluate(state);
            if (next == null || next.Id == state.CurrentNicknameId)
            {
                return;
            }

            ReplaceNickname(state, next, day);
        }

        public static void ReplaceNickname(ReputationState state, NicknameDefinition next, double day)
        {
            if (!string.IsNullOrEmpty(state.CurrentNicknameId))
            {
                state.NicknameHistory.Add(new NicknameHistoryEntry
                {
                    NicknameId = state.CurrentNicknameId,
                    AcquiredDay = state.NicknameEstablishedDay,
                    LostDay = day,
                    ClaimScore = 0f
                });

                if (state.NicknameHistory.Count > ReputationBalance.MaxNicknameHistory)
                {
                    state.NicknameHistory.RemoveAt(0);
                }

                state.PreviousNicknameId = state.CurrentNicknameId;
            }

            state.CurrentNicknameId = next.Id;
            state.NicknameEstablishedDay = day;
            state.PendingChallengerId = string.Empty;
            state.ChallengerStreakDays = 0;
            state.Career.RecordEra(next.Id, day, next.Requirements.FocusCategory);
        }

        private NicknameDefinition? ConfirmChallenger(ReputationState state, NicknameClaim challenger)
        {
            if (state.PendingChallengerId == challenger.Nickname.Id)
            {
                state.ChallengerStreakDays++;
            }
            else
            {
                state.PendingChallengerId = challenger.Nickname.Id;
                state.ChallengerStreakDays = 1;
            }

            int required = ReputationBalance.SustainDaysFor(challenger.Nickname.Rarity);
            if (state.ChallengerStreakDays >= required)
            {
                return challenger.Nickname;
            }

            return _registry.Find(state.CurrentNicknameId);
        }

        public bool IsEligible(NicknameDefinition definition, ReputationState state)
        {
            NicknameRequirements req = definition.Requirements;
            CategoryScore focus = state.Score(req.FocusCategory);

            if (focus.Current + 0.001f < req.MinFocusScore)
            {
                return false;
            }

            if (req.MinPeakFocus > 0f && focus.Peak + 0.001f < req.MinPeakFocus)
            {
                return false;
            }

            if (req.MinRecentFocus > 0f && focus.Recent + 0.001f < req.MinRecentFocus)
            {
                return false;
            }

            if (req.MustOutrankCategory.HasValue)
            {
                if (focus.Current <= state.Score(req.MustOutrankCategory.Value).Current + 0.5f)
                {
                    return false;
                }
            }

            if (state.Age + 0.001f < req.MinAge)
            {
                return false;
            }

            if (req.MaxAge > 0f && state.Age > req.MaxAge)
            {
                return false;
            }

            LifetimeStatistics stats = state.Stats;
            if (stats.CommandVictories < req.MinCommandVictories) return false;
            if (stats.MajorVictories < req.MinMajorVictories) return false;
            if (stats.MajorDefeats < req.MinMajorDefeats) return false;
            if (stats.OutnumberedVictories < req.MinOutnumberedVictories) return false;
            if (stats.MajorDefeats > req.MaxMajorDefeats) return false;
            if (stats.PersonalHits < req.MinPersonalHits) return false;
            if (stats.TrivialVictories < req.MinTrivialVictories) return false;
            if (stats.NoblesReleased < req.MinNoblesReleased) return false;
            if (stats.NoblesExecuted < req.MinNoblesExecuted) return false;
            if (stats.VillagesRaided < req.MinVillagesRaided) return false;
            if (stats.TownsCaptured < req.MinTownsCaptured) return false;
            if (stats.TournamentsWon < req.MinTournamentsWon) return false;
            if (stats.CurrentGold < req.MinGold && stats.GoldPeak < req.MinGold) return false;
            if (stats.TradeProfit < req.MinTradeProfit) return false;
            if (stats.TimesCaptured < req.MinTimesCaptured) return false;
            if (stats.ConsecutiveMajorDefeats < req.MinConsecutiveMajorDefeats) return false;
            if (stats.SettlementsLost < req.MinSettlementsLost) return false;
            if (stats.SettlementsOwnedPeak < req.MinSettlementsOwnedPeak) return false;
            if (stats.DaysSinceLastBattle < req.MinDaysSinceLastBattle) return false;

            if (!TrivialShareAllowed(req, stats))
            {
                return false;
            }

            if (!ContainsAll(req.RequiredLegacyIds, id => state.HasLegacy(id)))
            {
                return false;
            }

            if (req.RequiredPreviousIds.Length > 0 && !ContainsAny(req.RequiredPreviousIds, state.HeldNickname))
            {
                return false;
            }

            if (req.EvolvesFromIds.Length > 0 && !ContainsAny(req.EvolvesFromIds, state.HeldNickname))
            {
                return false;
            }

            return true;
        }

        private static bool TrivialShareAllowed(NicknameRequirements req, LifetimeStatistics stats)
        {
            int totalFights = stats.CommandVictories + stats.CommandDefeats;
            if (totalFights <= 0)
            {
                return req.MaxTrivialVictorySharePercent >= 100;
            }

            int share = (stats.TrivialVictories * 100) / totalFights;
            return share <= req.MaxTrivialVictorySharePercent;
        }

        private static float ComputeDeedStrength(NicknameDefinition definition, ReputationState state)
        {
            LifetimeStatistics stats = state.Stats;
            ReputationTag tags = definition.Tags;
            float value = 0f;
            int parts = 0;

            if ((tags & ReputationTag.Command) != 0 || (tags & ReputationTag.Military) != 0)
            {
                value += Scale(stats.MajorVictories, 8f) * 0.6f + Scale(stats.OutnumberedVictories, 5f) * 0.4f;
                parts++;
            }

            if ((tags & ReputationTag.Combat) != 0)
            {
                value += Scale(stats.PersonalHits, 40f);
                parts++;
            }

            if ((tags & ReputationTag.Fallen) != 0 || (tags & ReputationTag.Mocked) != 0)
            {
                value += Scale(stats.MajorDefeats + stats.ConsecutiveMajorDefeats, 6f);
                parts++;
            }

            if ((tags & ReputationTag.Wealthy) != 0 || (tags & ReputationTag.Merchant) != 0)
            {
                value += Scale(stats.GoldPeak / 10000f, 40f);
                parts++;
            }

            if ((tags & ReputationTag.Raider) != 0)
            {
                value += Scale(stats.VillagesRaided, 8f);
                parts++;
            }

            if ((tags & ReputationTag.Redeemed) != 0)
            {
                value += Scale(stats.ConsecutiveVictories + stats.MajorVictories, 8f);
                parts++;
            }

            if (parts == 0)
            {
                return Scale(stats.CommandVictories + stats.PersonalHits, 20f);
            }

            return value / parts;
        }

        private static float MatchingLegacyWeight(NicknameDefinition definition, ReputationState state)
        {
            float total = 0f;
            for (int i = 0; i < state.LegacyEvents.Count; i++)
            {
                LegacyEvent legacy = state.LegacyEvents[i];
                if (legacy.Category == definition.Requirements.FocusCategory)
                {
                    total += legacy.Weight;
                }
            }

            return Math.Min(ReputationBalance.ScoreCap, total);
        }

        private static bool IsEvolutionFromCurrent(NicknameDefinition definition, ReputationState state)
        {
            if (string.IsNullOrEmpty(state.CurrentNicknameId))
            {
                return false;
            }

            return ContainsAny(definition.Requirements.EvolvesFromIds, id => id == state.CurrentNicknameId)
                || ContainsAny(definition.Requirements.RequiredPreviousIds, id => id == state.CurrentNicknameId);
        }

        private static float Average(float a, float b)
        {
            return (a + b) * 0.5f;
        }

        private static float Scale(float value, float target)
        {
            if (target <= 0f)
            {
                return 0f;
            }

            float scaled = (value / target) * ReputationBalance.ScoreCap;
            return scaled > ReputationBalance.ScoreCap ? ReputationBalance.ScoreCap : Math.Max(0f, scaled);
        }

        private static bool ContainsAll(string[] required, Func<string, bool> has)
        {
            for (int i = 0; i < required.Length; i++)
            {
                if (!has(required[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsAny(string[] items, Func<string, bool> has)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (has(items[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
