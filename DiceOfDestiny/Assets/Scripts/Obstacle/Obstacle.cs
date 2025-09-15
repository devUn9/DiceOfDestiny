using UnityEngine;
using System.Collections;

public enum ObstacleType
{
    Zombie,
    Tree,
    Rock,
    Lion,
    Puddle,
    Chest,
    PoisonousHerb,
    Grass,
    Slime,
    SlimeDdong,
    Pawn,
    Rook,
    Knight,
    WoodBox,
    House,
    None
}

public enum NextStep
{
    Up,
    Down,
    Left,
    Right,
    None
}

public class Obstacle : MonoBehaviour
{
    public ObstacleType obstacleType;

    public NextStep nextStep = NextStep.None;
    public Vector2Int obstaclePosition;

    public bool isWalkable;

    public SpriteRenderer spriteRenderer { get; private set; }
    public Animator animator { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    protected IEnumerator GoHand(PieceController pieceController, float duration = 1.5f)
    {
        GameManager.Instance.IsLockCursor = true;

        // 약간의 연출 대기 (예: 1.5초)
        yield return new WaitForSeconds(duration);

        // 타일 상의 피스 존재 제거
        BoardManager.Instance.GetTile(pieceController.gridPosition).SetPiece(null);

        if (pieceController.gameObject == null)
            yield break;

        // 기존 보드의 기물 제거
        Destroy(pieceController.gameObject);

        // 피스 리스트 상의 피스 제거
        PieceManager.Instance.Pieces.Remove(pieceController);

        // 기물 선택 타일 제거
        BoardSelectManager.Instance.DestroyPieceHighlightTile();

        // 약간의 연출 대기 (예: 0.5초)
        yield return new WaitForSeconds(0.5f);

        // 가방에 새로운 기물 생성
        for (int i = 0; i < 4; i++)
        {
            if (PieceManager.Instance.pieceDatas[i] == null) // 슬롯이 비어 있는 경우
            {
                Debug.Log("가방에 기물 생성");
                PieceManager.Instance.pieceDatas[i] = pieceController.GetPiece();
                EventManager.Instance.TriggerEvent("Refresh");
                break;
            }
        }

        GameManager.Instance.IsLockCursor = false;
    }
}
