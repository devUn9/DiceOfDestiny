using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class SceneRouter : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainScene";

    public void LoadNext()
    {
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning("[Tutorial] nextSceneName is empty.");
            return;
        }
        SceneManager.LoadScene(nextSceneName);
    }
}
