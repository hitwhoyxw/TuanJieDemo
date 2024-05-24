using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDispacth
{
    private Dictionary<string, Action<EventParam>> _eventHandlers = new Dictionary<string, Action<EventParam>>();
    public void AddListener(string type, Action<EventParam> handler)
    {
        if (_eventHandlers.ContainsKey(type))
        {
            var handlerList = _eventHandlers[type].GetInvocationList();
            foreach (var item in handlerList)
            {
                if (item.Equals(handler))
                {
                    CustomLog.Dlog("EventDispacth", $"AddListener type: {type}   already have handler");
                    return;
                }
            }
            _eventHandlers[type] += handler;
        }
        _eventHandlers[type] = handler;
    }
    public void DispatchEvent(string type, EventParam param)
    {
        if (_eventHandlers.ContainsKey(type))
        {
            var ac= _eventHandlers[type];
            if (ac != null)
            {
                ac.Invoke(param);
            }
        }
        else
        {
            CustomLog.Dlog("EventDispacth", $"Dispatch type: {type}   no handler");
        }
    }
    public void RemoveListener(string type, Action<EventParam> handler)
    {
        if (_eventHandlers.ContainsKey(type))
        {
            _eventHandlers[type] -= handler;
            if (_eventHandlers[type] == null)
            {
                _eventHandlers.Remove(type);
            }
        }
    }
    public void RemoveAllListener()
    {
        _eventHandlers.Clear();
    }
}
public class EventParam
{
    public object sender;
    public object param;
    public string type;
}
