using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public class StickerDetailUI : MonoBehaviour
{
    public Image classSprite;
    public Image lockImage;
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
        lockImage.gameObject.SetActive(false);
        classDescriptionText.alignment = TextAlignmentOptions.TopLeft;
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
        else if(classSticker.classData.className == "Berserker")
        {
            classNameText.text = "직업 : 광전사";
            classDescriptionText.text = "설명 : 전투에 능하지만 어리석음.\n패시브 : 주변 8칸의 적을 공격하고 처치 시 돌진함.\n스킬 : 한 턴 동안 기절함.";
        }
        else if(classSticker.classData.className == "Logger")
        {
            classNameText.text = "직업 : 나무꾼";
            classDescriptionText.text = "설명 : 나무를 베는 사람.\n패시브 : 전방 1칸의 나무를 부숨.\n스킬 : 원하는 한 칸에 나무 방벽을 설치함.";
        }
        else if (classSticker.classData.className == "Wizard")
        {
            classNameText.text = "직업 : 마법사";
            classDescriptionText.text = "설명 : 시공간 마법을 깨우친 자.\n패시브 : -\n스킬 : 자신을 포함한 두 기물의 위치를 뒤바꾸고, 다른 직업의 스킬을 사용할 수 있음.";
        }
    }

    public void SetLocked()
    {
        classSprite.sprite = null;
        lockImage.gameObject.SetActive(true);
        classNameText.text = "직업 : ???";
        classDescriptionText.text = "해금이 필요합니다.";
        classDescriptionText.alignment = TextAlignmentOptions.Center;
        classDescriptionText.alignment = TextAlignmentOptions.Midline;
    }
}
