using System;
using System.Collections;
using System.Collections.Generic;

namespace Battle.CombatInfo.Tag
{
    public sealed class TagSet : IReadOnlyCollection<TagId>
    {
        private readonly HashSet<TagId> _tags = new HashSet<TagId>();

        public int Count => _tags.Count;

        public bool Add(TagId tag)
        {
            if (!tag.IsValid)
            {
                throw new ArgumentException("TagId must be valid.", nameof(tag));
            }

            return _tags.Add(tag);
        }

        public bool Remove(TagId tag)
        {
            return _tags.Remove(tag);
        }

        public bool Contains(TagId tag)
        {
            return _tags.Contains(tag);
        }

        public void Clear()
        {
            _tags.Clear();
        }

        public bool ContainsAll(TagSet tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return true;
            }

            foreach (var tag in tags._tags)
            {
                if (!_tags.Contains(tag))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ContainsAny(TagSet tags)
        {
            if (tags == null || tags.Count == 0)
            {
                return false;
            }

            foreach (var tag in tags._tags)
            {
                if (_tags.Contains(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyFrom(TagSet tags)
        {
            _tags.Clear();
            if (tags == null)
            {
                return;
            }

            foreach (var tag in tags._tags)
            {
                _tags.Add(tag);
            }
        }

        public IEnumerator<TagId> GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
