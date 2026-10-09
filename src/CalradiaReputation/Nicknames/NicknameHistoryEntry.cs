using System;
using System.Collections.Generic;

namespace CalradiaReputation
{
    [Serializable]
    public sealed class NicknameClaim
    {
        public string NicknameId { get; set; }
        public float Score { get; set; }
        public float Resistance { get; set; }
        public int CampaignDay { get; set; }
        public List<string> Evidence { get; set; } = new List<string>();

        public bool IsEligibleForReplacement(string currentNicknameId, float threshold)
        {
            if (string.IsNullOrWhiteSpace(currentNicknameId))
            {
                return true;
            }

            return Score >= threshold && !string.Equals(currentNicknameId, NicknameId, StringComparison.Ordinal);
        }
    }
}
