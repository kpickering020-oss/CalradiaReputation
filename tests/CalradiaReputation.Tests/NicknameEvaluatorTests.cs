using Xunit;

namespace CalradiaReputation.Tests
{
    public sealed class NicknameEvaluatorTests
    {
        private readonly NicknameRegistry _registry = NicknameRegistry.CreateStarter();
        private readonly NicknameEvaluator _evaluator;

        public NicknameEvaluatorTests()
        {
            _evaluator = new NicknameEvaluator(_registry);
        }

        [Fact]
        public void StarterCatalog_HasTenDistinctTitles()
        {
            Assert.Equal(10, _registry.Count);
            Assert.NotNull(_registry.Find("the_iron_general"));
            Assert.NotNull(_registry.Find("the_grey_lion"));
        }

        [Fact]
        public void GreatGeneral_ReceivesCommandTitleInsteadOfPersonalCombat()
        {
            ReputationState state = NewState();
            state.SetScore(ReputationCategory.Generalship, 70f, 70f, 68f);
            state.SetScore(ReputationCategory.Warrior, 22f, 24f, 18f);
            state.Stats.CommandVictories = 12;
            state.Stats.MajorVictories = 5;
            state.Stats.OutnumberedVictories = 3;
            state.Stats.TrivialVictories = 2;
            state.Stats.PersonalHits = 4;

            Tick(_evaluator, state, 8);

            Assert.Equal("the_iron_general", state.CurrentNicknameId);
            Assert.NotEqual("the_bold", state.CurrentNicknameId);
            Assert.Equal("the Iron General", Display(state));
        }

        [Fact]
        public void FallenGeneral_QualifiesForBrokenEagleAfterRepeatedDisasters()
        {
            ReputationState state = NewState();
            NicknameDefinition iron = _registry.Find("the_iron_general");
            NicknameEvaluator.ReplaceNickname(state, iron, 10);

            state.SetScore(ReputationCategory.Generalship, 12f, 74f, 8f);
            state.Stats.CommandVictories = 14;
            state.Stats.MajorVictories = 6;
            state.Stats.MajorDefeats = 5;
            state.Stats.ConsecutiveMajorDefeats = 4;
            state.Stats.OutnumberedVictories = 3;

            Tick(_evaluator, state, 8);

            Assert.Equal("the_broken_eagle", state.CurrentNicknameId);
            Assert.Equal("the_iron_general", state.PreviousNicknameId);
        }

        [Fact]
        public void RetiredGeneral_QualifiesForGildedCrossover()
        {
            ReputationState state = NewState();
            NicknameEvaluator.ReplaceNickname(state, _registry.Find("the_iron_general"), 20);

            state.SetScore(ReputationCategory.Generalship, 30f, 72f, 8f);
            state.SetScore(ReputationCategory.Wealth, 78f, 78f, 76f);
            state.Stats.CommandVictories = 12;
            state.Stats.MajorVictories = 5;
            state.Stats.OutnumberedVictories = 3;
            state.Stats.GoldPeak = 280000;
            state.Stats.CurrentGold = 250000;
            state.Stats.DaysSinceLastBattle = 45;

            Tick(_evaluator, state, 8);

            Assert.Equal("the_gilded_general", state.CurrentNicknameId);
        }

        [Fact]
        public void RedeemedCoward_QualifiesForLionheart()
        {
            ReputationState state = NewState();
            NicknameEvaluator.ReplaceNickname(state, _registry.Find("the_hare"), 5);

            state.SetScore(ReputationCategory.Generalship, 60f, 60f, 58f);
            state.Stats.TimesCaptured = 2;
            state.Stats.ConsecutiveMajorDefeats = 0;
            state.Stats.MajorVictories = 4;
            state.Stats.CommandVictories = 7;
            state.Stats.ConsecutiveVictories = 4;

            Tick(_evaluator, state, 8);

            Assert.Equal("the_lionheart", state.CurrentNicknameId);
            Assert.Equal("the_hare", state.PreviousNicknameId);
        }

        [Fact]
        public void LegendaryWarrior_AgeAloneDoesNotGrantGreyLion()
        {
            ReputationState state = NewState();
            state.Age = 62f;
            state.SetScore(ReputationCategory.Warrior, 20f, 22f, 10f);
            state.Stats.PersonalHits = 8;

            Tick(_evaluator, state, 10);

            Assert.NotEqual("the_grey_lion", state.CurrentNicknameId);
        }

        [Fact]
        public void LegendaryWarrior_OldVeteranWithHistoryGetsGreyLion()
        {
            ReputationState state = NewState();
            state.Age = 51f;
            state.SetScore(ReputationCategory.Warrior, 74f, 80f, 60f);
            state.Stats.PersonalHits = 48;
            state.Stats.MajorVictories = 3;
            state.AddLegacy("legendary_warrior", "Won a famous duel before the gates of Marunath.", ReputationCategory.Warrior, 40f, 100);

            Tick(_evaluator, state, 10);

            Assert.Equal("the_grey_lion", state.CurrentNicknameId);
            Assert.Equal("the Grey Lion", Display(state));
        }

        [Fact]
        public void GenderVariants_UseLionessForFemaleCharacters()
        {
            ReputationState state = NewState();
            state.IsFemale = true;
            state.Age = 51f;
            state.SetScore(ReputationCategory.Warrior, 74f, 80f, 60f);
            state.Stats.PersonalHits = 48;
            state.Stats.MajorVictories = 3;
            state.AddLegacy("legendary_warrior", "Won a famous duel.", ReputationCategory.Warrior, 40f, 100);

            Tick(_evaluator, state, 10);

            Assert.Equal("the Grey Lioness", Display(state));
        }

        [Fact]
        public void LooterFarmer_CannotUnlockRareCommandTitle()
        {
            ReputationState state = NewState();
            state.SetScore(ReputationCategory.Warrior, 40f, 40f, 38f);
            state.SetScore(ReputationCategory.Generalship, 28f, 28f, 26f);
            state.Stats.CommandVictories = 20;
            state.Stats.TrivialVictories = 20;
            state.Stats.BanditFights = 20;
            state.Stats.PersonalHits = 18;

            Tick(_evaluator, state, 10);

            Assert.NotEqual("the_iron_general", state.CurrentNicknameId);
            Assert.NotEqual("the_grey_lion", state.CurrentNicknameId);
            NicknameClaim iron = _evaluator.Score(_registry.Find("the_iron_general"), state);
            Assert.False(iron.Eligible);
        }

        [Fact]
        public void NicknameStability_SmallFluctuationsDoNotSwitchTitle()
        {
            ReputationState state = NewState();
            NicknameEvaluator.ReplaceNickname(state, _registry.Find("the_iron_general"), 30);
            state.SetScore(ReputationCategory.Generalship, 68f, 70f, 60f);
            state.SetScore(ReputationCategory.Warrior, 30f, 30f, 28f);
            state.Stats.CommandVictories = 12;
            state.Stats.MajorVictories = 5;
            state.Stats.OutnumberedVictories = 3;
            state.Stats.PersonalHits = 10;

            string before = state.CurrentNicknameId;
            state.Score(ReputationCategory.Warrior).Add(3f, state.CurrentDay);
            Tick(_evaluator, state, 6);

            Assert.Equal(before, state.CurrentNicknameId);
            Assert.Equal("the_iron_general", state.CurrentNicknameId);
        }

        private string Display(ReputationState state)
        {
            NicknameDefinition definition = _registry.Find(state.CurrentNicknameId);
            Assert.NotNull(definition);
            return definition.GetDisplayText(state.IsFemale);
        }

        private static ReputationState NewState()
        {
            return new ReputationState
            {
                Age = 28f,
                CurrentDay = 40
            };
        }

        private static void Tick(NicknameEvaluator evaluator, ReputationState state, int days)
        {
            for (int i = 0; i < days; i++)
            {
                state.CurrentDay += 1;
                evaluator.ApplyEvaluation(state, state.CurrentDay);
            }
        }
    }
}
