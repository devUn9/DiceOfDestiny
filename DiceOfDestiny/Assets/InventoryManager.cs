using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singletone<InventoryManager>
{
    public List<Piece> pieces = new List<Piece>();
    public List<PieceNet> pieceNets = new List<PieceNet>();
    public Dictionary<ClassData, int> classStickers = new Dictionary<ClassData, int>();

    [Header("class Data")]
    public ClassData knightClassData;
    public ClassData demonClassData;
    public ClassData babyClassData;
    public ClassData fanaticClassData;
    public ClassData thiefClassData;
    public ClassData preistClassData;
    public ClassData painterClassData;

    private void Awake()
    {
        TestInitialize();
    }

    void TestInitialize()
    {
        // 테스트용 초기화
        Debug.Log("InventoryManager 초기화");

        PieceNet testPieceNet = new PieceNet();
        testPieceNet.faces[0].color = TileColor.Red;
        testPieceNet.faces[1].color = TileColor.Blue;
        testPieceNet.faces[2].color = TileColor.Green;
        testPieceNet.faces[3].color = TileColor.Yellow;
        testPieceNet.faces[4].color = TileColor.Yellow;
        testPieceNet.faces[5].color = TileColor.Purple;
        AddPieceNet(testPieceNet);

        testPieceNet = new PieceNet();
        testPieceNet.faces[0].color = TileColor.Red;
        testPieceNet.faces[1].color = TileColor.Red;
        testPieceNet.faces[2].color = TileColor.Green;
        testPieceNet.faces[3].color = TileColor.Green;
        testPieceNet.faces[4].color = TileColor.Blue;
        testPieceNet.faces[5].color = TileColor.Blue;
        AddPieceNet(testPieceNet);

        testPieceNet = new PieceNet();
        testPieceNet.faces[0].color = TileColor.Red;
        testPieceNet.faces[1].color = TileColor.Blue;
        testPieceNet.faces[2].color = TileColor.Purple;
        testPieceNet.faces[3].color = TileColor.Yellow;
        testPieceNet.faces[4].color = TileColor.Blue;
        testPieceNet.faces[5].color = TileColor.Purple;
        AddPieceNet(testPieceNet);

        testPieceNet = new PieceNet();
        testPieceNet.faces[0].color = TileColor.Blue;
        testPieceNet.faces[1].color = TileColor.Blue;
        testPieceNet.faces[2].color = TileColor.Green;
        testPieceNet.faces[3].color = TileColor.Yellow;
        testPieceNet.faces[4].color = TileColor.Blue;
        testPieceNet.faces[5].color = TileColor.Red;
        AddPieceNet(testPieceNet);

        for(int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = knightClassData;
            AddSticker(testSticker);
        }   
        for(int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = demonClassData;
            AddSticker(testSticker);
        } 
        for(int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = thiefClassData;
            AddSticker(testSticker);
        }  
        for(int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = painterClassData;
            AddSticker(testSticker);
        }  
        for(int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = babyClassData;
            AddSticker(testSticker);
        } 
        for(int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = fanaticClassData;
            AddSticker(testSticker);
        }

    }

    public void AddPiece(Piece piece)
    {
        pieces.Add(piece);
    }

    public bool RemovePiece(Piece piece)
    {
        bool removed = pieces.Remove(piece);
        if (removed)
            Debug.Log($"Piece 제거: {piece.name}");
        else
            Debug.LogWarning($"제거 실패 - Piece를 찾을 수 없음: {piece.name}");

        return removed;
    }

    public List<Piece> GetPieces()
    {
        return new List<Piece>(pieces);
    }

    public void AddPieceNet(PieceNet pieceNet)
    {
        pieceNets.Add(pieceNet);
    }

    public bool RemovePieceNet(PieceNet pieceNet)
    {
        bool removed = pieceNets.Remove(pieceNet);
        if (removed)
            Debug.Log($"PieceNet 제거: {pieceNet}");
        else
            Debug.LogWarning($"제거 실패 - PieceNet을 찾을 수 없음: {pieceNet}");

        return removed;
    }

    public List<PieceNet> GetPieceNets()
    {
        return new List<PieceNet>(pieceNets);
    }

    public void AddSticker(ClassSticker sticker)
    {
        var key = sticker.classData;

        if (classStickers.ContainsKey(key))
            classStickers[key]++;
        else
            classStickers[key] = 1;
    }

    public void RemoveSticker(ClassSticker sticker)
    {
        var key = sticker.classData;
        if (classStickers.ContainsKey(key))
        {
            classStickers[key]--;
            if (classStickers[key] <= 0)
                classStickers.Remove(key);
        }
        else
        {
            Debug.LogWarning($"스티커 제거 실패 - 스티커를 찾을 수 없음: {sticker.classData.name}");
        }
    }
}
