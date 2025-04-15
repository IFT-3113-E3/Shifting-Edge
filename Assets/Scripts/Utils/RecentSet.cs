using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Utils
{
    public class RecentSet<T> : IEnumerable<T>
    {
        private readonly LinkedList<T> _list = new();
        private readonly HashSet<T> _set = new();
        private int _consecutiveCount = 0;
        
        public int ConsecutiveCount => _consecutiveCount;

        public void Add(T item)
        {
            var lastItem = _list.First;
            if (lastItem != null)
            {
                if (lastItem.Value.Equals(item))
                    _consecutiveCount++;
                else
                    _consecutiveCount = 0;
            }
            if (_set.Contains(item))
            {
                _list.Remove(item);
            }
            else
            {
                _set.Add(item);
            }

            _list.AddFirst(item);
            

        }
        
        public void Remove(T item)
        {
            var lastItem = _list.First;
            if (lastItem != null && lastItem.Value.Equals(item))
            {
                _consecutiveCount = 0;
            }
            if (_set.Remove(item))
            {
                _list.Remove(item);
            }
        }
        
        public void Clear()
        {
            _list.Clear();
            _set.Clear();
            _consecutiveCount = 0;
        }
        
        public T GetLast()
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("The set is empty.");
            return _list.First.Value;
        }
        
        public T GetFirst()
        {
            if (_list.Count == 0)
                throw new InvalidOperationException("The set is empty.");
            return _list.Last.Value;
        }
        
        public bool Contains(T item) => _set.Contains(item);
        
        public bool IsLast(T item)
        {
            if (_list.Count == 0)
                return false;
            return _list.First.Value.Equals(item);
        }
        
        public bool IsFirst(T item)
        {
            if (_list.Count == 0)
                return false;
            return _list.Last.Value.Equals(item);
        }
        
        public int Count => _set.Count;
        
        public IReadOnlyList<T> AsList() => _list.ToList();

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}