using UnityEngine;
using Wave.Essence;
using Wave.Native;

namespace SharedSpaceExperience.UI
{
    public class Keyboard : MonoBehaviour
    {
        private IMEManager imeManager = null;
        private IMEManager.IMEParameter currentIMEParameter = null;
        private const int MODE_FLAG_FIX_MOTION = 0x02;
        private bool isInited = false;
        private bool isShow = false;
        private bool isUpdated = false;
        private bool isNumeric = false;
        private bool isIPInput = false;
        private string inputClickText = null;
        private TextInput selectedTextInput = null;

        void Start()
        {
            imeManager = IMEManager.instance;
            isInited = imeManager.isInitialized();
            isShow = false;
            InitParameter();
        }

        public void InitParameter()
        {
            int id = 0;
            int type = MODE_FLAG_FIX_MOTION;
            int mode = 2;

            string exist = "";
            int cursor = 0;
            int selectStart = 0;
            int selectEnd = 0;
            double[] pos = null;
            double[] rot = null;
            int width = 800;
            int height = 800;
            int shadow = 100;
            string locale = "";
            string title = "";
            int extraInt = 0;
            string extraString = "";
            int buttonId = (1 << (int)WVR_InputId.WVR_InputId_Alias1_Thumbstick)
                | (1 << (int)WVR_InputId.WVR_InputId_Alias1_Touchpad)
                | (1 << (int)WVR_InputId.WVR_InputId_Alias1_Trigger)
                | (1 << (int)WVR_InputId.WVR_InputId_Alias1_Bumper);
            currentIMEParameter = new IMEManager.IMEParameter(
                id, type, mode, exist, cursor, selectStart, selectEnd, pos, rot,
                width, height, shadow, locale, title, extraInt, extraString, buttonId
            );
        }

        public void ShowKeyboard(TextInput textInput)
        {
            selectedTextInput = textInput;
            isNumeric = selectedTextInput.isNumeric;
            isIPInput = selectedTextInput.isIPInput;
            inputClickText = selectedTextInput.inputField == null ? "" : selectedTextInput.inputField.text;

            if (isInited && !isShow)
            {
                imeManager.showKeyboard(currentIMEParameter, false, InputDoneCallback, InputClickCallback);
                isShow = true;
            }
        }

        public void HideKeyboard()
        {
            imeManager.hideKeyboard();
            isShow = false;
        }

        public void InputDoneCallback(IMEManager.InputResult results)
        {
            // deselect input field
            if (selectedTextInput != null)
            {
                selectedTextInput.Deselect();
                selectedTextInput = null;
            }
            isShow = false;
        }

        public void InputClickCallback(IMEManager.InputResult results)
        {
            switch (results.KeyCode)
            {
                case IMEManager.InputResult.Key.BACKSPACE:
                    inputClickText = inputClickText[..^1];
                    break;
                case IMEManager.InputResult.Key.ENTER:
                    HideKeyboard();
                    break;
                case IMEManager.InputResult.Key.CLOSE:
                    isShow = false;
                    break;
                default:
                    if (!isIPInput || CheckIsIPv4Input(inputClickText + results.InputContent))
                    {
                        inputClickText += results.InputContent;
                    }
                    break;
            }

            isUpdated = true;
        }

        private bool CheckIsIPv4Input(string inputStr)
        {
            string[] numList = inputStr.Split(".");
            if (numList.Length > 4) return false;

            int i = 0;
            while (i < numList.Length - 1)
            {
                if ((numList[i].Length > 1 && numList[i][0] == '0') ||
                    !int.TryParse(numList[i], out int num) ||
                    num > 255
                ) return false;
                ++i;
            }
            if (numList[i].Length != 0 &&
                ((numList[i].Length > 1 && numList[i][0] == '0') ||
                 !int.TryParse(numList[i], out int n)
                 || n > 255)
            ) return false;

            return true;
        }

        private void Update()
        {
            // update input field
            if (isUpdated)
            {
                if (selectedTextInput?.inputField != null)
                {
                    selectedTextInput.inputField.text = inputClickText;
                }
                isUpdated = false;
            }
        }
    }
}