﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitFaster.Caching.Atomic
{
    public sealed class AtomicFactoryAsyncCache<K, V> : IAsyncCache<K, V>
    {
        private readonly ICache<K, AsyncAtomicFactory<K, V>> cache;
        private readonly EventProxy eventProxy;

        public AtomicFactoryAsyncCache(ICache<K, AsyncAtomicFactory<K, V>> cache)
        {
            if (cache == null)
            {
                throw new ArgumentNullException(nameof(cache));
            }

            this.cache = cache;
            this.eventProxy = new EventProxy(cache.Events);
        }

        public int Capacity => cache.Capacity;

        public int Count => cache.Count;

        public ICacheMetrics Metrics => cache.Metrics;

        public ICacheEvents<K, V> Events => this.eventProxy;

        public ICollection<K> Keys => this.cache.Keys;

        public void AddOrUpdate(K key, V value)
        {
            cache.AddOrUpdate(key, new AsyncAtomicFactory<K, V>(value));
        }

        public void Clear()
        {
            cache.Clear();
        }

        public ValueTask<V> GetOrAddAsync(K key, Func<K, Task<V>> valueFactory)
        {
            var synchronized = cache.GetOrAdd(key, _ => new AsyncAtomicFactory<K, V>());
            return synchronized.GetValueAsync(key, valueFactory);
        }

        public void Trim(int itemCount)
        {
            cache.Trim(itemCount);
        }

        public bool TryGet(K key, out V value)
        {
            AsyncAtomicFactory<K, V> output;
            var ret = cache.TryGet(key, out output);

            if (ret && output.IsValueCreated)
            {
                value = output.ValueIfCreated;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryRemove(K key)
        {
            return cache.TryRemove(key);
        }

        public bool TryUpdate(K key, V value)
        {
            return cache.TryUpdate(key, new AsyncAtomicFactory<K, V>(value));
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            foreach (var kvp in this.cache)
            {
                yield return new KeyValuePair<K, V>(kvp.Key, kvp.Value.ValueIfCreated);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((AtomicFactoryAsyncCache<K, V>)this).GetEnumerator();
        }

        private class EventProxy : CacheEventProxyBase<K, AsyncAtomicFactory<K, V>, V>
        {
            public EventProxy(ICacheEvents<K, AsyncAtomicFactory<K, V>> inner)
                : base(inner)
            {
            }

            protected override ItemRemovedEventArgs<K, V> TranslateOnRemoved(ItemRemovedEventArgs<K, AsyncAtomicFactory<K, V>> inner)
            {
                return new ItemRemovedEventArgs<K, V>(inner.Key, inner.Value.ValueIfCreated, inner.Reason);
            }
        }
    }
}