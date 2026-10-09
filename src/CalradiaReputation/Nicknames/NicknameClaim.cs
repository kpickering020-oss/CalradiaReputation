using System;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class NicknameDefinition
    {
        public string Id { get; set; }
        public string MaleText { get; set; }
        public string FemaleText { get; set; }
        public string Tier { get; set; }
        public int Priority { get; set; }
        public float ClaimScore { get; set; }
        public float ReplacementResistance { get; set; }
        public int RequiredAgeYears { get; set; }
        public int RequiredCareerDays { get; set; }
        public string PreviousTitleId { get; set; }
        public string[] HiddenRequirements { get; set; } = Array.Empty<string>();

        public string GetDisplayText(bool isFemale)
        {
            return isFemale && !string.IsNullOrWhiteSpace(FemaleText) ? FemaleText : MaleText;
        }
    }
}
