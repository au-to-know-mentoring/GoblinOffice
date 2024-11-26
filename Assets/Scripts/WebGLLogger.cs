using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebGLLogger : MonoBehaviour
{
    private static WebGLLogger instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceived += HandleLog;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        Debug.Log($"[{type}] {logString}\n{stackTrace}");
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}