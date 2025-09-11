using UnityEngine;

public sealed class TutorialBoardBootstrap : MonoBehaviour
{
    [SerializeField] private BoardManager board;
    [SerializeField] private PieceManager pieceManager;
    [SerializeField] private int boardSize = 4;
    [SerializeField] private Vector2Int spawnPos = new Vector2Int(1, 1);
    [SerializeField] private int piecePrefabIndex = 0; // 기사면 Knight 인덱스 등

    private void Start()
    {
        // 보드 크기 설정 후 Initialize()
        var fieldInfo = typeof(BoardManager).GetField("boardSize",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        fieldInfo?.SetValue(board, boardSize);
        board.Initialize(); // 4x4 + 여유행 생성  :contentReference[oaicite:10]{index=10}

        // 기물 1개 스폰 및 현재 기물 지정
        pieceManager.GeneratePiece(piecePrefabIndex, spawnPos); // 보드 등록/하이라이트까지 처리  :contentReference[oaicite:11]{index=11}
    }
}
