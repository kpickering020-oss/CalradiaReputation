using Xunit;

namespace CalradiaReputation.Tests
{
    public sealed class ReputationScoringTests
    {
        private readonly ReputationScoringEngine _scoring = new ReputationScoringEngine();

        [Fact]
        public void LooterFarming_IsWorthLessThanOneOutnumberedNobleVictory()
        {
            ReputationState farmers = NewState();
            EncounterContext looters = Bandits(farmers.CurrentDay);
            for (int i = 0; i < 20; i++)
            {
                looters.Day = farmers.CurrentDay + i;
                looters.Summary = "looter-" + i;
                _scoring.ApplyBattleVictory(farmers, looters);
            }

            ReputationState general = NewState();
            EncounterContext noble = NobleOutnumbered(general.CurrentDay);
            _scoring.ApplyBattleVictory(general, noble);

            Assert.True(farmers.Stats.TrivialVictories == 20);
            Assert.Equal(1, general.Stats.OutnumberedVictories);
            Assert.True(
                general.Score(ReputationCategory.Generalship).Current
                > farmers.Score(ReputationCategory.Generalship).Current);
            Assert.True(farmers.Score(ReputationCategory.Generalship).Current < ReputationBalance.UncommonMinScore);
        }

        [Fact]
        public void GreatGeneral_CommandFocusOutpacesPersonalCombat()
        {
            ReputationState state = NewState();
            for (int i = 0; i < 8; i++)
            {
                EncounterContext battle = NobleOutnumbered(state.CurrentDay + i);
                battle.PlayerInvolvement = 0.08f;
                battle.Summary = "field-" + i;
                _scoring.ApplyBattleVictory(state, battle);
            }

            Assert.True(state.Score(ReputationCategory.Generalship).Current > state.Score(ReputationCategory.Warrior).Current + 10f);
            Assert.True(state.Stats.MajorVictories >= 8);
            Assert.Equal(0, state.Stats.TrivialVictories);
            Assert.Equal(ReputationCategory.Generalship, state.DominantCategory(false));
        }

        [Fact]
        public void PersonalFighter_GainsWarriorFromHitsMoreThanCommand()
        {
            ReputationState state = NewState();
            EncounterContext duel = new EncounterContext
            {
                Day = state.CurrentDay,
                PlayerStrength = 40,
                EnemyStrength = 55,
                PlayerInvolvement = 1f,
                PlayerCommanded = false,
                EnemyIsNobleArmy = false,
                Summary = "melee"
            };

            for (int i = 0; i < 12; i++)
            {
                duel.Day = state.CurrentDay + i;
                duel.Summary = "hit-" + i;
                _scoring.ApplyPersonalCombat(state, duel);
            }

            Assert.True(state.Score(ReputationCategory.Warrior).Current > state.Score(ReputationCategory.Generalship).Current);
            Assert.True(state.Stats.PersonalHits >= 12);
        }

        [Fact]
        public void MercyAndCruelty_StayIndependentWithOppositeBleed()
        {
            ReputationState state = NewState();
            _scoring.ApplyNobleReleased(state, Context(state, "release"));
            float mercy = state.Score(ReputationCategory.Mercy).Current;
            float crueltyAfterMercy = state.Score(ReputationCategory.Cruelty).Current;

            _scoring.ApplyNobleExecuted(state, Context(state, "execute"));

            Assert.True(mercy > 0f);
            Assert.True(crueltyAfterMercy < 0.01f);
            Assert.True(state.Score(ReputationCategory.Cruelty).Current > 0f);
            Assert.True(state.Score(ReputationCategory.Mercy).Current < mercy);
        }

        [Fact]
        public void DailyTick_DecaysRecentAndInactiveCurrentButKeepsPeak()
        {
            ReputationState state = NewState();
            state.SetScore(ReputationCategory.Warrior, 40f, 40f, 40f);
            state.Score(ReputationCategory.Warrior).LastActivityDay = 1;
            state.CurrentDay = 1;

            _scoring.DailyTick(state, 1 + ReputationBalance.InactivityDaysBeforeDecay + 40);

            CategoryScore warrior = state.Score(ReputationCategory.Warrior);
            Assert.Equal(40f, warrior.Peak);
            Assert.True(warrior.Recent < 12f);
            Assert.True(warrior.Current < 40f);
            Assert.True(warrior.Current >= 40f * ReputationBalance.PeakFloorRatio - 0.05f);
        }

        [Fact]
        public void LegendaryOutnumberedVictory_IsPreservedAsLegacy()
        {
            ReputationState state = NewState();
            EncounterContext battle = NobleOutnumbered(state.CurrentDay);
            battle.PlayerStrength = 40;
            battle.EnemyStrength = 220;
            battle.Summary = "stand at Charas";
            battle.RegionId = "charas";

            ReputationEvent recorded = _scoring.ApplyBattleVictory(state, battle);

            Assert.True(recorded.IsLegendary);
            Assert.NotEmpty(state.LegacyEvents);
            Assert.Contains(state.LegacyEvents, item => item.Category == ReputationCategory.Generalship);
        }

        [Fact]
        public void DuplicateSameDayEvent_IsIgnored()
        {
            ReputationState state = NewState();
            EncounterContext battle = NobleOutnumbered(state.CurrentDay);
            battle.Summary = "same fight";
            battle.RegionId = "ortysia";

            _scoring.ApplyBattleVictory(state, battle);
            _scoring.ApplyBattleVictory(state, battle);

            Assert.Equal(1, state.Stats.CommandVictories);
            Assert.Single(state.RecentEvents);
        }

        [Fact]
        public void RepeatedLowRiskRaids_SufferDiminishingReturns()
        {
            ReputationState state = NewState();
            float first = 0f;
            float last = 0f;
            for (int i = 0; i < 12; i++)
            {
                EncounterContext raid = Context(state, "raid-" + i);
                raid.Day = state.CurrentDay + i;
                float before = state.Score(ReputationCategory.Raider).Current;
                _scoring.ApplyVillageRaided(state, raid);
                float gained = state.Score(ReputationCategory.Raider).Current - before;
                if (i == 0)
                {
                    first = gained;
                }

                last = gained;
            }

            Assert.True(last < first * 0.5f);
            Assert.True(last >= ReputationBalance.BaseRaid * ReputationBalance.FarmingFloor * 0.5f);
        }

        [Fact]
        public void ScoredGreatGeneralCampaign_UnlocksIronGeneral()
        {
            ReputationState state = NewState();
            NicknameEvaluator evaluator = new NicknameEvaluator(NicknameRegistry.CreateStarter());

            for (int i = 0; i < 10; i++)
            {
                EncounterContext battle = NobleOutnumbered(10 + i);
                battle.PlayerInvolvement = 0.1f;
                battle.Summary = "campaign-" + i;
                _scoring.ApplyBattleVictory(state, battle);
            }

            for (int day = 0; day < 8; day++)
            {
                state.CurrentDay += 1;
                evaluator.ApplyEvaluation(state, state.CurrentDay);
            }

            Assert.Equal("the_iron_general", state.CurrentNicknameId);
        }

        private static ReputationState NewState()
        {
            return new ReputationState
            {
                Age = 30f,
                CurrentDay = 10
            };
        }

        private static EncounterContext Context(ReputationState state, string summary)
        {
            return new EncounterContext
            {
                Day = state.CurrentDay,
                Summary = summary,
                RegionId = "calradia"
            };
        }

        private static EncounterContext Bandits(double day)
        {
            return new EncounterContext
            {
                Day = day,
                PlayerStrength = 120f,
                EnemyStrength = 12f,
                PlayerInvolvement = 0.35f,
                PlayerCommanded = true,
                EnemyIsBandit = true,
                Summary = "looters"
            };
        }

        private static EncounterContext NobleOutnumbered(double day)
        {
            return new EncounterContext
            {
                Day = day,
                PlayerStrength = 70f,
                EnemyStrength = 210f,
                PlayerInvolvement = 0.12f,
                PlayerCommanded = true,
                EnemyIsNobleArmy = true,
                Summary = "noble army"
            };
        }
    }
}
