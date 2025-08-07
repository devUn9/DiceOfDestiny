using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class PiecePreviewButton : MonoBehaviour
{
    public Image backgroundColor;
    public Button button;
    public Image classSticker;

    public void InitializePiecePreviewButton(Color backgroundColor, Sprite sticker, UnityAction onClick)
    {
        this.backgroundColor.color = backgroundColor;
        this.classSticker.sprite = sticker;
        button.onClick.AddListener(onClick);
    }
}
