using System;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class NicknameHistoryEntry
    {
        public string NicknameId { get; set; }
        public string DisplayText { get; set; }
        public int CampaignDay { get; set; }
        public int AgeYears { get; set; }
        public string Reason { get; set; }
        public DateTime TimestampUtc { get; set; }
    }
}
