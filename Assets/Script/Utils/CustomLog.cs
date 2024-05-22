using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
public class CustomLog
{

    public static void Dlog(string tag, string msg)
    {
        Debug.Log(tag + ":" + msg);
    }
    public static void Ilog(string tag, string msg)
    {

    }
    public static void Wlog(string tag, string msg)
    {

    }
    public static void Elog(string tag, string msg)
    {

    }
    public static string LogFormat(string tag, string msg, LogLevel logLevel)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(logLevel);
        sb.Append("]");
        sb.Append("[");
        sb.Append(tag);
        sb.Append("]");
        sb.Append(" ");
        sb.Append(msg);
        return sb.ToString();
    }
}
