using System.Collections.Generic;

namespace CalradiaReputation
{
    public sealed class CareerHistory
    {
        public List<CareerEra> Eras { get; } = new List<CareerEra>();
        public string DominantEraName { get; set; } = "Unknown";
        public ReputationCategory LifetimeDominant { get; set; } = ReputationCategory.Warrior;
        public ReputationCategory RecentDominant { get; set; } = ReputationCategory.Warrior;

        public void RecordEra(string name, double day, ReputationCategory category)
        {
            if (Eras.Count > 0)
            {
                CareerEra last = Eras[Eras.Count - 1];
                if (last.Name == name)
                {
                    last.EndDay = day;
                    return;
                }

                last.EndDay = day;
            }

            Eras.Add(new CareerEra
            {
                Name = name,
                StartDay = day,
                EndDay = day,
                DominantCategory = category
            });

            if (Eras.Count > 16)
            {
                Eras.RemoveAt(0);
            }

            DominantEraName = name;
        }
    }
}
