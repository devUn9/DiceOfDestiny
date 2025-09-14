using UnityEngine;
using UnityEngine.Events;

public sealed class TileClickRelay : MonoBehaviour
{
    [SerializeField] private UnityEvent onTileClicked;

    private void OnMouseUpAsButton()
    {
        onTileClicked?.Invoke();
    }
}
