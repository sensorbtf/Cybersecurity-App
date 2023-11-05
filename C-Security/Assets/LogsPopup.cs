using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class LogsPopup : MonoBehaviour
{
    [SerializeField] private GameObject _singleLog;
    [SerializeField] private Button _exitButton;
    [SerializeField] private RectTransform _content;
    [SerializeField] private List<GameObject> _logsList;

    internal void ShowPopup()
    {
        gameObject.SetActive(true);

        var logs = DeserializeLogData();

        foreach (var log in logs)
        {
            var newLogGo = Instantiate(_singleLog, _content);
            var newLogRefs = newLogGo.GetComponent<SingleLogPrefabRefs>();

            newLogRefs.Username.text = log.UserName;
            newLogRefs.DateTime.text = log.DateTime.ToString();
            newLogRefs.ActivityType.text = log.TypeOfActivity.ToString();
            newLogRefs.WasSuccessfull.text = log.WasSuccessfull.ToString();
        }
    }

    void Start()
    {
        gameObject.SetActive(false);
        _logsList = new List<GameObject>();

        _exitButton.onClick.AddListener(ClosePanel);
    }

    private void ClosePanel()
    {
        foreach (var gO in _logsList)
        {
            Destroy(gO);
        }

        _logsList.Clear();
        gameObject.SetActive(false);
    }

    private List<LogData> DeserializeLogData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "activity_log.json");

        // Check if the file exists
        if (!File.Exists(filePath))
        {
            Debug.LogError("Log file not found");
            return null;
        }

        string json = File.ReadAllText(filePath);
        LogDataList logList = JsonUtility.FromJson<LogDataList>(json);
        return logList.logDataList;
    }

}
