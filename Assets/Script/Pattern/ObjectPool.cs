using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public enum PoolType
{
    Fixed,
    Expandable
}
public enum PoolStatus
{
    Idle,
    Busy,
}
/// <summary>
/// ��ʼ�����������̰߳�ȫ�Ķ����,������
/// �ο�arraypoolȥʵ��
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectPool<T>
{
    private Stack<T> _pool;
    private int _maxCount;
    private int _totalCount;
    private readonly Func<T> _factoryFunc;
    private Dictionary<T, bool> _usedDic;
    private PoolType _poolType;
    private readonly float expandStep = 1.5f;
    public ObjectPool(int maxCount, Func<T> func, bool lazy, PoolType poolType = PoolType.Expandable)
    {
        this._factoryFunc = func;
        _maxCount = maxCount;
        _poolType = poolType;
        if (lazy)
        {
            return;
        }
        _pool = new Stack<T>(maxCount);
        _usedDic = new Dictionary<T, bool>(_maxCount);
    }

    /// <summary>
    /// ���ݹ���Ķ�������ͷ��ض����/�½�����
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        if (_pool == null)
        {
            _pool = new Stack<T>(_maxCount);
            _usedDic = new Dictionary<T, bool>(_maxCount);
            foreach (var poolItem in _pool)
            {
                _usedDic.Add(poolItem, false);
            }
        }
        var status = EnsureCapacity();
        if (status == PoolStatus.Idle)
        {
            var item = _pool.Pop();
            _usedDic[item] = true;
            return item;
        }
        return _factoryFunc();
    }
    public void Recycle(T item)
    {
        if (_usedDic.ContainsKey(item))
        {
            _usedDic[item] = false;
            _pool.Push(item);
        }
        else
        {
            CustomLog.Elog("ObjectPool", "Recycle item not in pool");
        }
    }
    private PoolStatus EnsureCapacity()
    {
        if (_pool.Count > 0)
        {
            return PoolStatus.Idle;
        }
        if (_poolType == PoolType.Fixed)
        {
            return PoolStatus.Busy;
        }
        _maxCount = (int)(_maxCount * expandStep);
        var tempStack = new Stack<T>(_maxCount);
        foreach (var item in _pool)
        {
            tempStack.Push(item);
        }
        _pool = tempStack;
        var tempDic = new Dictionary<T, bool>(_maxCount);
        foreach (var item in _pool)
        {
            tempDic.Add(item, false);
        }
        foreach (var item in _usedDic)
        {
            tempDic[item.Key] = item.Value;
        }
        _usedDic = tempDic;
        return PoolStatus.Idle;
    }


}
public class ConcurrentObjectPool<T> where T : class
{
    private Stack<T> _pool;
    private object _locker = new object();
    private int _maxCount;
    private readonly Func<T> _factoryFunc;
    private Dictionary<T, bool> _usedDic;
    private PoolType _poolType;
    private readonly float expandStep = 1.5f;
    public ConcurrentObjectPool(int maxCount, Func<T> func, bool lazy, PoolType poolType = PoolType.Expandable)
    {
        this._maxCount = maxCount;
        this._factoryFunc = func;
        _pool = new Stack<T>(maxCount);
        _usedDic = new Dictionary<T, bool>(_maxCount);
        foreach (var poolItem in _pool)
        {
            _usedDic.Add(poolItem, false);
        }
    }
    public T Get()
    {
        lock (_locker)
        {
            var status = EnsureCapacity();
            if (status == PoolStatus.Idle)
            {
                var item = _pool.Pop();
                _usedDic[item] = true;
                return item;
            }
            return _factoryFunc();
        }
    }
    public void Recycle(T item)
    {
        lock (_locker)
        {
            if (_usedDic.ContainsKey(item))
            {
                _pool.Push(item);
                _usedDic[item] = false;
            }
            else
            {
                CustomLog.Elog("ConcurrentObjectPool", "Recycle item not in pool");
            }
        }
    }
    /// <summary>
    /// ��ն����,�����IDisposable�Ķ�������Dispose
    /// </summary>
    public void Clear()
    {
        lock (_locker)
        {
            _pool.Clear();
            foreach (var item in _usedDic)
            {
                if(item.Key is IDisposable disposableT)
                {
                    disposableT.Dispose();
                }
            }
            _usedDic.Clear();
        }
    }
    private PoolStatus EnsureCapacity()
    {
        if (_pool.Count > 0)
        {
            return PoolStatus.Idle;
        }
        if (_poolType == PoolType.Fixed)
        {
            return PoolStatus.Busy;
        }
        _maxCount = (int)(_maxCount * expandStep);
        var tempStack = new Stack<T>(_maxCount);
        foreach (var item in _pool)
        {
            tempStack.Push(item);
        }
        _pool = tempStack;
        var tempDic = new Dictionary<T, bool>(_maxCount);
        foreach (var item in _pool)
        {
            tempDic.Add(item, false);
        }
        foreach (var item in _usedDic)
        {
            tempDic[item.Key] = item.Value;
        }
        _usedDic = tempDic;
        return PoolStatus.Idle;
    }

}