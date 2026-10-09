namespace CalradiaReputation
{
    public static class NicknameAwareness
    {
        public static bool CompanionAlwaysKnows(AwarenessContext context)
        {
            return context != null && context.IsCompanionInParty;
        }

        public static float KnowledgeChance(AwarenessContext context)
        {
            if (context == null)
            {
                return 0f;
            }

            if (context.IsCompanionInParty)
            {
                return 1f;
            }

            float chance = 0.08f;
            chance += Clamp01(context.NicknameFame / 100f) * 0.28f;
            chance += Clamp01(context.ClanRenown / 800f) * 0.18f;
            chance += RaritySpread(context.Rarity);
            chance += context.SameFaction ? 0.12f : 0f;
            chance += context.NearbyRecentDeed ? 0.16f : 0f;
            chance -= DistancePenalty(context.Distance);
            return Clamp01(chance);
        }

        public static bool KnowsNickname(AwarenessContext context)
        {
            return KnowledgeChance(context) >= ReputationBalance.AwarenessKnowsThreshold;
        }

        private static float RaritySpread(NicknameRarity rarity)
        {
            switch (rarity)
            {
                case NicknameRarity.Legendary:
                    return 0.22f;
                case NicknameRarity.Rare:
                    return 0.12f;
                case NicknameRarity.Uncommon:
                    return 0.05f;
                default:
                    return 0f;
            }
        }

        private static float DistancePenalty(float distance)
        {
            if (distance <= 8f)
            {
                return 0f;
            }

            if (distance <= 40f)
            {
                return 0.08f;
            }

            if (distance <= 90f)
            {
                return 0.18f;
            }

            return 0.3f;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
