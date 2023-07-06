using System.Collections;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonUtils
{
    public static T GetRoomProperty<T>(string key)
    {
        return (T)PhotonNetwork.CurrentRoom.CustomProperties[key];
    }

    public static bool TryGetRoomProperty(string key, out object value)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out value);
    }

    public static void SetRoomProperty(string key, object value)
    {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { key, value } });
    }

    public static bool HasRoomProperty(string key)
    {
        return PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(key);
    }

    public static T GetPlayerProperty<T>(Player player, string key)
    {
        return (T)player.CustomProperties[key];
    }

    public static bool TryGetPlayerProperty(Player player, string key, out object value)
    {
        return player.CustomProperties.TryGetValue(key, out value);
    }

    public static void SetPlayerProperty(Player player, string key, object value)
    {
        player.SetCustomProperties(new Hashtable { { key, value } });
    }

    public static void SetPlayerProperty(Player player, Hashtable properites)
    {
        player.SetCustomProperties(properites);
    }

    public static bool HasPlayerProperty(Player player, string key)
    {
        return player.CustomProperties.ContainsKey(key);
    }

    public static Player GetPlayer(int id, bool findMaster = false)
    {
        return PhotonNetwork.CurrentRoom.GetPlayer(id, findMaster);
    }

    public static void LogRoomProperty()
    {
        string log = "Room Properties:\n";
        foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
        {
            log += entry.Key + ": " + entry.Value + "\n";
        }
        Logger.Log(log);
    }

    public static void LogPlayerProperty(Player player)
    {
        string log = "Player " + player.ActorNumber + " Properties:\n";
        foreach (DictionaryEntry entry in player.CustomProperties)
        {
            log += entry.Key + ": " + entry.Value + "\n";
        }
        Logger.Log(log);
    }
}
