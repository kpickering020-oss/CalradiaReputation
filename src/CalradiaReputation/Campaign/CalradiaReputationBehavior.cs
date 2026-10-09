using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace CalradiaReputation
{
    public sealed class CalradiaReputationBehavior : CampaignBehaviorBase
    {
        private readonly ReputationState _state = new ReputationState();

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, OnNewGameCreated);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("CalradiaReputation_State", ref _state);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            if (Campaign.Current == null)
            {
                return;
            }

            _state.EnsureInitialized();
            _state.LastKnownCampaignDay = CampaignTime.Now.ToDays;
        }

        private void OnDailyTick()
        {
            _state.TrackDailyDecay();
            _state.EnsureInitialized();
        }

        private void OnNewGameCreated(CampaignGameStarter starter)
        {
            _state.EnsureInitialized();
        }

        public ReputationState State
        {
            get { return _state; }
        }
    }
}
