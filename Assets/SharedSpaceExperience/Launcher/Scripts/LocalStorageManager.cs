using UnityEngine;
using System.IO;
using System.Threading.Tasks;

namespace SharedSpaceExperience
{
    public class LocalStorage
    {
        private static string GetFilePath(string fileName)
        {
            return Path.Join(Application.persistentDataPath, fileName);
        }

        public static bool Load(string fileName, out string text)
        {
            text = null;
            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath)) return false;

            text = File.ReadAllText(filePath);
            return true;
        }

        public static bool Load(string fileName, out byte[] data)
        {
            data = null;
            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath)) return false;

            data = File.ReadAllBytes(filePath);
            return true;
        }

        public static async Task<byte[]> LoadBytesAsync(string fileName)
        {
            string filePath = GetFilePath(fileName);
            if (!File.Exists(filePath)) return null;

            return await File.ReadAllBytesAsync(filePath);
        }

        public static async void SaveAsync(string fileName, string data)
        {
            string filePath = GetFilePath(fileName);
            await File.WriteAllTextAsync(filePath, data);
        }

        public static async void SaveAsync(string fileName, byte[] data)
        {
            string filePath = GetFilePath(fileName);
            await File.WriteAllBytesAsync(filePath, data);
        }

    }
}