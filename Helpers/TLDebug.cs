using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TLDebug
{
   
    public static void LogRedBold(string message, string prefex = "")
    {
        Debug.Log($"<color=red><b>{prefex}{message}</b></color>");
    }
    public static void LogRed(string message, string prefex = "")
    {
        Debug.Log($"<color=red>{prefex}{message}</color>");
    }
    public static void LogGreen(string message, string prefex = "", int size = 12)
    {
        Debug.Log($"<color=#8ecc79><size={size}><b>{prefex}{message}</b></size></color>");
    }

    public static void LogBlue(string message)
    {
        Debug.Log($"<color=cyan>{message}</color>");
    }
    public static void LogYellow(string message)
    {
        Debug.Log($"<color=yellow>{message}</color>");
    }
    public static void LogBold(string message, string prefex = "")
    {
        Debug.Log($"<b>{prefex}{message}</b>");
    }
    public static void LogSequance(string message, string prefex = "Seq")
    {
        Debug.Log($"<color=#ff8400><size=16>{prefex}:{message}</size></color>");
    }
    public static void LogUnitTest(string message)
    {
        Debug.Log($"<color=#ffe748><size=16> UnitTest : {message}</size></color>");

    }
    public static void UnitTestSuccess(string message)
    {
        LogGreen(message, "UnitTest : ", size: 20);
    }
    public static void UnitTestFail(string message)
    {
        LogRedBold(message, "UnitTest : ");
    }
}
public class DebugChannel
{
    public string ChannelName;
    Queue<DebugMesseg> messages = new Queue<DebugMesseg>();
    public void AddMessage(DebugMesseg message)
    {
        messages.Enqueue(message);
    }
    public void Clear()
    {
        messages.Clear();
    }
    public DebugMesseg NextMessage()
    {
        if (messages.Count == 0)
            return null;
        return messages.Dequeue();
    }
    public bool HasMessages()
    {
        return messages.Count > 0;
    }
}
public class DebugMesseg
{
    [HideInInspector] public string channel;

    [TableColumnWidth(200), ReadOnly]
    public string message;
    [TableColumnWidth(50), ReadOnly]
    public Time time;
}