namespace CalradiaReputation
{
    public sealed class AwarenessRecord
    {
        public string HeroId { get; set; }
        public bool KnowsNickname { get; set; }
        public double LastMentionDay { get; set; }

        public AwarenessRecord()
        {
            HeroId = string.Empty;
        }
    }
}
