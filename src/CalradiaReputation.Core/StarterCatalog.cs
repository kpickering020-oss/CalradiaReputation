namespace CalradiaReputation
{
    internal static class StarterCatalog
    {
        public static NicknameDefinition[] Create()
        {
            return new[]
            {
                Bold(),
                IronGeneral(),
                Unbroken(),
                CityTaker(),
                BlackTorch(),
                Hare(),
                BrokenEagle(),
                Lionheart(),
                GildedGeneral(),
                GreyLion()
            };
        }

        private static NicknameDefinition Bold()
        {
            return new NicknameDefinition
            {
                Id = "the_bold",
                MaleText = "the Bold",
                FemaleText = "the Bold",
                Rarity = NicknameRarity.Common,
                Tags = ReputationTag.Combat | ReputationTag.Respected,
                Priority = 10,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Warrior,
                    MinFocusScore = ReputationBalance.CommonMinScore,
                    MinPersonalHits = 6
                }
            };
        }

        private static NicknameDefinition IronGeneral()
        {
            return new NicknameDefinition
            {
                Id = "the_iron_general",
                MaleText = "the Iron General",
                FemaleText = "the Iron General",
                Rarity = NicknameRarity.Rare,
                Tags = ReputationTag.Command | ReputationTag.Military | ReputationTag.Respected,
                Priority = 80,
                ExtraResistance = 4f,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Generalship,
                    MinFocusScore = ReputationBalance.RareMinScore,
                    MustOutrankCategory = ReputationCategory.Warrior,
                    MinCommandVictories = 8,
                    MinMajorVictories = 3,
                    MinOutnumberedVictories = 2,
                    MaxMajorDefeats = 2,
                    MaxTrivialVictorySharePercent = 40
                }
            };
        }

        private static NicknameDefinition Unbroken()
        {
            return new NicknameDefinition
            {
                Id = "the_unbroken",
                MaleText = "the Unbroken",
                FemaleText = "the Unbroken",
                Rarity = NicknameRarity.Uncommon,
                Tags = ReputationTag.Military | ReputationTag.Command | ReputationTag.Respected,
                Priority = 40,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Generalship,
                    MinFocusScore = ReputationBalance.UncommonMinScore,
                    MinCommandVictories = 6,
                    MaxMajorDefeats = 0
                }
            };
        }

        private static NicknameDefinition CityTaker()
        {
            return new NicknameDefinition
            {
                Id = "city_taker",
                MaleText = "City-Taker",
                FemaleText = "City-Taker",
                Rarity = NicknameRarity.Uncommon,
                Tags = ReputationTag.Military | ReputationTag.Feared,
                Priority = 45,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Conquest,
                    MinFocusScore = ReputationBalance.UncommonMinScore,
                    MinTownsCaptured = 3
                }
            };
        }

        private static NicknameDefinition BlackTorch()
        {
            return new NicknameDefinition
            {
                Id = "the_black_torch",
                MaleText = "the Black Torch",
                FemaleText = "the Black Torch",
                Rarity = NicknameRarity.Rare,
                Tags = ReputationTag.Raider | ReputationTag.Feared | ReputationTag.Cruel,
                Priority = 70,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Raider,
                    MinFocusScore = ReputationBalance.RareMinScore,
                    MinVillagesRaided = 5
                }
            };
        }

        private static NicknameDefinition Hare()
        {
            return new NicknameDefinition
            {
                Id = "the_hare",
                MaleText = "the Hare",
                FemaleText = "the Hare",
                Rarity = NicknameRarity.Common,
                Tags = ReputationTag.Mocked | ReputationTag.Fallen,
                Priority = 15,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Generalship,
                    MinFocusScore = 0f,
                    MinTimesCaptured = 2,
                    MinConsecutiveMajorDefeats = 2
                }
            };
        }

        private static NicknameDefinition BrokenEagle()
        {
            return new NicknameDefinition
            {
                Id = "the_broken_eagle",
                MaleText = "the Broken Eagle",
                FemaleText = "the Broken Eagle",
                Rarity = NicknameRarity.Rare,
                Tags = ReputationTag.Fallen | ReputationTag.Military | ReputationTag.Mocked,
                Priority = 75,
                Evolution = EvolutionType.Reversal,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Generalship,
                    MinFocusScore = 0f,
                    MinPeakFocus = ReputationBalance.RareMinScore,
                    MinMajorDefeats = 4,
                    MinConsecutiveMajorDefeats = 3,
                    EvolvesFromIds = new[] { "the_iron_general", "the_unbroken" }
                }
            };
        }

        private static NicknameDefinition Lionheart()
        {
            return new NicknameDefinition
            {
                Id = "the_lionheart",
                MaleText = "the Lionheart",
                FemaleText = "the Lionheart",
                Rarity = NicknameRarity.Rare,
                Tags = ReputationTag.Redeemed | ReputationTag.Military | ReputationTag.Respected,
                Priority = 85,
                Evolution = EvolutionType.Redemption,
                ExtraResistance = 6f,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Generalship,
                    MinFocusScore = ReputationBalance.RareMinScore,
                    MinMajorVictories = 3,
                    RequiredPreviousIds = new[] { "the_hare" }
                }
            };
        }

        private static NicknameDefinition GildedGeneral()
        {
            return new NicknameDefinition
            {
                Id = "the_gilded_general",
                MaleText = "the Gilded General",
                FemaleText = "the Gilded General",
                Rarity = NicknameRarity.Rare,
                Tags = ReputationTag.Wealthy | ReputationTag.Merchant | ReputationTag.Military | ReputationTag.Command,
                Priority = 78,
                Evolution = EvolutionType.CrossReputation,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Wealth,
                    MinFocusScore = ReputationBalance.RareMinScore,
                    MinPeakFocus = 0f,
                    MinGold = 200000,
                    MinDaysSinceLastBattle = 30,
                    EvolvesFromIds = new[] { "the_iron_general" }
                }
            };
        }

        private static NicknameDefinition GreyLion()
        {
            return new NicknameDefinition
            {
                Id = "the_grey_lion",
                MaleText = "the Grey Lion",
                FemaleText = "the Grey Lioness",
                Rarity = NicknameRarity.Legendary,
                Tags = ReputationTag.Veteran | ReputationTag.Combat | ReputationTag.Respected,
                Priority = 95,
                ExtraResistance = 8f,
                Requirements = new NicknameRequirements
                {
                    FocusCategory = ReputationCategory.Warrior,
                    MinFocusScore = ReputationBalance.RareMinScore,
                    MinPeakFocus = ReputationBalance.LegendaryMinScore,
                    MinAge = 48f,
                    MinPersonalHits = 40,
                    MinMajorVictories = 2,
                    RequiredLegacyIds = new[] { "legendary_warrior" }
                }
            };
        }
    }
}
