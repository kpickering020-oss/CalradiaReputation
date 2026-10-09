namespace CalradiaReputation
{
    public sealed class CategoryScore
    {
        public float Current { get; set; }
        public float Peak { get; set; }
        public float Recent { get; set; }
        public double LastActivityDay { get; set; }

        public void Add(float amount, double day)
        {
            if (amount > 0f)
            {
                Current = Clamp(Current + amount);
                if (Current > Peak)
                {
                    Peak = Current;
                }

                Recent = Clamp(Recent + amount);
                LastActivityDay = day;
            }
            else if (amount < 0f)
            {
                Current = Clamp(Current + amount);
                Recent = Clamp(Recent + (amount * 0.5f));
                LastActivityDay = day;
            }
        }

        public void DailyTick(double day)
        {
            Recent *= ReputationBalance.DailyRecentDecay;
            if (Recent < 0.05f)
            {
                Recent = 0f;
            }

            if (day - LastActivityDay >= ReputationBalance.InactivityDaysBeforeDecay)
            {
                float floor = Peak * ReputationBalance.PeakFloorRatio;
                if (Current > floor)
                {
                    Current = Clamp(Current * ReputationBalance.DailyInactiveScoreDecay);
                    if (Current < floor)
                    {
                        Current = floor;
                    }
                }
            }
        }

        private static float Clamp(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > ReputationBalance.ScoreCap ? ReputationBalance.ScoreCap : value;
        }
    }
}
