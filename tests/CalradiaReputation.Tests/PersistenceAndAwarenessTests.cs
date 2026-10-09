using Xunit;

namespace CalradiaReputation.Tests
{
    public sealed class PersistenceAndAwarenessTests
    {
        [Fact]
        public void SaveAndReload_PreservesNicknameHistoryAndLegacy()
        {
            ReputationState original = new ReputationState { Age = 41f, IsFemale = true, CurrentDay = 120 };
            original.CurrentNicknameId = "the_iron_general";
            original.PreviousNicknameId = "the_bold";
            original.NicknameEstablishedDay = 90;
            original.SetScore(ReputationCategory.Generalship, 66f, 70f, 40f);
            original.Stats.CommandVictories = 11;
            original.Stats.MajorVictories = 4;
            original.AddLegacy("legendary_warrior", "A famous stand.", ReputationCategory.Warrior, 22f, 80);
            original.NicknameHistory.Add(new NicknameHistoryEntry
            {
                NicknameId = "the_bold",
                AcquiredDay = 20,
                LostDay = 90,
                ClaimScore = 40f
            });
            original.Career.RecordEra("the_bold", 20, ReputationCategory.Warrior);
            original.GetAwareness("hero_npc")!.KnowsNickname = true;
            original.GetAwareness("hero_npc")!.LastMentionDay = 110;

            ReputationState loaded = new ReputationState();
            ReputationSaveCodec.Unpack(loaded, ReputationSaveCodec.Pack(original));

            Assert.Equal("the_iron_general", loaded.CurrentNicknameId);
            Assert.Equal("the_bold", loaded.PreviousNicknameId);
            Assert.Equal(66f, loaded.Score(ReputationCategory.Generalship).Current);
            Assert.Equal(70f, loaded.Score(ReputationCategory.Generalship).Peak);
            Assert.Equal(11, loaded.Stats.CommandVictories);
            Assert.True(loaded.HasLegacy("legendary_warrior"));
            Assert.Single(loaded.NicknameHistory);
            Assert.True(loaded.GetAwareness("hero_npc")!.KnowsNickname);
            Assert.True(loaded.IsFemale);
        }

        [Fact]
        public void NearbyFactionNpc_IsMoreLikelyToKnowNicknameThanDistantStranger()
        {
            AwarenessContext nearby = new AwarenessContext
            {
                ClanRenown = 400f,
                Distance = 6f,
                SameFaction = true,
                NearbyRecentDeed = true,
                Rarity = NicknameRarity.Rare,
                NicknameFame = 70f
            };
            AwarenessContext distant = new AwarenessContext
            {
                ClanRenown = 40f,
                Distance = 140f,
                SameFaction = false,
                NearbyRecentDeed = false,
                Rarity = NicknameRarity.Common,
                NicknameFame = 15f
            };

            Assert.True(NicknameAwareness.KnowledgeChance(nearby) > NicknameAwareness.KnowledgeChance(distant));
            Assert.True(NicknameAwareness.KnowsNickname(nearby));
            Assert.False(NicknameAwareness.KnowsNickname(distant));
        }

        [Fact]
        public void CompanionInParty_AlwaysKnowsNickname()
        {
            Assert.True(NicknameAwareness.KnowsNickname(new AwarenessContext { IsCompanionInParty = true, Distance = 200f, Rarity = NicknameRarity.Common }));
        }

        [Fact]
        public void TroopTalk_UsesCurrentAndPreviousTitlesWithoutNumbers()
        {
            ReputationState state = new ReputationState { IsFemale = false, CurrentDay = 50, NicknameEstablishedDay = 10 };
            NicknameRegistry registry = NicknameRegistry.CreateStarter();
            NicknameEvaluator.ReplaceNickname(state, registry.Find("the_iron_general")!, 10);
            NicknameEvaluator.ReplaceNickname(state, registry.Find("the_gilded_general")!, 40);

            string text = TroopTalk.CompanionAnswer(state, registry, mercyTrait: 1, honorTrait: 0);

            Assert.Contains("Gilded General", text);
            Assert.DoesNotContain("52", text);
            Assert.DoesNotContain("Generalship", text);
        }

        [Fact]
        public void Gender_GreyLionessAppearsInNpcRemark()
        {
            ReputationState state = new ReputationState { IsFemale = true };
            NicknameRegistry registry = NicknameRegistry.CreateStarter();
            state.CurrentNicknameId = "the_grey_lion";
            string text = TroopTalk.NpcRemark(state, registry, ReputationTag.Veteran);
            Assert.Contains("Grey Lioness", text);
        }
    }
}
