using UnityEngine;
using System;
using TMPro;

public class LoggerUI : MonoBehaviour
{
    public TMP_Text textMesh;
    public int maxCharacterCount = 1024;
    string log;

    void Update()
    {
        try
        {
            if (Logger.TryGetLog(out log))
            {
                textMesh.text += log + "\n";
                if (textMesh.text.Length > maxCharacterCount)
                {
                    textMesh.text = textMesh.text.Substring(textMesh.text.Length - maxCharacterCount);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Logger Error: " + e);
        }
    }
}
