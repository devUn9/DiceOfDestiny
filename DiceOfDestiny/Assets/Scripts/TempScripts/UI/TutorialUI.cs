using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoD.Tutorial
{
    /// <summary>
    /// 아주 단순한 오버레이 UI. 제목/설명/건너뛰기 버튼만 제공합니다.
    /// </summary>
    public sealed class TutorialUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup rootCanvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Button skipButton;

        private void Awake()
        {
            if (this.skipButton != null)
            {
                this.skipButton.onClick.AddListener(OnClickSkip);
            }
        }

        public void Show(string title, string body)
        {
            if (this.titleText != null) this.titleText.text = title;
            if (this.instructionText != null) this.instructionText.text = body;
            if (this.rootCanvasGroup != null)
            {
                this.rootCanvasGroup.alpha = 1f;
                this.rootCanvasGroup.blocksRaycasts = true;
                this.rootCanvasGroup.interactable = true;
            }
        }

        private void OnClickSkip()
        {
            var director = FindFirstObjectByType<TutorialDirector>();
            if (director != null)
            {
                director.SkipTutorial();
            }
        }
    }
}
