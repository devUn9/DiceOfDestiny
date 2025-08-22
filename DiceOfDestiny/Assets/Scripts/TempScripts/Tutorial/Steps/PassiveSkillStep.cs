using UnityEngine;

namespace DoD.Tutorial
{
    /// <summary>
    /// "윗면 고유 패시브 발동" 단계.
    /// 지정된 윗면 타입의 패시브가 발동되면 완료.
    /// </summary>
    public sealed class PassiveSkillStep : TutorialStepBase
    {
        [SerializeField] private PieceFaceType requiredFace = PieceFaceType.Top;

        protected override void OnBeginInternal()
        {
            TutorialEvents.OnPassiveTriggered += HandlePassive;
        }

        protected override void OnEndInternal()
        {
            TutorialEvents.OnPassiveTriggered -= HandlePassive;
        }

        private void HandlePassive(PieceFaceType face)
        {
            if (face == this.requiredFace)
            {
                Complete();
            }
        }
    }
}
