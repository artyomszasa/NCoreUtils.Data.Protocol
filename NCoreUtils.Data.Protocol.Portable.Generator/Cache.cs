using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class Cache<TKey, TValue>
{
    private class Entry
    {
        public DateTimeOffset Expiry { get; set; }

        public TValue Value { get; }

        public Entry(TValue value, DateTimeOffset expiry)
        {
            Expiry = expiry;
            Value = value;
        }
    }

    private static TimeSpan EntryDuration { get; } = TimeSpan.FromMinutes(5);

    private SpinLock _sync;

    private ref SpinLock Sync => ref _sync;

    private Dictionary<TKey, Entry> Data { get; }

    private int Counter { get; set; }

    public Cache(IEqualityComparer<TKey> keyEqualityComparer)
    {
        Data = new(keyEqualityComparer);
    }

    private void CleanUp(DateTimeOffset now)
    {
        List<TKey>? toRemove = default;
        foreach (var kv in Data)
        {
            if (kv.Value.Expiry < now)
            {
                (toRemove ?? new()).Add(kv.Key);
            }
        }
        if (toRemove is not null)
        {
            foreach (var key in toRemove)
            {
                Data.Remove(key);
            }
        }
    }

    private void IncCounter(DateTimeOffset now)
    {
        if (++Counter > 50)
        {
            CleanUp(now);
            Counter = 0;
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
    {
        var lockTaken = false;
        Sync.Enter(ref lockTaken);
        try
        {
            var now = DateTimeOffset.Now;
            IncCounter(now);
            if (Data.TryGetValue(key, out var entry) && entry.Expiry > now)
            {
                entry.Expiry = now + EntryDuration;
                return entry.Value;
            }
            var value = factory(key);
            Data[key] = new Entry(value, now + EntryDuration);
            return value;
        }
        finally
        {
            if (lockTaken)
            {
                Sync.Exit();
            }
        }
    }
}