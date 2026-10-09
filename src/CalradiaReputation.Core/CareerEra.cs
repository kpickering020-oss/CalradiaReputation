namespace CalradiaReputation
{
    public sealed class CareerEra
    {
        public string Name { get; set; }
        public double StartDay { get; set; }
        public double EndDay { get; set; }
        public ReputationCategory DominantCategory { get; set; }

        public CareerEra()
        {
            Name = string.Empty;
        }
    }
}
