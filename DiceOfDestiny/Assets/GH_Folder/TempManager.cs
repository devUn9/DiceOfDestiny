using UnityEngine;

public class TempManager : Singletone<TempManager>
{
    [SerializeField] private GameObject NextStageUI;
    [SerializeField] private GameObject mainCanvasGroup;
    [SerializeField] private StageBannerManager bannerManager;

    private StageData currentStage = null;
    private Coroutine bannerRoutine;
    private void Awake()
    {
        
    }
    

    

    
}
