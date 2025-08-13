using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 피스의 면 데이터를 저장하고 복구하는 매니저
public class PieceFaceManager : Singletone<PieceFaceManager>
{
    // pieceNumber를 키로 하여 각 피스의 면 데이터(직업, 색상)와 면 순서를 저장
    private Dictionary<int, (Dictionary<int, (ClassData classData, TileColor color)> faceData, int[] faceOrder)> pieceFaceData = new Dictionary<int, (Dictionary<int, (ClassData, TileColor)>, int[])>();

    // 현재 피스의 면 데이터를 저장하는 함수
    public void SavePieceFaceData(int pieceNumber)
    {
        // 현재 선택된 피스 가져오기
        PieceController piece = PieceManager.Instance.GetCurrentPiece();
        if (piece == null)
        {
            Debug.LogError("현재 피스가 null입니다!");
            return;
        }

        Dictionary<int, (ClassData classData, TileColor color)> faceData = new Dictionary<int, (ClassData, TileColor)>();
        int[] faceOrder = new int[6];

        // 6개 면의 데이터를 저장
        for (int i = 0; i < 6; i++)
        {
            Face face = piece.GetFace(i);
            if (face.classData == null)
            {
                Debug.LogWarning($"면 {i}의 ClassData가 null입니다. 피스: {piece.GetInstanceID()}");
                continue;
            }
            faceData[i] = (face.classData, face.color);
            faceOrder[i] = i; // 기본 면 순서 저장 (0,1,2,3,4,5)
        }

        // 딕셔너리에 데이터 저장
        pieceFaceData[pieceNumber] = (faceData, faceOrder);
        Debug.Log($"피스 데이터 저장 완료: pieceNumber={pieceNumber}, 피스 ID={piece.GetInstanceID()}, 위치={piece.gridPosition}, 면 순서: {string.Join(",", faceOrder)}");
    }

    // 저장된 데이터를 사용해 피스의 면을 복구하는 함수
    public void RestorePieceFaceData(int pieceNumber)
    {
        // 현재 선택된 피스 가져오기
        PieceController piece = PieceManager.Instance.GetCurrentPiece();
        if (piece == null)
        {
            Debug.LogError("현재 피스가 null입니다!");
            return;
        }

        // 저장된 데이터 확인
        if (!pieceFaceData.ContainsKey(pieceNumber))
        {
            Debug.LogWarning($"pieceNumber={pieceNumber}에 해당하는 저장된 데이터가 없습니다. 피스 ID={piece.GetInstanceID()}, 위치={piece.gridPosition}");
            return;
        }

        // 저장된 데이터 가져오기
        var (faceData, faceOrder) = pieceFaceData[pieceNumber];

        // faceOrder가 null인지 확인
        if (faceOrder == null)
        {
            Debug.LogError($"faceOrder가 null입니다. 피스 ID={piece.GetInstanceID()}, 위치={piece.gridPosition}");
            return;
        }

        // 피스 데이터 확인
        Piece pieceData = piece.GetPiece();
        if (pieceData == null)
        {
            Debug.LogError($"Piece ScriptableObject가 null입니다. 피스 ID={piece.GetInstanceID()}");
            return;
        }

        // 6개 면 복구
        for (int i = 0; i < 6; i++)
        {
            if (i >= faceOrder.Length)
            {
                Debug.LogWarning($"faceOrder 배열 길이가 부족합니다: length={faceOrder.Length}, index={i}");
                continue;
            }

            int originalIndex = faceOrder[i];
            if (faceData.ContainsKey(originalIndex) && faceData[originalIndex].classData != null)
            {
                // 직업 복구
                piece.ChangeClass(i, faceData[originalIndex].classData.className);
                // 색상 복구
                piece.SetFaceColor(i, faceData[originalIndex].color);
                Debug.Log($"면 {i} 복구 완료: 직업={faceData[originalIndex].classData.className}, 색상={faceData[originalIndex].color}, 피스 ID={piece.GetInstanceID()}, 위치={piece.gridPosition}");
            }
            else
            {
                Debug.LogWarning($"면 {i}의 원본 ClassData가 없습니다. 위치={piece.gridPosition}");
            }
        }
    }
}