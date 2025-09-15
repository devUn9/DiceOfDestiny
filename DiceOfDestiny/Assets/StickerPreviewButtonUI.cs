using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class StickerPreviewButtonUI : MonoBehaviour
{
    public bool isUnlock = false;
    public int count = 0;

    public ClassSticker classSticker;
    public Image classImage;
    public Image lockImage;
    public TextMeshProUGUI countText;
    public GameObject classSprite;
    public Button button;

    public void Initialize(bool isUnlock, ClassSticker classSticker, UnityAction onClick)
    {
        this.isUnlock = isUnlock;
        this.classSticker = classSticker;
        button.onClick.AddListener(onClick);

        if (isUnlock)
        {
            lockImage.gameObject.SetActive(false);
            countText.gameObject.SetActive(true);
            InventoryManager.Instance.classStickers.TryGetValue(classSticker.classData, out count);
            countText.text = "x" + count;
            classImage.sprite = classSticker.classData.sprite;
            classImage.color = Color.white;
        }
        else
        {
            lockImage.gameObject.SetActive(true);
            countText.gameObject.SetActive(false);
            classImage.sprite = null;
            classImage.color = Color.gray;
        }
    }
}
