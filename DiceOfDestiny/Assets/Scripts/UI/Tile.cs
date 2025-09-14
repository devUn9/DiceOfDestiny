using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileColor tileColor;
    [SerializeField] private ObstacleType obstacle;
    [SerializeField] private PieceController piece;
    public bool isWalkable { get; set; }

    private SpriteRenderer sr;

    public TileColor TileColor
    {
        get => tileColor;
        set => tileColor = value;
    }

    public ObstacleType Obstacle
    {
        get => obstacle;
        set => obstacle = value;
    }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetTileColor(Color color)
    {
        sr.color = color;
    }

    public PieceController GetPiece()
    {
        return piece;
    }

    public void SetPiece(PieceController newPiece)
    {
        piece = newPiece;
    }

    // 타일 눌렀을 때 호출, BoardSelectManager에 저장
    private void OnMouseUp()
    {
        //Debug.Log(GameManager.Instance.IsLockCursor);
        //if (GameManager.Instance.IsLockCursor)
        //    return; // 커서 잠금 상태면 무시

        // UI 위 클릭이면 무시
        if (IsPointerOnLayer("BlockUI"))
            return;

        if (SkillManager.Instance.IsSelectingProgress)
            return; // 스킬 진행 중이면 클릭 무시

        Vector2Int position = new Vector2Int(
        Mathf.RoundToInt(transform.position.x - BoardManager.Instance.boardTransform.position.x),
        Mathf.RoundToInt(transform.position.y - BoardManager.Instance.boardTransform.position.y));

        // 장애물이 있는 타일에 장애물 제한 트리거가 켜져있으면 저장 X
        if (!BoardManager.Instance.IsEmptyTile(position) && BoardSelectManager.Instance.restrictObstacle)
            return;

        // y값이 0 또는 13인 타일에 바운더리 제한이 켜져있으면 저장 X
        if ((position.y == 0 || position.y == 14) && BoardSelectManager.Instance.restrictYBoundaries)
            return;

        BoardSelectManager.Instance.SetClickedTilePosition(position);
        BoardSelectManager.Instance.ClearAllEffects();
    }

    private bool IsPointerOnLayer(string layerName)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        int targetLayer = LayerMask.NameToLayer(layerName);

        foreach (var result in results)
        {
            if (result.gameObject.layer == targetLayer)
                return true; // 해당 레이어 위에 있음
        }

        return false; // 해당 레이어 위에 없음
    }
}
