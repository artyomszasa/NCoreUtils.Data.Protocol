using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Internal;

public class LambdaArgumentLookup
{
    private sealed class Lookup
    {
        private Dictionary<string, UniqueString> Data { get; }

        private Lookup? Parent { get; }

        public Lookup(Dictionary<string, UniqueString> data, Lookup? parent)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Parent = parent;
        }

        public Lookup() : this(new(), default) { }

        public void Add(string key, UniqueString value)
            => Data.Add(key, value);

        public bool SelfContainsKey(string key)
            => Data.ContainsKey(key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out UniqueString value)
        {
            if (Data.TryGetValue(key, out value))
            {
                return true;
            }
            if (Parent is not null)
            {
                return Parent.TryGetValue(key, out value);
            }
            value = default;
            return false;
        }

        public bool Remove(string key)
            => Data.Remove(key) || (Parent is not null && Parent.Remove(key));
    }

    private Lookup Entry { get; set; } = new Lookup();

    public void Add(string key, UniqueString value)
    {
        if (Entry.SelfContainsKey(key))
        {
            Entry = new Lookup(new(), Entry);
        }
        Entry.Add(key, value);
    }

    public bool Remove(string key)
        => Entry.Remove(key);

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out UniqueString value)
        => Entry.TryGetValue(key, out value);
}