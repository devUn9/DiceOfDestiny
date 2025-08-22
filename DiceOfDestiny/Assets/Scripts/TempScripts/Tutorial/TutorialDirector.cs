using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using DoD.Persistence;

namespace DoD.Tutorial
{
    /// <summary>
    /// 튜토리얼 단계의 흐름을 제어합니다.
    /// 씬에 하나 배치하고, 단계 컴포넌트를 순서대로 등록합니다.
    /// </summary>
    public sealed class TutorialDirector : MonoBehaviour
    {
        [Header("Steps (등록 순서대로 진행)")]
        [SerializeField] private List<MonoBehaviour> stepBehaviours = new();

        [Header("UI")]
        [SerializeField] private TutorialUI tutorialUI;

        [Header("메인 메뉴 복귀")]
        [SerializeField] private string mainMenuSceneName = "MainScene";
        [SerializeField] private bool openPiecePanelOnReturn = true;

        private readonly Queue<ITutorialStep> stepQueue = new();
        private ITutorialStep current;
        private ITutorialFlagStore flagStore;
        private readonly string postTutorialOpenPanelKey = "dod.ui.open_piece_panel";

        private void Awake()
        {
            this.flagStore = new PlayerPrefsTutorialFlagStore();
            BuildQueue();
        }

        private void Start()
        {
            MoveNext();
        }

        private void BuildQueue()
        {
            this.stepQueue.Clear();
            foreach (var m in this.stepBehaviours)
            {
                if (m is ITutorialStep step)
                {
                    this.stepQueue.Enqueue(step);
                }
                else
                {
                    Debug.LogWarning($"[TutorialDirector] {m.name} 은(는) ITutorialStep 이 아닙니다.");
                }
            }
        }

        private void MoveNext()
        {
            if (this.current != null)
            {
                this.current.OnStepCompleted -= HandleStepCompleted;
                this.current.EndStep();
            }

            if (this.stepQueue.Count == 0)
            {
                CompleteTutorial();
                return;
            }

            this.current = this.stepQueue.Dequeue();
            this.current.OnStepCompleted += HandleStepCompleted;
            this.tutorialUI?.Show(this.current.Title, this.current.Instruction);
            this.current.BeginStep();
        }

        private void HandleStepCompleted(ITutorialStep step)
        {
            MoveNext();
        }

        private void CompleteTutorial()
        {
            this.flagStore.SetHasCompletedTutorial(true);

            if (this.openPiecePanelOnReturn)
            {
                PlayerPrefs.SetInt(this.postTutorialOpenPanelKey, 1);
                PlayerPrefs.Save();
            }

            if (!string.IsNullOrWhiteSpace(this.mainMenuSceneName))
            {
                SceneManager.LoadScene(this.mainMenuSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError("[TutorialDirector] mainMenuSceneName 이 설정되지 않았습니다.");
            }
        }

        public void SkipTutorial()
        {
            CompleteTutorial();
        }
    }
}
