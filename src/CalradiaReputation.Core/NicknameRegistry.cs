using System.Collections.Generic;

namespace CalradiaReputation
{
    public sealed class NicknameRegistry
    {
        private readonly List<NicknameDefinition> _nicknames = new List<NicknameDefinition>();
        private readonly Dictionary<string, NicknameDefinition> _byId = new Dictionary<string, NicknameDefinition>();

        public NicknameDefinition[] All => _nicknames.ToArray();

        public int Count => _nicknames.Count;

        public void Add(NicknameDefinition definition)
        {
            _nicknames.Add(definition);
            _byId[definition.Id] = definition;
        }

        public NicknameDefinition? Find(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            NicknameDefinition definition;
            return _byId.TryGetValue(id, out definition) ? definition : null;
        }

        public static NicknameRegistry CreateStarter()
        {
            NicknameRegistry registry = new NicknameRegistry();
            foreach (NicknameDefinition definition in StarterCatalog.Create())
            {
                registry.Add(definition);
            }

            return registry;
        }
    }
}
