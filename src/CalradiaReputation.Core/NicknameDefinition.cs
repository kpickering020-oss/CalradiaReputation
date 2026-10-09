namespace CalradiaReputation
{
    public sealed class NicknameDefinition
    {
        public string Id { get; set; }
        public string MaleText { get; set; }
        public string FemaleText { get; set; }
        public NicknameRarity Rarity { get; set; }
        public ReputationTag Tags { get; set; }
        public int Priority { get; set; }
        public float ExtraResistance { get; set; }
        public EvolutionType Evolution { get; set; }
        public NicknameRequirements Requirements { get; set; }

        public NicknameDefinition()
        {
            Id = string.Empty;
            MaleText = string.Empty;
            FemaleText = string.Empty;
            Requirements = new NicknameRequirements();
        }

        public string GetDisplayText(bool isFemale)
        {
            if (isFemale && !string.IsNullOrEmpty(FemaleText))
            {
                return FemaleText;
            }

            return MaleText;
        }

        public float ReplacementResistance()
        {
            return ReputationBalance.ResistanceFor(Rarity) + ExtraResistance;
        }
    }
}
