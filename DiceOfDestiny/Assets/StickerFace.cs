using UnityEngine;
using UnityEngine.UI;

public class StickerFace : MonoBehaviour
{
    public DraggableSticker draggableSticker;
    public TileColor tileColor;

    public void Initialize(DraggableSticker sticker)
    {
        draggableSticker = sticker;
    }
    public void Initialize(TileColor color)
    {
        draggableSticker = null;
        tileColor = color;
        this.gameObject.GetComponent<Image>().color = BoardManager.Instance.GetColor(color);
    }
}
