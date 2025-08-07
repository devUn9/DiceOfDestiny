using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    [SerializeField] private TileColor tileColor;
    private ObstacleType obstacle;
    [SerializeField] private PieceController piece;
    public bool isWalkable { get; set; }

    SpriteRenderer sr;

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
    private void OnMouseDown()
    {
        if (SkillManager.Instance.IsSelectingProgress)
            return; // 스킬 진행 중이면 클릭 무시

        Vector2Int position = new Vector2Int(
        Mathf.RoundToInt(transform.position.x - BoardManager.Instance.boardTransform.position.x),
        Mathf.RoundToInt(transform.position.y - BoardManager.Instance.boardTransform.position.y));

        if (!BoardManager.Instance.IsEmptyTile(position) && BoardSelectManager.Instance.restrictObstacle)
            return; // 장애물이 있는 타일에 장애물 제한 트리거가 켜져있으면 저장하지마.

        if (piece != null)
        {
            PieceManager.Instance.currentPiece = piece;
            BoardSelectManager.Instance.PieceHighlightTiles(position);
            EventManager.Instance.TriggerEvent("ToggleUIElement");
        }
        BoardSelectManager.Instance.SetClickedTilePosition(position);
        BoardSelectManager.Instance.ClearAllEffects();
    }
}
