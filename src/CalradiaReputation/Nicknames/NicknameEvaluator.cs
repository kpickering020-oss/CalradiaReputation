using System;
using System.Collections.Generic;
using System.Linq;

namespace CalradiaReputation
{
    public sealed class NicknameRegistry
    {
        private readonly List<NicknameDefinition> _definitions;

        public NicknameRegistry()
        {
            _definitions = new List<NicknameDefinition>
            {
                new NicknameDefinition
                {
                    Id = "wolf",
                    MaleText = "the Wolf",
                    FemaleText = "the She-Wolf",
                    Tier = "Common",
                    Priority = 10,
                    ClaimScore = 12f,
                    ReplacementResistance = 6f,
                    RequiredAgeYears = 20,
                    RequiredCareerDays = 150,
                    HiddenRequirements = new[] { "warrior", "not_coward" }
                },
                new NicknameDefinition
                {
                    Id = "great_wolf",
                    MaleText = "the Great Wolf",
                    FemaleText = "the Great She-Wolf",
                    Tier = "Uncommon",
                    Priority = 20,
                    ClaimScore = 22f,
                    ReplacementResistance = 9f,
                    RequiredAgeYears = 25,
                    RequiredCareerDays = 350,
                    PreviousTitleId = "wolf",
                    HiddenRequirements = new[] { "consistent_warrior_achievements", "military_presence" }
                },
                new NicknameDefinition
                {
                    Id = "iron_general",
                    MaleText = "the Iron General",
                    FemaleText = "the Iron General",
                    Tier = "Rare",
                    Priority = 30,
                    ClaimScore = 34f,
                    ReplacementResistance = 12f,
                    RequiredAgeYears = 28,
                    RequiredCareerDays = 520,
                    HiddenRequirements = new[] { "command_success", "generalship" }
                },
                new NicknameDefinition
                {
                    Id = "gilded_general",
                    MaleText = "the Gilded General",
                    FemaleText = "the Gilded General",
                    Tier = "Rare",
                    Priority = 35,
                    ClaimScore = 36f,
                    ReplacementResistance = 14f,
                    RequiredAgeYears = 30,
                    RequiredCareerDays = 700,
                    PreviousTitleId = "iron_general",
                    HiddenRequirements = new[] { "wealth_and_command", "career_crossover" }
                },
                new NicknameDefinition
                {
                    Id = "unbeaten",
                    MaleText = "the Unbeaten",
                    FemaleText = "the Unbeaten",
                    Tier = "Rare",
                    Priority = 25,
                    ClaimScore = 30f,
                    ReplacementResistance = 12f,
                    RequiredAgeYears = 26,
                    RequiredCareerDays = 400,
                    HiddenRequirements = new[] { "few_losses", "military_dominance" }
                },
                new NicknameDefinition
                {
                    Id = "broken_eagle",
                    MaleText = "the Broken Eagle",
                    FemaleText = "the Broken Eagle",
                    Tier = "Rare",
                    Priority = 32,
                    ClaimScore = 31f,
                    ReplacementResistance = 15f,
                    RequiredAgeYears = 32,
                    RequiredCareerDays = 650,
                    HiddenRequirements = new[] { "repeated_defeats", "disgrace" }
                },
                new NicknameDefinition
                {
                    Id = "grey_lion",
                    MaleText = "the Grey Lion",
                    FemaleText = "the Grey Lioness",
                    Tier = "Legendary",
                    Priority = 55,
                    ClaimScore = 42f,
                    ReplacementResistance = 18f,
                    RequiredAgeYears = 45,
                    RequiredCareerDays = 1200,
                    HiddenRequirements = new[] { "late_legacy", "legendary_warrior" }
                }
            };
        }

        public IReadOnlyList<NicknameDefinition> GetAll()
        {
            return _definitions.AsReadOnly();
        }

        public NicknameDefinition GetById(string id)
        {
            return _definitions.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
