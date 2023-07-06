using UnityEngine;
using System.Collections.Concurrent;

public class Logger
{
    // singleton instance
    private static readonly Logger _instance = new Logger();
    public Logger instance
    {
        get
        {
            return _instance;
        }
    }

    public static int bufferSize = 15;
    private static ConcurrentQueue<string> buffer = new ConcurrentQueue<string>();


    public static bool Log(string log, bool canDrop = true)
    {
        Debug.Log(log);

        if (buffer.Count > bufferSize && canDrop) return false;
        buffer.Enqueue(log);

        return true;
    }

    public static bool TryGetLog(out string log)
    {
        return buffer.TryDequeue(out log);
    }

}
