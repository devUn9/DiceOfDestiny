using UnityEngine;

namespace DoD.Persistence
{
    public interface ITutorialFlagStore
    {
        bool GetHasCompletedTutorial();
        void SetHasCompletedTutorial(bool hasCompleted);
        void ClearFlag();
    }

    /// <summary>
    /// 단순 1회성 튜토리얼 플래그 저장. AES/JSON 저장 시스템 준비 전까지 임시 사용.
    /// 나중에 교체할 수 있도록 ITutorialFlagStore 인터페이스를 사용합니다.
    /// </summary>
    public sealed class PlayerPrefsTutorialFlagStore : ITutorialFlagStore
    {
        // 상수 사용을 지양하는 사용자의 원칙을 고려하여 readonly 필드로 처리합니다.
        private readonly string completedKey = "dod.tutorial.completed";

        public bool GetHasCompletedTutorial()
        {
            return PlayerPrefs.GetInt(this.completedKey, 0) == 1;
        }

        public void SetHasCompletedTutorial(bool hasCompleted)
        {
            PlayerPrefs.SetInt(this.completedKey, hasCompleted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void ClearFlag()
        {
            PlayerPrefs.DeleteKey(this.completedKey);
        }
    }
}
