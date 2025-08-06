using UnityEngine;

public class PieceNet
{
    [SerializeField] public Face[] faces = new Face[6]; // 6개 면 데이터

    public PieceNet()
    {
        for (int i = 0; i < 6; i++)
            faces[i] = new Face(); // 각 요소도 인스턴스화 필요
    }
}
