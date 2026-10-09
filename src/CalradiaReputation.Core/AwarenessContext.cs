namespace CalradiaReputation
{
    public sealed class AwarenessContext
    {
        public float ClanRenown { get; set; }
        public float Distance { get; set; }
        public bool SameFaction { get; set; }
        public bool NearbyRecentDeed { get; set; }
        public bool IsCompanionInParty { get; set; }
        public NicknameRarity Rarity { get; set; }
        public float NicknameFame { get; set; }
    }
}
