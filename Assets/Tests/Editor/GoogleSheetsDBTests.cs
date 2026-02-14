using NUnit.Framework;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class GoogleSheetsDBTests
{
    private string testDBPath;
    private string folderName = "TestGameData";

    [SetUp]
    public void Setup()
    {
        testDBPath = Path.Combine(Application.persistentDataPath, folderName);
        if (!Directory.Exists(testDBPath))
        {
            Directory.CreateDirectory(testDBPath);
        }
    }

    [TearDown]
    public void Teardown()
    {
        if (Directory.Exists(testDBPath))
        {
            Directory.Delete(testDBPath, true);
        }
    }

    [Test]
    public void TestLoadFromJSON()
    {
        string json = "{\"Row1\": {\"Col1\": \"Val1\", \"Col2\": \"Val2\"}}";
        var sheet = new GoogleSheet("", "", "TestSheet", null);

        bool result = sheet.LoadFromJSON(json);

        Assert.IsTrue(result);
        Assert.AreEqual(1, sheet.AvailableRows.Count);
        Assert.AreEqual("Val1", sheet.GetRowData("Row1", "Col1"));
        Assert.AreEqual("Val2", sheet.GetRowData("Row1", "Col2"));
    }

    [Test]
    public void TestImportLocalData()
    {
        string sheetName = "TestSheet";
        string json = "{\"Row1\": {\"Col1\": \"Val1\", \"Col2\": \"Val2\"}}";
        string filePath = Path.Combine(testDBPath, sheetName + ".json");
        File.WriteAllText(filePath, json);

        var go = new GameObject();
        var db = go.AddComponent<GoogleSheetsDB>();

        db.DBPath = testDBPath;
        db.sheetTabNames = new List<string> { sheetName };
        db.isOnlineMode = false;

        bool callbackCalled = false;
        db.OnDownloadComplete += () => { callbackCalled = true; };

        db.ImportData();

        Assert.IsTrue(callbackCalled);
        Assert.AreEqual(1, db.dataSheets.Count);
        Assert.AreEqual("Val1", db.dataSheets[0].GetRowData("Row1", "Col1"));

        Object.DestroyImmediate(go);
    }
}
