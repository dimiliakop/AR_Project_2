using UnityEngine;
using UnityEngine.UI;

public class LogDisplay : MonoBehaviour
{
    public TMPro.TextMeshProUGUI errorText;
    private string logMessages = "";

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logMessages += logString + "\n";
        errorText.text = logMessages;
    }
}