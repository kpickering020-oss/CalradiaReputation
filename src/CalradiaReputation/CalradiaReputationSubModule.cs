using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CalradiaReputation.Module
{
    public sealed class CalradiaReputationSubModule : MBSubModuleBase
    {
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            CampaignGameStarter campaignStarter = gameStarterObject as CampaignGameStarter;
            if (campaignStarter == null)
            {
                return;
            }

            campaignStarter.AddBehavior(new ReputationCampaignBehavior());
            campaignStarter.AddBehavior(new ReputationDialogueBehavior());
        }
    }
}
