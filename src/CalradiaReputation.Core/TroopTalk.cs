namespace CalradiaReputation
{
    public static class TroopTalk
    {
        public static string CompanionAnswer(ReputationState state, NicknameRegistry registry, int mercyTrait, int honorTrait)
        {
            if (state == null || registry == null)
            {
                return "The men haven't quite made up their minds about you.";
            }

            NicknameDefinition current = registry.Find(state.CurrentNicknameId);
            if (current == null)
            {
                return "The men haven't quite made up their minds about you.";
            }

            string currentName = current.GetDisplayText(state.IsFemale);
            NicknameDefinition previous = registry.Find(state.PreviousNicknameId);
            ReputationCategory dominant = state.DominantCategory(true);
            bool fallen = (current.Tags & ReputationTag.Fallen) != 0 || (current.Tags & ReputationTag.Mocked) != 0;
            bool crossover = current.Evolution == EvolutionType.CrossReputation;
            bool emerging = state.CurrentDay - state.NicknameEstablishedDay < 10;

            if (fallen && previous != null)
            {
                string oldName = previous.GetDisplayText(state.IsFemale);
                if (mercyTrait > 0)
                {
                    return "They remember when they called you " + oldName + ". After these defeats, that name has changed. Now they say " + currentName + ".";
                }

                return "They remember when they called you " + oldName + ". After these defeats, that name has changed.";
            }

            if (crossover && previous != null)
            {
                return "The men still remember your victories, but these days they speak more of your caravans and your denars. They call you " + currentName + ".";
            }

            if (emerging && current.Rarity <= NicknameRarity.Uncommon)
            {
                return "I've heard a name going around the camp lately. Some of the lads have taken to calling you " + currentName + ".";
            }

            if (honorTrait < 0 && (current.Tags & ReputationTag.Cruel) != 0)
            {
                return "The men keep their voices low, but they call you " + currentName + ".";
            }

            if (dominant == ReputationCategory.Warrior && (current.Tags & ReputationTag.Combat) != 0)
            {
                return "The men have taken to calling you " + currentName + ". They talk of the way you fight.";
            }

            return "The men have taken to calling you " + currentName + ".";
        }

        public static string NpcRemark(ReputationState state, NicknameRegistry registry, ReputationTag tone)
        {
            NicknameDefinition current = registry == null || state == null ? null : registry.Find(state.CurrentNicknameId);
            if (current == null)
            {
                return "I do not know what name the realm has given you.";
            }

            string name = current.GetDisplayText(state.IsFemale);
            NicknameDefinition previous = registry.Find(state.PreviousNicknameId);

            if ((tone & ReputationTag.Mocked) != 0 && previous != null && (previous.Tags & ReputationTag.Mocked) != 0)
            {
                return "I remember when they called you " + previous.GetDisplayText(state.IsFemale) + ". Times have changed.";
            }

            if ((tone & ReputationTag.Feared) != 0)
            {
                return "So you're " + name + " I've heard so much about.";
            }

            if ((tone & ReputationTag.Veteran) != 0 || (tone & ReputationTag.Respected) != 0)
            {
                return name + ". It is an honor to finally meet you.";
            }

            if ((tone & ReputationTag.Wealthy) != 0)
            {
                return "They say the roads grow richer wherever " + name + " passes.";
            }

            return "You are the one they call " + name + ".";
        }

        public static ReputationTag ToneFor(NicknameDefinition nickname)
        {
            if (nickname == null)
            {
                return ReputationTag.None;
            }

            if ((nickname.Tags & ReputationTag.Feared) != 0 || (nickname.Tags & ReputationTag.Cruel) != 0)
            {
                return ReputationTag.Feared;
            }

            if ((nickname.Tags & ReputationTag.Mocked) != 0)
            {
                return ReputationTag.Mocked;
            }

            if ((nickname.Tags & ReputationTag.Wealthy) != 0)
            {
                return ReputationTag.Wealthy;
            }

            if ((nickname.Tags & ReputationTag.Veteran) != 0)
            {
                return ReputationTag.Veteran;
            }

            return ReputationTag.Respected;
        }
    }
}
