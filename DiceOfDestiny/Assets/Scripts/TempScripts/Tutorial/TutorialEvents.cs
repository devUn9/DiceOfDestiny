using System;
using UnityEngine;

namespace DoD.Tutorial
{
    /// <summary>
    /// 기존 시스템(이동/스킬/패시브)에서 튜토리얼을 향해 신호를 보낼 때 사용합니다.
    /// 예) 이동이 끝나면 NotifyPieceMovedToTile 호출.
    /// </summary>
    public static class TutorialEvents
    {
        public static event Action<Vector3Int> OnPieceMovedToTile;
        public static event Action<TileColor> OnActiveSkillCastOnColor;
        public static event Action<PieceFaceType> OnPassiveTriggered;

        public static void NotifyPieceMovedToTile(in Vector3Int boardCell)
        {
            OnPieceMovedToTile?.Invoke(boardCell);
        }

        public static void NotifyActiveSkillCastOnColor(TileColor color)
        {
            OnActiveSkillCastOnColor?.Invoke(color);
        }

        public static void NotifyPassiveTriggered(PieceFaceType faceType)
        {
            OnPassiveTriggered?.Invoke(faceType);
        }
    }

    public enum TileColor
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
        Yellow = 4,
        Purple = 5,
    }

    public enum PieceFaceType
    {
        Top = 0,
        Front = 1,
        Back = 2,
        Left = 3,
        Right = 4,
        Bottom = 5
    }
}
