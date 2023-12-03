using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Debugger : MonoBehaviour
{
    public GameObject debugParent;
    List<TextMeshProUGUI> debugTexts = new List<TextMeshProUGUI>();
    List<string> titles = new List<string>();
    public GameObject debugTextPrefab;

    public void setDebug(string title, string value)
    {
        int debugID = titles.IndexOf(title);

        if (debugID == -1)
        {
            Debug.Log(debugParent);

            TextMeshProUGUI newText = GameObject.Instantiate(debugTextPrefab, debugParent.transform).GetComponent<TextMeshProUGUI>();
            newText.name = title + " debug";

            debugTexts.Add(newText);
            titles.Add(title);
            setDebug(title, value);
            return;
        }

        debugTexts[debugID].text = title + ": " + value;
    }
}
