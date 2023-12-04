using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static readonly object queueLock = new object();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        lock (queueLock)
        {
            while (actions.Count > 0)
            {
                actions.Dequeue().Invoke();
            }
        }
    }

    public static void Enqueue(Action action)
    {
        lock (queueLock)
        {
            actions.Enqueue(action);
        }
    }
}
