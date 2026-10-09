using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CalradiaReputation.Module
{
    public sealed class ReputationCampaignBehavior : CampaignBehaviorBase
    {
        private readonly ReputationScoringEngine _scoring = new ReputationScoringEngine();
        private readonly NicknameRegistry _registry = NicknameRegistry.CreateStarter();
        private readonly NicknameEvaluator _evaluator;
        private ReputationState _state = new ReputationState();
        private int _personalHitsThisBattle;

        public ReputationCampaignBehavior()
        {
            _evaluator = new NicknameEvaluator(_registry);
        }

        public static ReputationCampaignBehavior Instance
        {
            get
            {
                if (Campaign.Current == null)
                {
                    return null;
                }

                return Campaign.Current.GetCampaignBehavior<ReputationCampaignBehavior>();
            }
        }

        public ReputationState State => _state;

        public NicknameRegistry Registry => _registry;

        public override void RegisterEvents()
        {
            CampaignEvents.OnPlayerBattleEndEvent.AddNonSerializedListener(this, OnPlayerBattleEnd);
            CampaignEvents.OnHeroCombatHitEvent.AddNonSerializedListener(this, OnHeroCombatHit);
            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, OnMapEventStarted);
            CampaignEvents.HeroKilledEvent.AddNonSerializedListener(this, OnHeroKilled);
            CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener(this, OnHeroPrisonerReleased);
            CampaignEvents.HeroPrisonerTaken.AddNonSerializedListener(this, OnHeroPrisonerTaken);
            CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChanged);
            CampaignEvents.VillageLooted.AddNonSerializedListener(this, OnVillageLooted);
            CampaignEvents.OnPlayerTradeProfitEvent.AddNonSerializedListener(this, OnPlayerTradeProfit);
            CampaignEvents.TournamentFinished.AddNonSerializedListener(this, OnTournamentFinished);
            CampaignEvents.PlayerEliminatedFromTournament.AddNonSerializedListener(this, OnPlayerEliminatedFromTournament);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.OnPlayerMetHeroEvent.AddNonSerializedListener(this, OnPlayerMetHero);
        }

        public override void SyncData(IDataStore dataStore)
        {
            ReputationSaveData data = dataStore.IsSaving ? ReputationSaveCodec.Pack(_state) : new ReputationSaveData();
            dataStore.SyncData("cr_schema", ref data.SchemaVersion);
            dataStore.SyncData("cr_nick", ref data.CurrentNicknameId);
            dataStore.SyncData("cr_prev", ref data.PreviousNicknameId);
            dataStore.SyncData("cr_nick_day", ref data.NicknameEstablishedDay);
            dataStore.SyncData("cr_pending", ref data.PendingChallengerId);
            dataStore.SyncData("cr_streak", ref data.ChallengerStreakDays);
            dataStore.SyncData("cr_female", ref data.IsFemale);
            dataStore.SyncData("cr_age", ref data.Age);
            dataStore.SyncData("cr_day", ref data.CurrentDay);
            dataStore.SyncData("cr_cur", ref data.CurrentScores);
            dataStore.SyncData("cr_peak", ref data.PeakScores);
            dataStore.SyncData("cr_recent", ref data.RecentScores);
            dataStore.SyncData("cr_activity", ref data.LastActivityDays);
            dataStore.SyncData("cr_stats", ref data.Stats);
            dataStore.SyncData("cr_ekind", ref data.EventKinds);
            dataStore.SyncData("cr_eday", ref data.EventDays);
            dataStore.SyncData("cr_ecat", ref data.EventCategories);
            dataStore.SyncData("cr_eamt", ref data.EventAmounts);
            dataStore.SyncData("cr_ediff", ref data.EventDifficulties);
            dataStore.SyncData("cr_eimp", ref data.EventImportances);
            dataStore.SyncData("cr_estr", ref data.EventEnemyStrengths);
            dataStore.SyncData("cr_einv", ref data.EventInvolvements);
            dataStore.SyncData("cr_eflag", ref data.EventFlags);
            dataStore.SyncData("cr_ereg", ref data.EventRegions);
            dataStore.SyncData("cr_esum", ref data.EventSummaries);
            dataStore.SyncData("cr_lid", ref data.LegacyIds);
            dataStore.SyncData("cr_lsum", ref data.LegacySummaries);
            dataStore.SyncData("cr_lday", ref data.LegacyDays);
            dataStore.SyncData("cr_lcat", ref data.LegacyCategories);
            dataStore.SyncData("cr_lw", ref data.LegacyWeights);
            dataStore.SyncData("cr_hid", ref data.HistoryIds);
            dataStore.SyncData("cr_hacq", ref data.HistoryAcquired);
            dataStore.SyncData("cr_hlost", ref data.HistoryLost);
            dataStore.SyncData("cr_hscore", ref data.HistoryScores);
            dataStore.SyncData("cr_era_n", ref data.EraNames);
            dataStore.SyncData("cr_era_s", ref data.EraStarts);
            dataStore.SyncData("cr_era_e", ref data.EraEnds);
            dataStore.SyncData("cr_era_c", ref data.EraCategories);
            dataStore.SyncData("cr_era_dom", ref data.DominantEraName);
            dataStore.SyncData("cr_life_dom", ref data.LifetimeDominant);
            dataStore.SyncData("cr_rec_dom", ref data.RecentDominant);
            dataStore.SyncData("cr_aware", ref data.AwareHeroIds);
            dataStore.SyncData("cr_ment_id", ref data.MentionHeroIds);
            dataStore.SyncData("cr_ment_day", ref data.MentionDays);
            if (dataStore.IsLoading)
            {
                ReputationSaveCodec.Unpack(_state, data);
            }
        }

        private void OnMapEventStarted(MapEvent mapEvent, PartyBase attacker, PartyBase defender)
        {
            if (mapEvent != null && mapEvent.IsPlayerMapEvent)
            {
                _personalHitsThisBattle = 0;
            }
        }

        private void OnHeroCombatHit(CharacterObject attacker, CharacterObject victim, PartyBase attackerParty, WeaponComponentData weapon, bool isFatal, int damage)
        {
            Safe(() =>
            {
                if (attacker != null && attacker.IsPlayerCharacter)
                {
                    _personalHitsThisBattle++;
                }
            });
        }

        private void OnPlayerBattleEnd(MapEvent mapEvent)
        {
            Safe(() =>
            {
                if (mapEvent == null || !mapEvent.IsPlayerMapEvent)
                {
                    return;
                }

                EncounterContext context = CampaignEncounterFactory.FromMapEvent(mapEvent, _personalHitsThisBattle);
                bool won = mapEvent.HasWinner && mapEvent.WinningSide == mapEvent.PlayerSide;
                if (mapEvent.IsRaid)
                {
                    if (won && mapEvent.PlayerSide == BattleSideEnum.Attacker)
                    {
                        context.Summary = "loot:" + context.RegionId;
                        _scoring.ApplyVillageRaided(_state, context);
                    }
                    else if (!won)
                    {
                        _scoring.ApplyBattleDefeat(_state, context);
                    }
                }
                else if (mapEvent.IsHideoutBattle)
                {
                    if (won)
                    {
                        _scoring.ApplyHideoutCleared(_state, context);
                    }
                    else
                    {
                        _scoring.ApplyBattleDefeat(_state, context);
                    }
                }
                else if (mapEvent.IsSiegeAssault && won && mapEvent.PlayerSide == BattleSideEnum.Defender)
                {
                    _scoring.ApplySettlementDefended(_state, context);
                }
                else if (won)
                {
                    _scoring.ApplyBattleVictory(_state, context);
                }
                else
                {
                    _scoring.ApplyBattleDefeat(_state, context);
                }

                _personalHitsThisBattle = 0;
            });
        }

        private void OnHeroKilled(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail detail, bool showNotification)
        {
            Safe(() =>
            {
                if (killer != Hero.MainHero || victim == null)
                {
                    return;
                }

                if (detail != KillCharacterAction.KillCharacterActionDetail.Executed
                    && detail != KillCharacterAction.KillCharacterActionDetail.ExecutionAfterMapEvent)
                {
                    return;
                }

                if (!victim.IsLord)
                {
                    return;
                }

                _scoring.ApplyNobleExecuted(_state, CampaignEncounterFactory.ForHero(victim, "exec:" + victim.StringId));
            });
        }

        private void OnHeroPrisonerReleased(Hero prisoner, PartyBase party, IFaction faction, EndCaptivityDetail detail, bool showNotification)
        {
            Safe(() =>
            {
                if (prisoner == null || !prisoner.IsLord || detail != EndCaptivityDetail.ReleasedByChoice)
                {
                    return;
                }

                if (party != PartyBase.MainParty && (party == null || party.LeaderHero != Hero.MainHero))
                {
                    return;
                }

                _scoring.ApplyNobleReleased(_state, CampaignEncounterFactory.ForHero(prisoner, "release:" + prisoner.StringId));
            });
        }

        private void OnHeroPrisonerTaken(PartyBase capturer, Hero prisoner)
        {
            Safe(() =>
            {
                if (prisoner == Hero.MainHero)
                {
                    _scoring.ApplyPlayerCaptured(_state, new EncounterContext { Day = CampaignEncounterFactory.Today(), Summary = "captured" });
                }
            });
        }

        private void OnSettlementOwnerChanged(Settlement settlement, bool openToClaim, Hero newOwner, Hero oldOwner, Hero capturerHero, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            Safe(() =>
            {
                Hero player = Hero.MainHero;
                if (settlement == null || player == null)
                {
                    return;
                }

                EncounterContext context = CampaignEncounterFactory.ForSettlement(settlement, "owner:" + settlement.StringId);
                bool gained = capturerHero == player || newOwner == player || (newOwner != null && newOwner.Clan == player.Clan);
                bool lost = oldOwner == player || (oldOwner != null && oldOwner.Clan == player.Clan);
                if (gained && !lost)
                {
                    _scoring.ApplySettlementCaptured(_state, context);
                }
                else if (lost && !gained)
                {
                    _scoring.ApplySettlementLost(_state, context);
                }
            });
        }

        private void OnVillageLooted(Village village)
        {
            Safe(() =>
            {
                if (village == null || village.Settlement == null)
                {
                    return;
                }

                MobileParty attacker = village.Settlement.LastAttackerParty;
                if (attacker == null || !attacker.IsMainParty)
                {
                    return;
                }

                EncounterContext context = CampaignEncounterFactory.ForSettlement(village.Settlement, "loot:" + village.Settlement.StringId);
                context.IsVillage = true;
                _scoring.ApplyVillageRaided(_state, context);
            });
        }

        private void OnPlayerTradeProfit(int profit)
        {
            Safe(() =>
            {
                EncounterContext context = new EncounterContext
                {
                    Day = CampaignEncounterFactory.Today(),
                    Profit = profit,
                    Summary = "trade:" + profit
                };
                _scoring.ApplyTradeProfit(_state, context);
            });
        }

        private void OnTournamentFinished(CharacterObject winner, MBReadOnlyList<CharacterObject> participants, Town town, ItemObject prize)
        {
            Safe(() =>
            {
                if (winner != null && winner.IsPlayerCharacter)
                {
                    EncounterContext context = CampaignEncounterFactory.ForSettlement(town != null ? town.Settlement : null, "tourney-win");
                    _scoring.ApplyTournamentResult(_state, context, true);
                }
            });
        }

        private void OnPlayerEliminatedFromTournament(int round, Town town)
        {
            Safe(() =>
            {
                EncounterContext context = CampaignEncounterFactory.ForSettlement(town != null ? town.Settlement : null, "tourney-loss:" + round);
                _scoring.ApplyTournamentResult(_state, context, false);
            });
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom, ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification)
        {
            Safe(() =>
            {
                if (clan == null || Hero.MainHero == null || clan != Hero.MainHero.Clan)
                {
                    return;
                }

                bool defected = detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinKingdomByDefection;
                bool joined = detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinKingdom
                    || detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinAsMercenary;
                if (defected || joined)
                {
                    _scoring.ApplyFactionJoined(_state, new EncounterContext { Day = CampaignEncounterFactory.Today(), Summary = detail.ToString() }, defected);
                }
            });
        }

        private void OnPlayerMetHero(Hero hero)
        {
            Safe(() => UpdateAwareness(hero, true));
        }

        private void OnDailyTick()
        {
            Safe(() =>
            {
                RefreshHeroSnapshot();
                _scoring.DailyTick(_state, CampaignEncounterFactory.Today());
                if (Hero.MainHero != null)
                {
                    _scoring.ApplyWealthSnapshot(_state, new EncounterContext
                    {
                        Day = _state.CurrentDay,
                        Gold = Hero.MainHero.Gold,
                        Summary = "gold"
                    });
                    if (Hero.MainHero.OwnedCaravans != null && Hero.MainHero.OwnedCaravans.Count > _state.Stats.CaravansOwnedPeak)
                    {
                        _state.Stats.CaravansOwnedPeak = Hero.MainHero.OwnedCaravans.Count;
                    }
                }

                _evaluator.ApplyEvaluation(_state, _state.CurrentDay);
            });
        }

        public void RefreshHeroSnapshot()
        {
            Hero player = Hero.MainHero;
            if (player == null)
            {
                return;
            }

            _state.IsFemale = player.IsFemale;
            _state.Age = player.Age;
            _state.Stats.CurrentGold = player.Gold;
            if (player.IsKingdomLeader || player.IsClanLeader)
            {
                _state.Stats.DaysAsRuler = Math.Max(_state.Stats.DaysAsRuler, 1);
            }
        }

        public bool HeroKnowsNickname(Hero hero)
        {
            if (hero == null)
            {
                return false;
            }

            if (hero.IsPlayerCompanion && hero.PartyBelongedTo == MobileParty.MainParty)
            {
                return true;
            }

            AwarenessRecord record = _state.GetAwareness(hero.StringId);
            if (record != null && record.KnowsNickname)
            {
                return true;
            }

            UpdateAwareness(hero, false);
            record = _state.GetAwareness(hero.StringId);
            return record != null && record.KnowsNickname;
        }

        public bool CanMention(Hero hero)
        {
            if (hero == null || !HeroKnowsNickname(hero))
            {
                return false;
            }

            AwarenessRecord record = _state.GetAwareness(hero.StringId);
            return record != null && CampaignEncounterFactory.Today() - record.LastMentionDay >= ReputationBalance.MentionCooldownDays;
        }

        public void MarkMentioned(Hero hero)
        {
            if (hero == null)
            {
                return;
            }

            AwarenessRecord record = _state.GetAwareness(hero.StringId);
            if (record != null)
            {
                record.LastMentionDay = CampaignEncounterFactory.Today();
                record.KnowsNickname = true;
            }
        }

        private void UpdateAwareness(Hero hero, bool forceEvaluate)
        {
            if (hero == null || string.IsNullOrEmpty(hero.StringId))
            {
                return;
            }

            NicknameDefinition current = _registry.Find(_state.CurrentNicknameId);
            AwarenessContext context = new AwarenessContext
            {
                IsCompanionInParty = hero.IsPlayerCompanion && hero.PartyBelongedTo == MobileParty.MainParty,
                ClanRenown = Hero.MainHero != null && Hero.MainHero.Clan != null ? Hero.MainHero.Clan.Renown : 0f,
                Distance = DistanceTo(hero),
                SameFaction = Hero.MainHero != null && hero.MapFaction != null && hero.MapFaction == Hero.MainHero.MapFaction,
                NearbyRecentDeed = HasNearbyDeed(hero),
                Rarity = current != null ? current.Rarity : NicknameRarity.Common,
                NicknameFame = current != null ? _state.Score(current.Requirements.FocusCategory).Peak : 0f
            };

            AwarenessRecord record = _state.GetAwareness(hero.StringId);
            if (record == null)
            {
                return;
            }

            if (forceEvaluate || !record.KnowsNickname)
            {
                record.KnowsNickname = NicknameAwareness.KnowsNickname(context);
            }
        }

        private static float DistanceTo(Hero hero)
        {
            if (MobileParty.MainParty == null)
            {
                return 999f;
            }

            Vec2 player = MobileParty.MainParty.GetPosition2D;
            Vec2 other = player;
            if (hero.PartyBelongedTo != null)
            {
                other = hero.PartyBelongedTo.GetPosition2D;
            }
            else if (hero.CurrentSettlement != null)
            {
                other = hero.CurrentSettlement.GetPosition2D;
            }

            return player.Distance(other);
        }

        private bool HasNearbyDeed(Hero hero)
        {
            string region = hero.CurrentSettlement != null ? hero.CurrentSettlement.StringId : string.Empty;
            if (region.Length == 0)
            {
                return false;
            }

            double minDay = _state.CurrentDay - 21;
            for (int i = 0; i < _state.RecentEvents.Count; i++)
            {
                ReputationEvent item = _state.RecentEvents[i];
                if (item.Day >= minDay && item.RegionId == region)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Safe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Debug.Print("[CalradiaReputation] " + ex);
            }
        }
    }
}
