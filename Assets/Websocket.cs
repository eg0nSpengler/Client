using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Websocket : MonoBehaviour
{
    public class Log : WebSocketBehavior
    {
        protected override void OnOpen ()
        {
            Debug.Log ("Websocket client connected, sending initial log");
            const int maxLogCount = 255;
            bool[] logFilter = new bool[5];
            string searchString = "";
            //Get which log types to filter

            for (var i = 0; i < logFilter.Length; i++)
            {
                if (Context.QueryString[i.ToString ()] != null)
                {
                    logFilter[i] = Context.QueryString[i.ToString ()] == "true" ? true : false;
                } else
                {
                    Debug.Log ("Websocket: Failed to get query string for log type " + i.ToString ());
                }
            }
            if (Context.QueryString["search"] != null)
            {
                searchString = Context.QueryString["search"];
            }
            
            LogContainer initialLogContainer = new LogContainer (new List<LogMessage> ());

            // Filter by log type
            initialLogContainer.logs = logContainer.logs.Where (x => logFilter[(int) x.type] == true).ToList ();
            // Filter by search term
            if (searchString != "")
            {
                initialLogContainer.logs = logContainer.logs.Where (x => x.condition.ToLower ().Contains (searchString.ToLower ())).ToList ();
            }
            // Filter by max log count length
            initialLogContainer.logs = initialLogContainer.logs.Skip (Math.Max (0, initialLogContainer.logs.Count () - maxLogCount)).ToList ();
            Send (JsonUtility.ToJson (initialLogContainer));
        }
    }

    [Serializable]
    public struct StatusMessage
    {
        public long memoryUsage;
        public float deltaTime;
        public StatusMessage (long _memoryUsage, float _deltaTime)
        {
            memoryUsage = _memoryUsage;
            deltaTime = _deltaTime;
        }
    }
    
    static float lastDeltaTime;

    public class Status : WebSocketBehavior
    {
        protected override void OnMessage (MessageEventArgs e) 
        {
            // DONE:
            // memory usage
            // frame time

            // TODO:
            // messages sent
            // connected users

            Send (JsonUtility.ToJson (new StatusMessage (System.GC.GetTotalMemory (false), lastDeltaTime)));
        }
    }

    [Serializable]
    public struct LogMessage 
    {
        public string condition;
        public string stackTrace;
        public LogType type;
        public string time;
        public LogMessage (string _condition, string _stackTrace, LogType _type, DateTime _time)
        {
            condition = _condition;
            stackTrace = _stackTrace;
            type = _type;
            time = _time.ToString ();
        }
    }

    public struct LogContainer
    {
        public List<LogMessage> logs;

        public LogContainer (List<LogMessage> _logs)
        {
            logs = _logs;
        }
    }

    private static LogContainer logContainer;
    private WebSocketServer webSocketServer;

    private void Start ()
    {
        logContainer = new LogContainer (new List<LogMessage> ());
        webSocketServer = new WebSocketServer ("wss://localhost:5335");
        webSocketServer.AddWebSocketService<Log> ("/Log");
        Application.logMessageReceived += LogCallback;
        Application.SetStackTraceLogType (LogType.Error, StackTraceLogType.Full);
        Application.SetStackTraceLogType (LogType.Assert, StackTraceLogType.Full);
        Application.SetStackTraceLogType (LogType.Warning, StackTraceLogType.Full);
        Application.SetStackTraceLogType (LogType.Log, StackTraceLogType.Full);
        Application.SetStackTraceLogType (LogType.Exception, StackTraceLogType.Full);
        webSocketServer.AddWebSocketService<Status> ("/Status");

        webSocketServer.SslConfiguration.ServerCertificate =
            new X509Certificate2 ("cert.pfx", "hunter2");
        webSocketServer.Start ();
        Debug.Log ("Started web socket server");
    }

    private void LogCallback (string condition, string stackTrace, LogType type)
    {
        // Collect logs in log list to be be able to send initial log to new clients
        var message = new LogMessage (condition, stackTrace, type, DateTime.UtcNow);
        logContainer.logs.Add (message);
        webSocketServer.WebSocketServices["/Log"].Sessions.Broadcast (JsonUtility.ToJson (message));
    }

    private void Update ()
    {
        lastDeltaTime = Time.smoothDeltaTime;
    }

    private void Quit()
    {
        Debug.Log ("Exiting web socket server");
        webSocketServer.Stop ();
    }

    private void OnApplicationQuit ()
    {
        Quit();
    }

    private void OnDestroy ()
    {
        Quit();
    }
}