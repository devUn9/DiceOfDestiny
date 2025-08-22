using UnityEngine;
using UnityEngine.SceneManagement;

using DoD.Persistence;

namespace DoD.Bootstrap
{
    /// <summary>
    /// 메인 메뉴의 "시작하기" 버튼에서 호출합니다.
    /// 최초 실행이면 튜토리얼 씬으로 이동하고, 아니면 현재 씬에서 기물 선택 패널만 엽니다.
    /// </summary>
    public sealed class FirstRunRouter : MonoBehaviour
    {
        [SerializeField] private string tutorialSceneName = "TutorialScene";
        [SerializeField] private GameObject pieceSelectPanelRoot;
        [SerializeField] private bool openPiecePanelAfterTutorial = true;

        private ITutorialFlagStore flagStore;
        private readonly string postTutorialOpenPanelKey = "dod.ui.open_piece_panel";

        private void Awake()
        {
            this.flagStore = new PlayerPrefsTutorialFlagStore();
        }

        /// <summary>
        /// UI 버튼에 바인딩하십시오.
        /// </summary>
        public void OnClickStart()
        {
            if (this.flagStore.GetHasCompletedTutorial())
            {
                OpenPieceSelectPanel();
                return;
            }

            if (this.openPiecePanelAfterTutorial)
            {
                PlayerPrefs.SetInt(this.postTutorialOpenPanelKey, 1);
                PlayerPrefs.Save();
            }

            if (!string.IsNullOrWhiteSpace(this.tutorialSceneName))
            {
                SceneManager.LoadScene(this.tutorialSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("[FirstRunRouter] tutorialSceneName 이 설정되지 않았습니다.");
            }
        }

        /// <summary>
        /// 메인 메뉴 씬이 로드될 때 자동으로 패널을 열어야 하는지 확인합니다.
        /// 메인 메뉴의 임의 GameObject에 본 스크립트를 붙여두고, SceneLoaded 이벤트에서 호출하세요
        /// (또는 메인 메뉴의 초기화 시점에 수동으로 호출).
        /// </summary>
        public void TryOpenPiecePanelIfRequested()
        {
            var shouldOpen = PlayerPrefs.GetInt(this.postTutorialOpenPanelKey, 0) == 1;
            if (!shouldOpen) return;

            PlayerPrefs.DeleteKey(this.postTutorialOpenPanelKey);
            PlayerPrefs.Save();
            OpenPieceSelectPanel();
        }

        private void OpenPieceSelectPanel()
        {
            if (this.pieceSelectPanelRoot == null)
            {
                Debug.LogError("[FirstRunRouter] pieceSelectPanelRoot 할당 필요");
                return;
            }

            this.pieceSelectPanelRoot.SetActive(true);
        }
    }
}
