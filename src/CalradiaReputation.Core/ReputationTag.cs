using System;

namespace CalradiaReputation
{
    [Flags]
    public enum ReputationTag
    {
        None = 0,
        Feared = 1 << 0,
        Respected = 1 << 1,
        Mocked = 1 << 2,
        Noble = 1 << 3,
        Wealthy = 1 << 4,
        Military = 1 << 5,
        Cruel = 1 << 6,
        Merciful = 1 << 7,
        Veteran = 1 << 8,
        Outlaw = 1 << 9,
        Merchant = 1 << 10,
        Champion = 1 << 11,
        Fallen = 1 << 12,
        Redeemed = 1 << 13,
        Ruler = 1 << 14,
        Traitor = 1 << 15,
        Loyal = 1 << 16,
        Raider = 1 << 17,
        Combat = 1 << 18,
        Command = 1 << 19
    }
}
