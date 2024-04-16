using UnityEngine;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience
{
    public class LocalUserProperty : MonoBehaviour
    {
        private const string FILE_NAME = "local_user.json";
        public string userName = "User";
        public string lastConnectedServer = "";

        private void OnEnable()
        {
            Load();
        }

        private bool Load()
        {
            if (!LocalStorage.Load(FILE_NAME, out string data)) return false;

            JsonUtility.FromJsonOverwrite(data, this);
            return true;
        }

        public void Save()
        {
            string data = JsonUtility.ToJson(this);
            Logger.Log(data);

            LocalStorage.SaveAsync(FILE_NAME, data);
        }
    }
}