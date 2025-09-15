using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : Singletone<InventoryManager>
{
    public List<Piece> pieces = new List<Piece>();
    public List<PieceNet> pieceNets = new List<PieceNet>();
    public Dictionary<ClassData, int> classStickers = new Dictionary<ClassData, int>();
    public Dictionary<ClassData, bool> classUnlockStatus = new Dictionary<ClassData, bool>();

    [Header("class Data")]
    public ClassData knightClassData;
    public ClassData demonClassData;
    public ClassData babyClassData;
    public ClassData fanaticClassData;
    public ClassData thiefClassData;
    public ClassData preistClassData;
    public ClassData painterClassData;
    public ClassData berserkerClassData;
    public ClassData loggerClassData;
    public ClassData wizardClassData;

    [HideInInspector] public List<ClassData> classDataList;

    protected override void Awake()
    {
        base.Awake();
        SetClassDataList();
        TestInitialize();
    }

    private void SetClassDataList()
    {
        classDataList = new List<ClassData>
        {
            knightClassData,
            demonClassData,
            babyClassData,
            fanaticClassData,
            thiefClassData,
            preistClassData,
            painterClassData,
            berserkerClassData,
            loggerClassData,
            wizardClassData
        };

        foreach (var classData in classDataList)
        {
            Debug.Log($"ClassData 추가: {classData}");
            if (!classUnlockStatus.ContainsKey(classData))
            {
                classUnlockStatus[classData] = false; // 기본적으로 잠금 상태로 설정
            }
            
        }
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
        for (int i = 0; i < 5; i++)
        {
            ClassSticker testSticker = new ClassSticker();
            testSticker.classData = berserkerClassData;
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
            classUnlockStatus[key] = true; // 스티커를 처음 얻을 때 해당 클래스 잠금 해제
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

    public string GetRandomItem()
    {
        float random = Random.Range(0.0f, 1.0f);

        if (random > 0.9f)
        {
            var item = GenerateRandomPieceNet();
            AddPieceNet(item);
            return "전개도를 얻었습니다.";
        }
        else
        {
            var item = GenerateRandomSticker();
            AddSticker(item);

            string className = "";
            if (item.classData == knightClassData)
            {
                className = "기사";
            }
            else if (item.classData == demonClassData)
            {
                className = "악마";
            }
            else if (item.classData == babyClassData)
            {
                className = "아기";
            }
            else if (item.classData == fanaticClassData)
            {
                className = "광신도";
            }
            else if (item.classData == thiefClassData)
            {
                className = "도둑";
            }
            else if (item.classData == preistClassData)
            {
                className = "사제";
            }
            else if (item.classData == painterClassData)
            {
                className = "화가";
            }
            else if (item.classData == berserkerClassData)
            {
                className = "광전사";
            }
            else if (item.classData == loggerClassData)
            {
                className = "나무꾼";
            }
            else if (item.classData == wizardClassData)
            {
                className = "마법사";
            }

            return $"{className} 스티커를 얻었습니다.";
        }
    }

    public ClassSticker GenerateRandomSticker()
    {
        List<ClassData> classDataList = new List<ClassData>
        {
            knightClassData,
            demonClassData,
            babyClassData,
            fanaticClassData,
            thiefClassData,
            preistClassData,
            painterClassData,
            berserkerClassData,
            loggerClassData,
            wizardClassData
        };
        int randomIndex = Random.Range(0, classDataList.Count);
        ClassSticker sticker = new ClassSticker { classData = classDataList[randomIndex] };
        return sticker;
    }

    public PieceNet GenerateRandomPieceNet()
    {
        PieceNet pieceNet = new PieceNet();
        for (int i = 0; i < 6; i++)
        {
            pieceNet.faces[i].color = (TileColor)Random.Range(0, System.Enum.GetValues(typeof(TileColor)).Length);
        }
        return pieceNet;

    }
}
