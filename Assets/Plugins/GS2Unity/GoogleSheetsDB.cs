using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class GoogleSheetsDB : MonoBehaviour
{
    public bool isOnlineMode = true;
    public string googleSheetAddress = "";
    public bool saveRemoteData = true;
    public string FolderName = "GameData";
    [HideInInspector]
    public string DBPath;

    public List<string> sheetTabNames = new List<string>();
    public List<GoogleSheet> dataSheets = new List<GoogleSheet>();

    private string _googleSheetsRequestAddressFormat = "https://script.google.com/macros/s/{0}/exec?sheetNameString={1}";

    public event System.Action OnDownloadComplete;


    public void Awake()
    {
        DBPath = Path.Combine(Application.persistentDataPath, FolderName);

        if (!Directory.Exists(DBPath))
        {
            Directory.CreateDirectory(DBPath);
        }
    }

    public void Start()
    {
        ImportData();
    }

    public void ImportData()
    {
        if (sheetTabNames.Count == 0)
        {
            Debug.LogError("Add name of the sheet tabs to download from");
            return;
        }

        if (isOnlineMode)
        {
            StartCoroutine(ImportRemoteData());
        }
        else
        {
            ImportLocalData();
        }
    }


    public IEnumerator ImportRemoteData()
    {
        if (!string.IsNullOrEmpty(googleSheetAddress))
        {
            dataSheets.Clear();

            for (var i = 0; i < sheetTabNames.Count; i++)
            {
                StartCoroutine(RequestSheet(sheetTabNames[i]));
            }

            yield return new WaitUntil(() => dataSheets.Count == sheetTabNames.Count);

            if (OnDownloadComplete != null)
                OnDownloadComplete();
        }
        else
        {
            Debug.LogError("Check the GS Address or check the tab name");
        }
    }

    public void ImportLocalData()
    {
        dataSheets.Clear();

        foreach (var sheetName in sheetTabNames)
        {
            string fileName = Path.Combine(DBPath, sheetName + ".json");

            if (File.Exists(fileName))
            {
                string jsonStr = File.ReadAllText(fileName);
                var googleSheet = new GoogleSheet(_googleSheetsRequestAddressFormat, googleSheetAddress, sheetName, new UnityWebRequestProvider());

                if (googleSheet.LoadFromJSON(jsonStr))
                {
                    dataSheets.Add(googleSheet);
                    Debug.Log("Loaded local sheet: " + sheetName);
                }
                else
                {
                    Debug.LogError("Failed to parse local sheet: " + sheetName);
                }
            }
            else
            {
                 Debug.LogWarning("Local file not found: " + fileName);
            }
        }

        if (OnDownloadComplete != null)
        {
            OnDownloadComplete();
        }
    }

    public IEnumerator RequestSheet(string sheetName)
    {
        var googleSheet = new GoogleSheet(_googleSheetsRequestAddressFormat, googleSheetAddress, sheetName, new UnityWebRequestProvider());
        googleSheet.OnlineUpdateSheetData((success) => {});

        yield return new WaitUntil(() => googleSheet.isDataUpdatingFinished == true);

        dataSheets.Add(googleSheet);

        if (isOnlineMode && saveRemoteData && !googleSheet.isError)
        {
            SaveSheet(googleSheet);
        }
    }

    public void SaveSheet(GoogleSheet googleSheet)
    {
        string fileName = Path.Combine(DBPath, googleSheet.sheetName + ".json");
        var jsonStr = MiniJSON.Json.Serialize(googleSheet.sheetData);
        File.WriteAllText(fileName, jsonStr);

        Debug.Log("Online dataSheets Saved: " + googleSheet.sheetName);
    }

    public void LoadSheet()
    {
        ImportLocalData();
    }
}
