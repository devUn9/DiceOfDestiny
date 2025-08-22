using UnityEngine;

namespace DoD.Tutorial
{
    /// <summary>
    /// "타일 색을 이용해 윗면의 고유 액티브 스킬 발동" 단계.
    /// 지정한 타일 색에서 스킬을 1회 시전하면 완료.
    /// </summary>
    public sealed class ActiveSkillStep : TutorialStepBase
    {
        [SerializeField] private TileColor requiredColor = TileColor.Red;

        protected override void OnBeginInternal()
        {
            TutorialEvents.OnActiveSkillCastOnColor += HandleCast;
        }

        protected override void OnEndInternal()
        {
            TutorialEvents.OnActiveSkillCastOnColor -= HandleCast;
        }

        private void HandleCast(TileColor color)
        {
            if (color == this.requiredColor)
            {
                Complete();
            }
        }
    }
}
