namespace SharedCache.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a shared custom cache dictionary. Supports insert, update and removal of objects
    /// </summary>
    public class SharedCustomCacheDictionary<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
    {        
        private IDictionary<TKey, TValue> innerDictionary;
        private Action<TKey, TValue> addElementAction;
        private Action<TKey> removeElementAction;
        private Action clearAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomCacheDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="innerDictionary">The inner dictionary.</param>
        /// <param name="addElementAction">The add element action.</param>
        /// <param name="removeElementAction">The remove element action.</param>
        /// <param name="clearAction">The clear action.</param>
        public SharedCustomCacheDictionary(IDictionary<TKey, TValue> innerDictionary, Action<TKey, TValue> addElementAction, Action<TKey> removeElementAction, Action clearAction)
        {
            this.innerDictionary = innerDictionary;
            this.addElementAction = addElementAction;
            this.removeElementAction = removeElementAction;
            this.clearAction = clearAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomCacheDictionary{TKey, TValue}"/> class.
        /// </summary>
        public SharedCustomCacheDictionary(Action<TKey, TValue> addElementAction, Action<TKey> removeElementAction)
        {
            this.innerDictionary = new Dictionary<TKey, TValue>();
            this.addElementAction = addElementAction;
            this.removeElementAction = removeElementAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SharedCustomCacheDictionary{TKey, TValue}"/> class.
        /// This constructor should not be used
        /// </summary>
        [Obsolete]
        public SharedCustomCacheDictionary() : base()
        {
            throw new Exception();
        }

        /// <summary>
        /// Adds the specified key (or overwrites it if it exists).
        /// </summary>
        public void AddOrUpdate(TKey dictionaryKey, TValue dictionaryValue)
        {
            if (addElementAction != null) addElementAction(dictionaryKey, dictionaryValue);
            innerDictionary[dictionaryKey] = dictionaryValue;
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        public bool Remove(TKey dictionaryKey)
        {
            if (removeElementAction != null) removeElementAction(dictionaryKey);
            return innerDictionary.Remove(dictionaryKey);
        }

        /// <summary>
        /// Determines whether the dictionary contains a key.
        /// </summary>
        public bool ContainsKey(TKey dictionaryKey)
        {
            return innerDictionary.ContainsKey(dictionaryKey);
        }

        /// <summary>
        /// Serialize the dictionary.
        /// </summary>
        public IDictionary<string, string> Serialize(Func<TKey, string> keySerialize, Func<TValue, string> valueSerialize)
        {
            return innerDictionary.ToDictionary(kv => keySerialize(kv.Key), kv => valueSerialize(kv.Value));
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (ContainsKey(item.Key))
                throw new Exception("Key already exists");
            AddOrUpdate(item.Key, item.Value);
        }

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            this.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (clearAction != null) clearAction();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return innerDictionary.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            innerDictionary.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return innerDictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerDictionary.GetEnumerator();
        }

        /// <inheritdoc/>
        public int Count => innerDictionary.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets or sets a dictionary key.
        /// </summary>
        public TValue this[TKey dictionaryKey]
        {
            get { return innerDictionary[dictionaryKey]; }
            set { AddOrUpdate(dictionaryKey, value); }
        }
    }
}