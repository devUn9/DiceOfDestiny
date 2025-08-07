using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSticker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    bool isDragging = false;
    bool createdByDrawer = false; // 스티커가 서랍에서 생성되었는지 여부
    [HideInInspector] public ClassSticker classSticker;
    private Transform originalParent;

    private RectTransform rectTransform;
    private Canvas rootCanvas;



    public void Initialize(ClassData classData, bool createdByDrawer = false)
    {
        this.classSticker = new ClassSticker { classData = classData };
        this.gameObject.GetComponent<Image>().sprite = classData.sprite;
        this.createdByDrawer = createdByDrawer;

        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rootCanvas.transform as RectTransform,
                Input.mousePosition,
                rootCanvas.worldCamera,
                out localPoint
            );
            rectTransform.anchoredPosition = localPoint;

            if (createdByDrawer && Input.GetMouseButtonUp(0))
            {
                HandleDrop();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartDrag(eventData);
    }

    private void StartDrag(PointerEventData eventData)
    {
        if (isDragging) return;

        isDragging = true;
        gameObject.GetComponent<Image>().raycastTarget = false;

        originalParent = transform.parent;

        // 부모를 캔버스로 변경해서 UI 최상위로
        transform.SetParent(rootCanvas.transform, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("드래그 종료");
        if (isDragging)
        {
            Debug.Log("드래그한 스티커를 놓았습니다.");
            HandleDrop();
        }
    }

    private void HandleDrop()
    {
        // UI 위에 있는 오브젝트 확인
        GameObject target = GetObjectUnderPointer();

        if (target == null)
        {
            ReturnToOriginalPosition();
            return;
        }

        var stickerFace = target.GetComponentInParent<StickerFace>();
        if (stickerFace != null)
        {
            HandleDropOnEmptyFace(stickerFace);
            return;
        }

        var stickerDrawer = target.GetComponentInParent<StickerDrawer>();
        if (stickerDrawer != null)
        {
            HandleDropInDrawer(stickerDrawer);
            return;
        }

        ReturnToOriginalPosition();
    }

    public void HandleDropOnEmptyFace(StickerFace stickerFace)
    {
        if (stickerFace.draggableSticker == null)
        {
            stickerFace.draggableSticker = this;
            this.transform.SetParent(stickerFace.transform, false);
            this.rectTransform.anchoredPosition = Vector2.zero;
            isDragging = false;
            gameObject.GetComponent<Image>().raycastTarget = true;
            if (createdByDrawer)
            {
                InventoryManager.Instance.RemoveSticker(classSticker);
                DiceCustomizeManager.Instance.UpdateStickerDrawer();
                createdByDrawer = false; // 드래그 후에는 서랍에서 생성된 것이 아님
            }
            else
            {
                originalParent.GetComponent<StickerFace>().draggableSticker = null;
            }
        }
        else
        {
            Debug.Log("스티커가 이미 존재하는 얼굴에 드래그했습니다. 원래 위치로 되돌립니다.");
            ReturnToOriginalPosition();
        }
    }

    private void HandleDropInDrawer(StickerDrawer stickerDrawer)
    {
        Debug.Log("드래그한 스티커를 스티커 서랍에 넣었습니다.");
        if (createdByDrawer)
        {
            Destroy(gameObject);
        }
        else
        {
            originalParent.GetComponent<StickerFace>().draggableSticker = null;
            InventoryManager.Instance.AddSticker(classSticker);
            DiceCustomizeManager.Instance.UpdateStickerDrawer();
            Destroy(gameObject);
        }

    }

    public void ReturnToOriginalPosition()
    {
        if (createdByDrawer)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("드래그한 스티커를 원래 위치로 되돌렸습니다.");
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one; // 원래 크기로 되돌리기

            isDragging = false;
            gameObject.GetComponent<Image>().raycastTarget = true;
        }
    }

    private GameObject GetObjectUnderPointer()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
            return results[0].gameObject;

        return null;
    }

    public void StartDragManually(RectTransform parantRectTransform)
    {
        if (isDragging) return;

        isDragging = true;
        createdByDrawer = true;
        gameObject.GetComponent<Image>().raycastTarget = false;

        transform.SetParent(rootCanvas.transform, true);
        GetComponent<RectTransform>().anchoredPosition += parantRectTransform.anchoredPosition;
    }


}

