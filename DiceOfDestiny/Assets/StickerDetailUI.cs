using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public class StickerDetailUI : MonoBehaviour
{
    public Image classSprite;
    public TextMeshProUGUI classNameText;
    public TextMeshProUGUI classDescriptionText;

    public void SetDefault()
    {
        classSprite.sprite = null;
        classNameText.text = "Select a Sticker";
        classDescriptionText.text = "";
    }

    public void SetDetail(ClassSticker classSticker)
    {
        classSprite.sprite = classSticker.classData.sprite;
        if (classSticker.classData.className == "Knight")
        {
            classNameText.text = "직업 : 기사";
            classDescriptionText.text = "설명 : 용맹하지만 질병에 취약하다.\n패시브 : 상하좌우 4칸을 공격한다.\n스킬 : 진행하던 방향으로 1칸 돌진하며, 칸에 장애물이 있을경우 부수며 이동한다.";
        }
        else if (classSticker.classData.className == "Demon")
        {
            classNameText.text = "직업 : 악마";
            classDescriptionText.text = "설명 : 악한 존재이다.\n패시브 : 진행방향의 상단 3칸을 공격한다.\n스킬 : 게임판에서 비어있는 칸 하나를 선택하여 독초를 심는다.\n제약조건 : 주위 8칸에 사제와 함께할 수 없다.";
        }
        else if (classSticker.classData.className == "Baby")
        {
            classNameText.text = "직업 : 아기";
            classDescriptionText.text = "설명 : 보호 받아야 하는 존재.\n패시브 : - \n스킬 : 원하는 말을 하나 선택하여 아기쪽으로 한 칸 이동한다.\n제약조건 : 아기가 질병에 걸리면 말이 손으로 돌아간다.";
        }
        else if (classSticker.classData.className == "Fanatic")
        {
            classNameText.text = "직업 : 광신도";
            classDescriptionText.text = "설명 : 거짓된 신을 믿는 자.\n패시브 : 꼭짓점 4칸을 공격한다.\n스킬 : 8칸 안에 사제를 가진 말이 있을 시 사제가 광신도로 변한다.\n제약조건 : 주위 8칸에 사제와 함께할 수 없다.";
        }
        else if (classSticker.classData.className == "Thief")
        {
            classNameText.text = "직업 : 도둑";
            classDescriptionText.text = "설명 : 날쌔고 훔치기를 잘한다.\n패시브 : 상자를 열 수 있다.\n스킬: 원하는 방향으로 1칸 이동한다.";
        }
        else if (classSticker.classData.className == "Preist")
        {
            classNameText.text = "직업 : 사제";
            classDescriptionText.text = "설명 : 신성한 존재.\n패시브 : 디버프에 걸리지 않는다.\n스킬 : 행동력을 1 회복한다.\n제약조건 : 주위 8칸에 악마와 함께할 수 없다.";
        }
        else if (classSticker.classData.className == "Painter")
        {
            classNameText.text = "직업 : 화가";
            classDescriptionText.text = "설명 : 그림을 그리는 예술가.\n패시브 : -\n스킬 : 게임판에서 한 칸을 선택하여 원하는 색으로 칠할 수 있다.";
        }
    }

    public void SetLocked()
    {
        classSprite.sprite = null;
        classNameText.text = "직업 : ???";
        classDescriptionText.text = "설명 : ???????\n패시브 : ?????\n스킬 : ?????";
    }
}
