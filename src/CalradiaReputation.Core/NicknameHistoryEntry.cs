namespace CalradiaReputation
{
    public sealed class NicknameHistoryEntry
    {
        public string NicknameId { get; set; }
        public double AcquiredDay { get; set; }
        public double LostDay { get; set; }
        public float ClaimScore { get; set; }

        public NicknameHistoryEntry()
        {
            NicknameId = string.Empty;
        }
    }
}
