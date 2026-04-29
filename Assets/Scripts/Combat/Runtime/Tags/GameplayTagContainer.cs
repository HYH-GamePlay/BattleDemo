using System.Collections.Generic;

namespace Combat.Runtime.Tags
{
    public sealed class GameplayTagContainer
    {
        private readonly HashSet<string> _tags = new();

        public IReadOnlyCollection<string> Tags => _tags;

        public bool Has(string tag) => !string.IsNullOrEmpty(tag) && _tags.Contains(tag);

        public bool HasAny(IEnumerable<string> tags)
        {
            if (tags == null) return false;
            foreach (var tag in tags)
                if (Has(tag)) return true;
            return false;
        }

        public bool HasAll(IEnumerable<string> tags)
        {
            if (tags == null) return true;
            foreach (var tag in tags)
                if (!Has(tag)) return false;
            return true;
        }

        public bool Add(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return false;
            return _tags.Add(tag);
        }

        public bool Remove(string tag)
            => !string.IsNullOrWhiteSpace(tag) && _tags.Remove(tag);

        public void Clear() => _tags.Clear();
    }
}
