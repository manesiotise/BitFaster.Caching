﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitFaster.Caching.Lru
{
    public sealed class ConcurrentTLru<K, V> : TemplateConcurrentLru<K, V, TimeStampedLruItem<K, V>, TLruDateTimePolicy<K, V>, HitCounter>
    {
        public ConcurrentTLru(int capacity)
            : base(Defaults.ConcurrencyLevel, capacity, EqualityComparer<K>.Default, new TLruDateTimePolicy<K, V>(), new HitCounter())
        { 
        }

        public ConcurrentTLru(int concurrencyLevel, int capacity, IEqualityComparer<K> comparer, TimeSpan timeToLive)
            : base(concurrencyLevel, capacity, comparer, new TLruDateTimePolicy<K, V>(timeToLive), new HitCounter())
        {
        }

        public double HitRatio => this.hitCounter.HitRatio;
    }
}