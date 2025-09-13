using UnityEngine;

public sealed class TutorialHighlightController : MonoBehaviour
{
    [SerializeField] private RectTransform pointer;

    public void Focus(RectTransform target)
    {
        if (pointer == null || target == null) return;
        pointer.position = target.position;
        pointer.gameObject.SetActive(true);
    }

    public void Clear()
    {
        if (pointer == null) return;
        pointer.gameObject.SetActive(false);
    }
}