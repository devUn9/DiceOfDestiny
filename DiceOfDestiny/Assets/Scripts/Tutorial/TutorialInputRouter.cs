using UnityEngine;

public sealed class TutorialInputRouter : MonoBehaviour
{
    [SerializeField] private GameObject blocker;

    public void SetBlock(bool isBlocked)
    {
        if (blocker == null) return;
        blocker.SetActive(isBlocked);
    }
}