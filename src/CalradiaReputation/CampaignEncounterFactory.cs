using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace CalradiaReputation.Module
{
    internal static class CampaignEncounterFactory
    {
        public static double Today()
        {
            return CampaignTime.Now.ToDays;
        }

        public static EncounterContext FromMapEvent(MapEvent mapEvent, int personalHits)
        {
            EncounterContext context = new EncounterContext
            {
                Day = Today(),
                PlayerCommanded = true,
                PlayerInvolvement = 0.2f
            };

            if (mapEvent == null)
            {
                return context;
            }

            MapEventSide playerSide = mapEvent.PlayerSide == BattleSideEnum.Attacker ? mapEvent.AttackerSide : mapEvent.DefenderSide;
            MapEventSide enemySide = playerSide != null ? playerSide.OtherSide : null;
            context.PlayerStrength = StrengthOf(playerSide);
            context.EnemyStrength = StrengthOf(enemySide);
            context.PlayerCommanded = playerSide != null && playerSide.LeaderParty != null
                && (playerSide.LeaderParty == PartyBase.MainParty || playerSide.LeaderParty.LeaderHero == Hero.MainHero);

            bool bandit = false;
            bool noble = false;
            if (enemySide != null && enemySide.LeaderParty != null)
            {
                MobileParty enemyParty = enemySide.LeaderParty.MobileParty;
                if (enemyParty != null)
                {
                    bandit = enemyParty.IsBandit;
                }

                Hero leader = enemySide.LeaderParty.LeaderHero;
                if (leader != null && (leader.IsLord || leader.Occupation == Occupation.Lord))
                {
                    noble = true;
                }
            }

            context.EnemyIsBandit = bandit;
            context.EnemyIsNobleArmy = noble && !bandit;
            if (mapEvent.IsPlayerSimulation)
            {
                context.PlayerInvolvement = 0.12f;
            }
            else
            {
                context.PlayerInvolvement = Clamp(personalHits / 10f, 0.08f, 1f);
            }

            Settlement settlement = mapEvent.MapEventSettlement;
            if (settlement != null)
            {
                context.RegionId = settlement.StringId ?? string.Empty;
                context.IsTown = settlement.IsTown;
                context.IsCastle = settlement.IsCastle;
                context.IsVillage = settlement.IsVillage;
            }
            else if (MobileParty.MainParty != null)
            {
                context.RegionId = "field:" + Math.Round(MobileParty.MainParty.GetPosition2D.X) + "," + Math.Round(MobileParty.MainParty.GetPosition2D.Y);
            }

            context.Summary = mapEvent.EventType + "@" + context.RegionId;
            return context;
        }

        public static EncounterContext ForHero(Hero hero, string summary)
        {
            return new EncounterContext
            {
                Day = Today(),
                RegionId = hero != null && hero.CurrentSettlement != null ? hero.CurrentSettlement.StringId : string.Empty,
                Summary = summary
            };
        }

        public static EncounterContext ForSettlement(Settlement settlement, string summary)
        {
            EncounterContext context = new EncounterContext
            {
                Day = Today(),
                Summary = summary
            };
            if (settlement != null)
            {
                context.RegionId = settlement.StringId ?? string.Empty;
                context.IsTown = settlement.IsTown;
                context.IsCastle = settlement.IsCastle;
                context.IsVillage = settlement.IsVillage;
            }

            return context;
        }

        private static float StrengthOf(MapEventSide side)
        {
            if (side == null)
            {
                return 1f;
            }

            float strength = side.RecalculateStrengthOfSide();
            if (strength < 1f)
            {
                strength = Math.Max(1, side.TroopCount);
            }

            return strength;
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
