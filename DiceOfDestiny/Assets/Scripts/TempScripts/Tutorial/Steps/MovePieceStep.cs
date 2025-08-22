using UnityEngine;

namespace DoD.Tutorial
{
    /// <summary>
    /// "기물을 움직이는 법" 단계.
    /// 특정 타일까지 이동했는지 혹은 한번이라도 이동했는지로 완료 처리합니다.
    /// </summary>
    public sealed class MovePieceStep : TutorialStepBase
    {
        [SerializeField] private bool requireSpecificTile = false;
        [SerializeField] private Vector3Int targetTileCell;

        protected override void OnBeginInternal()
        {
            TutorialEvents.OnPieceMovedToTile += HandleMoved;
        }

        protected override void OnEndInternal()
        {
            TutorialEvents.OnPieceMovedToTile -= HandleMoved;
        }

        private void HandleMoved(Vector3Int cell)
        {
            if (!this.requireSpecificTile)
            {
                Complete();
                return;
            }

            if (cell == this.targetTileCell)
            {
                Complete();
            }
        }
    }
}
