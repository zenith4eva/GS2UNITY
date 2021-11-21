using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class GoogleSheet
{
	private string _URL = "https://script.google.com/macros/s/{0}/exec?sheetNameString={1}";
    WebRequestProvider _webRequestProvider;
    bool _updateRequestInProgress;

    public Dictionary<string, Dictionary<string, string>> sheetData = new Dictionary<string, Dictionary<string, string>>();
    public string currentRowID = "default";
    public string sheetAddress = "";
    public string sheetName = "";
    public bool isDataUpdatingFinished = false;
    public event Action OnUpdate;

    public GoogleSheet(string url, string sAddress, string sName, WebRequestProvider wrp)
    {
		_URL = url;
		this.sheetAddress = sAddress;
		this.sheetName = sName;
		_webRequestProvider = wrp;
	}

	public void OnlineUpdateSheetData(System.Action<bool> onDone = null)
	{
		if (_updateRequestInProgress)
		{
			onDone(false);
		}
		else
		{
			_updateRequestInProgress = true;

            var updateURL = string.Format(this._URL, sheetAddress, sheetName);

            _webRequestProvider.HTTPGet(updateURL, (wwwResult) =>
			{
				bool success = UpdateDataOnSuccess(wwwResult);
				onDone(success);
				_updateRequestInProgress = false;

				if (OnUpdate != null && success)
				{
					OnUpdate();
				}
			});
		}
	}

    public List<string> AvailableRows
    {
        get { return new List<string>(sheetData.Keys); }
    }

    public string CurrentRow
	{
		get { return currentRowID; }
		set { currentRowID = value; }
	}

	public Dictionary<string, string> CurrentRowData
	{
		get
		{
			Dictionary<string, string> result;
			if (sheetData.TryGetValue(CurrentRow, out result))
				return result;
			else
				return new Dictionary<string, string>();
		}
	}

	public string GetString(string key, string defaultValue = "")
	{
		Dictionary<string, string> data;
		if (sheetData.TryGetValue(currentRowID, out data))
		{
			string result;
			if (data.TryGetValue(key, out result))
			{
				return result;
			}
		}

		return defaultValue;
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		string data = GetString(key, null);

		if (data != null)
		{
			int result;
			if (int.TryParse(data, out result))
			{
				return result;
			}
		}

		return defaultValue;
	}

	public float GetFloat(string key, float defaultValue = 0)
	{
		string data = GetString(key, null);

		if (data != null)
		{
			float result;
			if (float.TryParse(data, out result))
			{
				return result;
			}
		}

		return defaultValue;
	}

    public string GetRowID(int rowID)
    {
        return sheetData.ElementAt(rowID).Key;
    }

    public string GetRowData(int rowIndex, string columnName)
    {
        if(sheetData[AvailableRows[rowIndex]].ContainsKey(columnName))
        {
            return sheetData[AvailableRows[rowIndex]][columnName];
        }
        else
        {
            return "";
        }
    }

    public string GetRowData(string rowName, string columnName)
    {
        if (sheetData[rowName].ContainsKey(columnName))
        {
            if(string.IsNullOrEmpty(sheetData[rowName][columnName]))
            {
                Debug.Log(rowName + " " + columnName);

                return "";
            }
            else
            {
                return sheetData[rowName][columnName];
            }
        }
        else
        {
            return "";
        }
    }


    bool UpdateDataOnSuccess(WebRequestProviderResult wpr)
	{
		if (wpr.error != null)
		{
			Debug.LogError("Update Failed... " + wpr.error);
			return false;
		}
		else
		{
			JSONUpdateSheetData(wpr.text);
			return true;
		}
	}

	string GetCellId(int column, int row)
	{
		return ((char)('A' + column - 1)).ToString() + row.ToString();
	}


	void JSONUpdateSheetData(string jsonData)
	{
        if (jsonData != null)
        {
            var root = MiniJSON.Json.Deserialize(jsonData) as Dictionary<string, object>;

            foreach (var kvp in root)
            {
                var entryData = kvp.Value as Dictionary<string, object>;
                var configVariant = new Dictionary<string, string>();

                foreach (var entryKVP in entryData)
                {
                    configVariant.Add(entryKVP.Key, entryKVP.Value.ToString());
                }

                sheetData.Add(kvp.Key, configVariant);
            }    
        }
        isDataUpdatingFinished = true;
    }
}