using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSaveSystem : MonoBehaviour
{
    private const string KeySaveExists = "SaveExists";
    private const string KeyLastScene = "LastScene";

    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveCurrentState();
    }

    private void OnApplicationQuit()
    {
        SaveCurrentState();
    }

    private void SaveCurrentState()
    {
        var scene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(KeyLastScene, scene);
        PlayerPrefs.SetInt(KeySaveExists, 1);
        PlayerPrefs.Save();
    }
}
