using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;

namespace CalradiaReputation.Module
{
    public sealed class ReputationDialogueBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (starter == null)
            {
                return;
            }

            starter.AddPlayerLine(
                "cr_ask_troops",
                "hero_main_options",
                "cr_troop_response",
                "{=cr_ask_troops}What do the troops think of me?",
                IsCompanionInPlayerParty,
                null);

            starter.AddDialogLine(
                "cr_troop_response",
                "cr_troop_response",
                "hero_main_options",
                "{=cr_troop_reply}{CR_TROOP_TALK}",
                SetCompanionReply,
                null);

            starter.AddPlayerLine(
                "cr_ask_name",
                "hero_main_options",
                "cr_npc_response",
                "{=cr_ask_name}Does my name mean anything in these parts?",
                IsNpcWhoMayKnow,
                null);

            starter.AddDialogLine(
                "cr_npc_response",
                "cr_npc_response",
                "hero_main_options",
                "{=cr_npc_reply}{CR_NPC_REMARK}",
                SetNpcReply,
                MarkNpcMention);

            starter.AddDialogLine(
                "cr_npc_greeting",
                "start",
                "hero_main_options",
                "{=cr_npc_greet}{CR_NPC_REMARK}",
                NpcGreetingCondition,
                MarkNpcMention,
                80);
        }

        private bool IsCompanionInPlayerParty()
        {
            Hero hero = Hero.OneToOneConversationHero;
            return hero != null && hero.IsPlayerCompanion && hero.PartyBelongedTo == MobileParty.MainParty;
        }

        private bool IsNpcWhoMayKnow()
        {
            Hero hero = Hero.OneToOneConversationHero;
            ReputationCampaignBehavior behavior = ReputationCampaignBehavior.Instance;
            if (hero == null || behavior == null || IsCompanionInPlayerParty())
            {
                return false;
            }

            return hero.IsLord || hero.IsNotable;
        }

        private bool SetCompanionReply()
        {
            ReputationCampaignBehavior behavior = ReputationCampaignBehavior.Instance;
            Hero hero = Hero.OneToOneConversationHero;
            if (behavior == null || hero == null)
            {
                MBTextManager.SetTextVariable("CR_TROOP_TALK", "The men haven't quite made up their minds about you.");
                return true;
            }

            behavior.RefreshHeroSnapshot();
            int mercy = hero.GetTraitLevel(DefaultTraits.Mercy);
            int honor = hero.GetTraitLevel(DefaultTraits.Honor);

            string text = TroopTalk.CompanionAnswer(behavior.State, behavior.Registry, mercy, honor);
            MBTextManager.SetTextVariable("CR_TROOP_TALK", text);
            return true;
        }

        private bool SetNpcReply()
        {
            FillNpcRemark();
            return true;
        }

        private bool NpcGreetingCondition()
        {
            Hero hero = Hero.OneToOneConversationHero;
            ReputationCampaignBehavior behavior = ReputationCampaignBehavior.Instance;
            if (hero == null || behavior == null || IsCompanionInPlayerParty())
            {
                return false;
            }

            if (!behavior.CanMention(hero) || string.IsNullOrEmpty(behavior.State.CurrentNicknameId))
            {
                return false;
            }

            int roll = hero.StringId.GetHashCode() & 3;
            if (roll != 0)
            {
                return false;
            }

            return FillNpcRemark();
        }

        private bool FillNpcRemark()
        {
            ReputationCampaignBehavior behavior = ReputationCampaignBehavior.Instance;
            if (behavior == null)
            {
                MBTextManager.SetTextVariable("CR_NPC_REMARK", "I have not heard a name for you.");
                return true;
            }

            behavior.RefreshHeroSnapshot();
            NicknameDefinition current = behavior.Registry.Find(behavior.State.CurrentNicknameId);
            if (current == null)
            {
                MBTextManager.SetTextVariable("CR_NPC_REMARK", "I have not heard a name for you.");
                return true;
            }

            if (!behavior.HeroKnowsNickname(Hero.OneToOneConversationHero))
            {
                MBTextManager.SetTextVariable("CR_NPC_REMARK", "I do not know what name the realm has given you.");
                return true;
            }

            string text = TroopTalk.NpcRemark(behavior.State, behavior.Registry, TroopTalk.ToneFor(current));
            MBTextManager.SetTextVariable("CR_NPC_REMARK", text);
            return true;
        }

        private void MarkNpcMention()
        {
            ReputationCampaignBehavior behavior = ReputationCampaignBehavior.Instance;
            if (behavior != null)
            {
                behavior.MarkMentioned(Hero.OneToOneConversationHero);
            }
        }
    }
}
