using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
enum FontOptions
{
    Option1,
    Option2, 
    Option3
}
public class MainController : MonoBehaviour
{
    #region initSettings
    const string ColorPropertyName = "_BaseColor";
    //배경 이미지 색
    [Header("배경 관련 설정")]
    [SerializeField] Image backGroundImage;
    [SerializeField] Color backGroundColor;
    Material backGroundMat;
    [SerializeField] FontOptions _fontOption = FontOptions.Option2;
    [SerializeField] TMP_FontAsset[] Fonts;
    TMP_FontAsset recentFont;
    TextMeshProUGUI[] Tmps;

    //색변경후 버튼을 누르면 적용되게
    [Space][Space]

    [Header("주사위 애니메이션")]
    //다이스 둥둥 애니메이션
    [Header("- 큰주사위")]
    [SerializeField] RectTransform 
        bigDice;
    [SerializeField] bool canBigRotate;
    [SerializeField] float bigRotate;
    float initBigY;
    [Header("- 작은 주사위")]
    [SerializeField] RectTransform smallDice;
    [SerializeField] bool canSmallRotate;
    [SerializeField] float smallRotate;
    float initSmallY;

    [Header("- 공통")]
    [SerializeField] float UpY;
    [SerializeField] float DownY;
    [SerializeField] float duration;
    [SerializeField] float animeSpeed;

    [Space][Space]

    [Header("버튼 설정")]
    //Button시스템
    [SerializeField] GameObject pieceSelectPanel;
    [SerializeField] Button gameStart;
    [SerializeField] Button continueGame;
    [SerializeField] Button createItem;
    [SerializeField] Button settingsButton;
    [SerializeField] Button gameExit;

    [Header("씬 설정")]
    [SerializeField] string gamePlayScene;
    [SerializeField] string createItemScene;
    [SerializeField] string tutorialScene;

    [Header("튜토리얼 확인 모달")]
    [SerializeField] private GameObject tutorialChoicePanel;
    [SerializeField] private Button tutorialProceedButton;
    [SerializeField] private Button tutorialSkipButton;
    [SerializeField] private Toggle dontAskAgainToggle;

    private const string KeyLastScene = "LastScene";
    private const string DefaultScene = "GameScene_2.1";
    private const string KeyFirstRun = "IsFirstRun"; // 추가: 최초 실행 키
    #endregion

    private void OnEnable()
    {
        if(backGroundImage == null)
        {
            Debug.LogError("배경의 이미지 컴포넌트가 할당 되지 않았습니다!");
        }
        backGroundMat = backGroundImage.material;
        backGroundMat.SetColor(ColorPropertyName,backGroundColor);

        initBigY = bigDice.anchoredPosition.y;
        initSmallY = smallDice.anchoredPosition.y;

        Button[] buttons = {gameStart, continueGame, createItem, settingsButton, gameExit};
        UnityAction[] actions = { OnPlayGameButton, OnContinueGameButton, OnCreateItemButton, OnSettingsButton, OnExitGameButton };

        for(int i = 0; i < buttons.Length; i++)
        {
            SetButton(buttons[i], actions[i]);
        }

        #region FontSetting
        Tmps = GetComponentsInChildren<TextMeshProUGUI>();
        if (Fonts.Length != 3)
        {
            Debug.LogError("폰트의 계수가 부족합니다 더 할당해 주세요");
            return;
        }
        switch (_fontOption)
        {
            case FontOptions.Option1:
                recentFont = Fonts[0];
                break;
            case FontOptions.Option2:
                recentFont = Fonts[1];
                break;
            case FontOptions.Option3:
                recentFont = Fonts[2];
                break;
        }

        foreach (TextMeshProUGUI tmp in Tmps)
        {
            tmp.font = recentFont;
        }
        #endregion

        StartCoroutine(StartBigDiceAnimation());
        StartCoroutine(StartSmallDiceAnimation());
        AudioManager.Instance.PlayBGM("MainBGM");

        // 튜토리얼 확인 모달 버튼 바인딩
        if (tutorialProceedButton != null) tutorialProceedButton.onClick.AddListener(OnChooseTutorialProceed);
        if (tutorialSkipButton != null) tutorialSkipButton.onClick.AddListener(OnChooseTutorialSkip);
    }

    private void ShowTutorialChoicePanel()
    {
        if (tutorialChoicePanel == null)
        {
            Debug.LogError("tutorialChoicePanel is not assigned!");
            // 폴백: 기존 흐름 유지
            ShowPieceSelectPanel(); // 기존 패널 오픈 로직  :contentReference[oaicite:6]{index=6}
            return;
        }
        tutorialChoicePanel.SetActive(true);
    }

    private void CloseTutorialChoicePanel()
    {
        if (tutorialChoicePanel != null)
            tutorialChoicePanel.SetActive(false);
    }

    private void SaveDontAskAgainIfChecked()
    {
        if (dontAskAgainToggle != null && dontAskAgainToggle.isOn)
        {
            PlayerPrefs.SetInt(KeyFirstRun, 0); // 다음부터 묻지 않기
        }
    }

    private void OnChooseTutorialProceed()
    {
        SaveDontAskAgainIfChecked();
        if (string.IsNullOrEmpty(tutorialScene))
        {
            Debug.LogError("튜토리얼 씬 이름이 설정되지 않았습니다!");
            CloseTutorialChoicePanel();
            ShowPieceSelectPanel();
            return;
        }
        SceneManager.LoadScene(tutorialScene); // S1 씬 이름을 할당해 주세요.
    }

    private void OnChooseTutorialSkip()
    {
        SaveDontAskAgainIfChecked();
        CloseTutorialChoicePanel();
        ShowPieceSelectPanel(); // 기존 기물 선택 패널 열기  :contentReference[oaicite:7]{index=7}
    }

    private void SetButton(Button btn, UnityAction e)//버튼과 이벤트가 비어있는지 확인하고 문제 없다면 설정
    {
        if(btn == null)
        {
            Debug.LogError("버튼이 할당되지 않았습니다!!");
            return;
        }
        if(e == null)
        {
            Debug.LogError($"{btn.name}버튼에 이벤트가 할당 되지 않았습니다!!");
            return;
        }
        btn.onClick.AddListener(e);
    }
    //다이스 애니메이션
    IEnumerator StartBigDiceAnimation()
    {
        int frame = Mathf.RoundToInt(duration * animeSpeed);

        float StartRot = 45f;

        Vector2 startVec = bigDice.anchoredPosition;
        Vector2 upPos = startVec + new Vector2(0, UpY);
        Vector2 downPos = startVec + new Vector2(0, -DownY);

        while (true)
        {
            for (float i = 0; i <= frame; i += 0.5f)
            {

                bigDice.anchoredPosition = Vector2.Lerp(startVec, upPos, i / frame);

                if (canBigRotate)
                {
                    float rot = Mathf.Lerp(StartRot, StartRot + bigRotate, i / frame);
                    bigDice.localRotation = Quaternion.Euler(0, 0, rot);
                }

                yield return new WaitForSeconds(1 / animeSpeed);
            }

            bigDice.anchoredPosition = upPos;


            for (float i = 0; i <= frame; i += 0.5f)
            {
                bigDice.anchoredPosition = Vector2.Lerp(upPos, downPos, i / frame);

                yield return new WaitForSeconds(1 / animeSpeed);
            }

            bigDice.anchoredPosition = downPos;

            for (float i = 0; i <= frame; i += 0.5f)
            {
                bigDice.anchoredPosition = Vector2.Lerp(downPos, startVec, i / frame);
                yield return new WaitForSeconds(1 / animeSpeed);
            }

            bigDice.anchoredPosition = startVec;
        }
    }//큰 주사위의 애니메이션을 담당하는 코루틴
    IEnumerator StartSmallDiceAnimation()
    {
        int frame = Mathf.RoundToInt(duration * animeSpeed);

        float StartRot = 0f;

        Vector2 startVec = smallDice.anchoredPosition;
        Vector2 upPos = startVec + new Vector2(0, UpY);
        Vector2 downPos = startVec + new Vector2(0, -DownY);

        while (true)
        {
            for (float i = 0; i <= frame; i += 0.5f)
            {

                smallDice.anchoredPosition = Vector2.Lerp(startVec, upPos, i / frame);

                if (canSmallRotate)
                {
                    float rot = Mathf.Lerp(StartRot, StartRot + smallRotate, i / frame);
                    smallDice.localRotation = Quaternion.Euler(0, 0, rot);
                }

                yield return new WaitForSeconds(1 / animeSpeed);
            }

            smallDice.anchoredPosition = upPos;

            for (float i = 0; i <= frame; i += 0.5f)
            {
                smallDice.anchoredPosition = Vector2.Lerp(upPos, downPos, i / frame);

                yield return new WaitForSeconds(1 / animeSpeed);
            }

            smallDice.anchoredPosition = downPos;

            for (float i = 0; i <= frame; i += 0.5f)
            {
                smallDice.anchoredPosition = Vector2.Lerp(downPos, startVec, i / frame);
                yield return new WaitForSeconds(1 / animeSpeed);
            }

            smallDice.anchoredPosition = startVec;
        }
    }//작은 주사위의 애니메이션을 담당하는 코루틴

    //버튼 시스템 구현
    #region ButtonSystem
    private void ShowPieceSelectPanel()
    {
        pieceSelectPanel.SetActive(true);
        pieceSelectPanel.GetComponent<PieceSelectPanelUI>().Intitialize(() => SceneManager.LoadScene(gamePlayScene));
    }
    private void OnPlayGameButton()
    {
        Debug.Log("게임 시작");
        // 최초 실행이면 '확인 모달'을 띄우고, 선택에 따라 진행/스킵
        if (PlayerPrefs.GetInt(KeyFirstRun, 1) == 1)
        {
            ShowTutorialChoicePanel();
            return;
        }

        // 최초 실행이 아니면 기존 흐름
        if (string.IsNullOrEmpty(gamePlayScene))
        {
            Debug.LogError("게임 시작 씬 이름이 비어있습니다!!");
            return;
        }
        ShowPieceSelectPanel();
    }//게임 시작

    private void OnContinueGameButton()
    {
        string lastScene = PlayerPrefs.GetString(KeyLastScene, DefaultScene);
        SceneManager.LoadScene(lastScene);
    }

    private void OnCreateItemButton()
    {
        Debug.Log("기물 제작");
        if(createItemScene == "")
        {
            Debug.LogError("기물 제작을 눌렀을때 바뀔 씬의 이름이 설정되지 않았습니다!!");
            return;
        }
        SceneManager.LoadScene(createItemScene);
    }//기물 제작

    private void OnSettingsButton()
    {
        UIManager.Instance?.ToggleSettings(true);
    }//세팅 UI

    private void OnExitGameButton()//게임 종료
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    #endregion
    //옵션 제작?

}
