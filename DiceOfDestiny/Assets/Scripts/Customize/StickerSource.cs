using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StickerSource : MonoBehaviour, IPointerDownHandler
{
    public ClassSticker classSticker;
    public TextMeshProUGUI stickerCount;
    public GameObject draggableStickerPrefab;

    public void OnPointerDown(PointerEventData eventData)
    {
        GameObject draggableSticker = Instantiate(draggableStickerPrefab, transform.position, Quaternion.identity, GetComponentInParent<Canvas>().transform);
        draggableSticker.GetComponent<DraggableSticker>().Initialize(classSticker.classData,true);
        draggableSticker.GetComponent<DraggableSticker>().StartDragManually(GetComponentInParent<StickerDrawer>().gameObject.GetComponent<RectTransform>());

    }

}
