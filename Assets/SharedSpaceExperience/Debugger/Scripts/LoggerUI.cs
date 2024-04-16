using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System;
using Logger = Debugger.Logger;

public class LoggerUI : MonoBehaviour
{

    [SerializeField]
    private int maxCharacterCount = 128 * Logger.MAX_BUFFER_SIZE; // 128 * 128

    [SerializeField]
    private GameObject canvas;
    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private TMP_Text textMesh;
    [SerializeField]
    private RectTransform textRect;

    [SerializeField]
    private Color headerColor;
    [SerializeField]
    private Color debugColor;
    [SerializeField]
    private Color warningColor;
    [SerializeField]
    private Color errorColor;

    [SerializeField]
    private InputAction showUIAction;

    [SerializeField]
    private InputAction scrollAction;

    [SerializeField]
    private bool showMethodName = true;

    private bool showUI = false;
    private bool hasUpdated = false;

    private bool isScrolling = false;

    private float scrollSpeed = 0;

    private Queue<int> charCounts = new();

    private void OnEnable()
    {
        textMesh.text = "";
        canvas.SetActive(showUI);

        showUIAction.Enable();
        scrollAction.Enable();
        showUIAction.started += ShowUI;
        scrollAction.started += StartScroll;
        scrollAction.performed += ChangeScrollSpeed;
        scrollAction.canceled += StopScroll;

        charCounts.Clear();

        Logger.EnableBuffer();
    }

    private void OnDisable()
    {
        Logger.DisableBuffer();

        charCounts.Clear();

        showUIAction.started -= ShowUI;
        scrollAction.started -= StartScroll;
        scrollAction.performed -= ChangeScrollSpeed;
        scrollAction.canceled -= StopScroll;
        showUIAction.Disable();
        scrollAction.Disable();
    }

    private void Update()
    {
        if (hasUpdated)
        {
            // scroll to bottom
            ScrollToBottom();
            hasUpdated = false;
        }
        Logging();

        // user scrolling
        if (isScrolling)
        {
            scrollRect.verticalNormalizedPosition += scrollSpeed * Time.deltaTime;
        }
    }

    private void ShowUI(InputAction.CallbackContext context)
    {
        showUI = !showUI;
        canvas.SetActive(showUI);

        if (showUI)
        {
            ScrollToBottom();
        }
    }

    private void StartScroll(InputAction.CallbackContext context)
    {
        if (showUI) isScrolling = true;
    }

    private void StopScroll(InputAction.CallbackContext context)
    {
        isScrolling = false;
    }
    private void ChangeScrollSpeed(InputAction.CallbackContext context)
    {
        float height = textRect.rect.height;
        scrollSpeed = height > 0 ? context.ReadValue<float>() * 3000 / height : 0;
    }

    private void Logging()
    {
        try
        {
            int logCount = Logger.GetLogCount();
            hasUpdated |= logCount > 0;

            while (logCount > 0 && Logger.TryGetLog(out Logger.LogObject log))
            {
                string coloredLog = GenFormattedLog(log);
                textMesh.text += coloredLog;

                charCounts.Enqueue(coloredLog.Length);

                --logCount;
            }

            if (textMesh.text.Length > maxCharacterCount)
            {
                int exceed = textMesh.text.Length - maxCharacterCount;
                while (exceed > 0 && charCounts.TryDequeue(out int removedCharCount))
                {
                    exceed -= removedCharCount;
                }

                textMesh.text = textMesh.text.Substring(textMesh.text.Length - maxCharacterCount - exceed);
            }

            if (hasUpdated)
            {
                // fit text rect size
                // since rect will not update immediately, we have to scroll view in the next frame
                textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textMesh.GetPreferredValues().y);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[LoggerUI] Logger Error: " + e);
        }

    }

    private string GenFormattedLog(Logger.LogObject log)
    {
        Color textColor = debugColor;
        switch (log.level)
        {
            case Logger.LogLevel.Warning:
                textColor = warningColor;
                break;
            case Logger.LogLevel.Error:
                textColor = errorColor;
                break;
        }

        string formattedLog = $"<color=#{ColorUtility.ToHtmlStringRGB(headerColor)}>[{log.className}]";
        if (showMethodName) formattedLog += $"[{log.methodName}]";
        formattedLog += $"</color>\v<color=#{ColorUtility.ToHtmlStringRGB(textColor)}>{log.log.Replace('\n', '\v')}</color>\n";

        return formattedLog;
    }

    private void ScrollToBottom()
    {
        // scroll to bottom
        scrollRect.verticalNormalizedPosition = 0;
    }
}
