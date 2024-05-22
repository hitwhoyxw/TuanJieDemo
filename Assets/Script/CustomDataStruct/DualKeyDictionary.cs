using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/// <summary>
/// 双键字典,使用两个键来索引一个值,2个键组合成哈希码,构成一个键
/// </summary>
/// <typeparam name="TKey1"></typeparam>
/// <typeparam name="TKey2"></typeparam>
/// <typeparam name="TValue"></typeparam>
class DualKeyConcurrentDictionary<TKey1, TKey2, TValue>
{
    private readonly ConcurrentDictionary<DualKey<TKey1, TKey2>, TValue> dictionary;

    public DualKeyConcurrentDictionary()
    {
        dictionary = new ConcurrentDictionary<DualKey<TKey1, TKey2>, TValue>();
    }

    public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
    {
        var compositeKey = new DualKey<TKey1, TKey2>(key1, key2);
        return dictionary.TryGetValue(compositeKey, out value);
    }

    public bool TryAdd(TKey1 key1, TKey2 key2, TValue value)
    {
        var compositeKey = new DualKey<TKey1, TKey2>(key1, key2);
        return dictionary.TryAdd(compositeKey, value);
    }

    public bool TryRemove(TKey1 key1, TKey2 key2, out TValue value)
    {
        var compositeKey = new DualKey<TKey1, TKey2>(key1, key2);
        return dictionary.TryRemove(compositeKey, out value);
    }

    public bool ContainsKey(TKey1 key1, TKey2 key2)
    {
        var compositeKey = new DualKey<TKey1, TKey2>(key1, key2);
        return dictionary.ContainsKey(compositeKey);
    }
}

class DualKey<TKey1, TKey2>
{
    public TKey1 Key1 { get; }
    public TKey2 Key2 { get; }

    public DualKey(TKey1 key1, TKey2 key2)
    {
        Key1 = key1;
        Key2 = key2;
    }

    public override int GetHashCode()
    {
        int hash1 = Key1.GetHashCode();
        int hash2 = Key2.GetHashCode();
        return CombineHashCodes(hash1, hash2);
    }

    public override bool Equals(object obj)
    {
        if (obj is DualKey<TKey1, TKey2> otherKey)
        {
            return Key1.Equals(otherKey.Key1) && Key2.Equals(otherKey.Key2);
        }
        return false;
    }

    private int CombineHashCodes(int hash1, int hash2)
    {
        return ((hash1 << 5) + hash1) ^ hash2;
    }
}