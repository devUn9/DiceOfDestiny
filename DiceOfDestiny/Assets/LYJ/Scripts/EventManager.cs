using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : Singletone<EventManager>
{
    // private static EventManager instance;
    // public static EventManager Instance
    // {
    //     get
    //     {
    //         if (instance == null)
    //         {
    //             GameObject obj = new GameObject("EventManager");
    //             instance = obj.AddComponent<EventManager>();
    //             DontDestroyOnLoad(obj);
    //         }
    //         return instance;
    //     }
    // }

    // void Awake()
    // {
    //     if (instance != null && instance != this)
    //     {
    //         Destroy(gameObject);
    //     }
    //     else
    //     {
    //         instance = this;
    //     }
    // }

    private Dictionary<string, Action<object>> eventDic = new();

    public void AddListener(string eventName, Action<object> newListener)
    {
        if (eventDic.TryGetValue(eventName, out Action<object> currentListeners))
        {
            currentListeners += newListener;
            eventDic[eventName] = currentListeners;
            Debug.Log($"<color=#30ffae>Added Listener ({currentListeners}) on {eventName}</color>");
        }
        else
        {
            eventDic.Add(eventName, newListener);
            Debug.Log($"<color=#30ffae>Added Listener ({currentListeners}) on New event ({eventName})</color>");
        }
    }

    public void RemoveListener(string eventName, Action<object> targetListener)
    {
        if (eventDic.TryGetValue(eventName, out Action<object> currentListeners))
        {
            currentListeners -= targetListener;
            eventDic[eventName] = currentListeners;
            Debug.Log($"<color=#30ffae>Removed Listener ({currentListeners}) on {eventName}</color>");
        }
        else
        {
            Debug.Log($"<color=#30ffae>Can not Remove {targetListener?.Method?.Name} From {eventName}</color>");
        }
    }

    public void TriggerEvent(string eventName, object data = null)
    {
        if (eventDic.TryGetValue(eventName, out Action<object> currentListeners))
        {
            currentListeners?.Invoke(data);
            Debug.Log($"<color=#30ffae>{currentListeners} Triggered! </color>");
        }
    }
}