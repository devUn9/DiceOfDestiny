using UnityEngine;

namespace DoD.Bootstrap
{
    /// <summary>
    /// 메인 메뉴 씬 최초 진입 시, 튜토리얼 종료 후 패널 자동 열기 요청을 수행합니다.
    /// </summary>
    public sealed class MainMenuStartup : MonoBehaviour
    {
        [SerializeField] private FirstRunRouter firstRunRouter;

        private void Start()
        {
            if (this.firstRunRouter == null)
            {
                this.firstRunRouter = FindFirstObjectByType<FirstRunRouter>();
            }

            if (this.firstRunRouter != null)
            {
                this.firstRunRouter.TryOpenPiecePanelIfRequested();
            }
            else
            {
                Debug.LogWarning("[MainMenuStartup] FirstRunRouter 를 찾지 못했습니다.");
            }
        }
    }
}
