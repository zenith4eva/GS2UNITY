using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GS2UnityDemoScript : MonoBehaviour
{
    GoogleSheetsDB googleSheetsDB;
    public GoogleSheet txtSheet;

    public Text txt_1;
    public Text txt_2;
    public Text txt_3;

    private void OnEnable()
    {
        googleSheetsDB = gameObject.GetComponent<GoogleSheetsDB>();
        googleSheetsDB.OnDownloadComplete += UpdateText;
    }

    private void OnDisable()
    {
        googleSheetsDB.OnDownloadComplete -= UpdateText;
    }

    public void UpdateText()
    {
        int txtSheetIndex = googleSheetsDB.sheetTabNames.IndexOf("TextDemo");

        txtSheet = googleSheetsDB.dataSheets[txtSheetIndex];
        txt_1.text = txtSheet.GetRowData("txt_1", "text");
        txt_2.text = txtSheet.GetRowData("txt_2", "text");
        txt_3.text = txtSheet.GetRowData("txt_3", "text");
    }
}
