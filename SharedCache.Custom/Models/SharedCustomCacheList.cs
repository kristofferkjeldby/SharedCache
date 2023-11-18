namespace SharedCache.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a shared custom cache list. This list is immutable
    /// </summary>
    public class SharedCustomCacheList<TValue> : IReadOnlyCollection<TValue>
    {        
        private IList<TValue> innerList;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomCacheList{TValue}"/> class.
        /// </summary>
        public SharedCustomCacheList(IList<TValue> innerList)
        {
            this.innerList = innerList ?? new List<TValue>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomCacheList{TValue}"/> class.
        /// This constructor should not be used
        /// </summary>
        [Obsolete]
        public SharedCustomCacheList() : base()
        {
            throw new Exception();
        }

        /// <summary>
        /// Serialize the list.
        /// </summary>
        public List<string> Serialize(Func<TValue, string> valueSerialize)
        {
            return innerList.Select(value => valueSerialize(value)).ToList();
        }

        /// <summary>
        /// Gets or sets a list index.
        /// </summary>
        public TValue this[int index]
        {
            get { return innerList[index]; }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count => innerList.Count;

        /// <summary>
        /// Converts to list.
        /// </summary>
        public IReadOnlyCollection<TValue> ToReadOnlyCollection()
        {
            return innerList.ToList();
        }

        /// <inheritdoc/>
        public IEnumerator<TValue> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }
    }
}