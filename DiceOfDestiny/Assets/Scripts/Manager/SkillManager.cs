using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SkillManager : Singletone<SkillManager>
{
    [SerializeField] public float blinkTime = 1.5f;
    [SerializeField] private PieceController currentPiece;

    private ActiveSkill activeSkill;
    private PassiveSkill passiveSkill;

    public float DelayTime { get; set; } = 0f;

    public bool IsSelectingProgress { get; set; } = false;

    private void Start()
    {
        activeSkill = GetComponent<ActiveSkill>();
        passiveSkill = GetComponent<PassiveSkill>();
    }

    // 패시브와 액티브 스킬을 순차적으로 실행
    public void TrySkill(Vector2Int position, PieceController piece)
    {
        StartCoroutine(TrySkillSequence(position, piece));
    }
    private IEnumerator TrySkillSequence(Vector2Int position, PieceController piece)
    {
        // 1. 패시브 스킬 실행 및 완료 대기
        yield return StartCoroutine(TryPassiveSkillCoroutine(position, piece));

        yield return new WaitForSeconds(DelayTime); // 패시브 스킬이 완료될 때까지 대기, 사제와 도둑 패시브에 사용

        // 2. 패시브 스킬 완료 후 액티브 스킬 실행
        yield return StartCoroutine(TryActiveSkillCoroutine(position, piece));
    }

    // 패시브 (공격) 코루틴
    private IEnumerator TryPassiveSkillCoroutine(Vector2Int position, PieceController piece)
    {
        ClassData classData = piece.GetTopFace().classData;

        switch (classData.className)
        {

            case "Baby":
                // 아기 패시브 로직

                break;

            case "Demon":
                // 악마 패시브 로직
                StartCoroutine(passiveSkill.DemonAttack(piece));

                break;
            case "Fanatic":
                // 광신도 패시브 로직
                StartCoroutine(passiveSkill.FanaticAttack(piece));

                break;
            case "Knight":
                // 기사 패시브 로직
                StartCoroutine(passiveSkill.KnightAttack(piece));

                break;
            case "Priest":
                // 사제 패시브 로직


                break;
            case "Thief":
                // 도둑 패시브 로직

                break;
            case "Painter":
                // 화가 패시브 로직

                break;

            case "Logger":
                // 나무꾼 패시브 로직
                StartCoroutine(passiveSkill.CutDownTree(piece));
                
                break;

            case "Berserker":
                // 광전사 패시브 로직
                StartCoroutine(passiveSkill.BerserkerAttack(piece));

                break;

            default:
                break;
        }

        yield return null;
    }

    public void PriestPassive()
    {
        StartCoroutine(passiveSkill.Halo());
        RuleEvents.TriggerRule("Priest_Passive_AllDebuffImmune");
    }

    public void ThiefPassive()
    {
        StartCoroutine(passiveSkill.Steal());
    }

    public void FanaticMeteor()
    {
        StartCoroutine(passiveSkill.DoFanaticMeteor());
    }

    // 스킬 사용 가능한지 판단하는 코루틴
    public IEnumerator TryActiveSkillCoroutine(Vector2Int position, PieceController piece)
    {
     

        bool isDdongBlind = false;

        // 주변 8칸 중 상단 컬러와 일치하는 칸 수 확인
        int matchCount = BoardManager.Instance.CountMatchingColors(position, piece.GetTopFace().color, ref isDdongBlind);
        if (matchCount >= 3)
        {
            if (isDdongBlind)
                RuleEvents.TriggerRule("Slime_Ddong_BlindColor");
                
            
            List<Vector2Int> matchingTile = BoardManager.Instance.GetMatchingColorTiles(position, piece.GetTopFace().color);

            StartCoroutine(SkillEffectCoroutine(piece.colorRenderer, position, matchingTile,piece));

            DoActiveSkill(piece.GetTopFace().classData);

            GameManager.Instance.IsLockCursor = true;

            yield return new WaitForSeconds(blinkTime);
           
        }


    }

    // 스킬 발동
    private void DoActiveSkill(ClassData classData)
    {
        currentPiece = PieceManager.Instance.currentPiece;

        switch (classData.className)
        {
            case "Baby":
                // 아기 스킬 : 원하는 말 한 칸 이동
                StartCoroutine(activeSkill.HelpBaby(currentPiece));

                ToastManager.Instance.ShowToast("아기 스킬 발동! 원하는 말을 한 칸 이동합니다.", currentPiece.transform, 0f); // 나중에 스킬 메서드 생기면 그리로 이동
                RuleEvents.TriggerRule("Baby_Active_ColorMatch");

                break;

            case "Demon":
                // 악마 스킬 : 원하는 보드 한칸에 독초 장애물을 만듬

                StartCoroutine(activeSkill.Plant(currentPiece));
                ToastManager.Instance.ShowToast("악마 스킬 발동! 원하는 보드 한 칸에 독초 장애물을 만듭니다.", currentPiece.transform, 0f);
                RuleEvents.TriggerRule("Demon_Active_ColorMatch");

                break;

            case "Fanatic":
                // 광신도 스킬 : 주변에 있는 사제를 광신도로 만듬

                StartCoroutine(activeSkill.ConvertToFanatic(currentPiece));
                ToastManager.Instance.ShowToast("광신도 스킬 발동! 주변에 있는 사제를 광신도로 만듭니다.", currentPiece.transform, 0f);
                RuleEvents.TriggerRule("Fanatic_Active_ColorMatch");

                break;

            case "Knight":
                // 기사 스킬 : 진행했던 방향으로 1칸 움직임, 다 부숨

                Vector2Int lastDirection = currentPiece.GetLastMoveDirection();

                StartCoroutine(activeSkill.KnightMoveForward(currentPiece, lastDirection));
                ToastManager.Instance.ShowToast("기사 스킬 발동! 기사 앞에 있는 모든 장애물을 제거합니다.", currentPiece.transform, 0f);
                RuleEvents.TriggerRule("Knight_Active_ColorMatch");

                BoardManager.Instance.Board[currentPiece.gridPosition.x, currentPiece.gridPosition.y].SetPiece(null);
                Vector2Int newPosition = currentPiece.gridPosition + lastDirection;

                if (!BoardManager.Instance.IsInsideBoard(newPosition))
                    return;
                BoardManager.Instance.Board[newPosition.x, newPosition.y].SetPiece(currentPiece);

                break;

            case "Priest":
                // 사제 스킬 : 행동력을 1 추가

                ActionPointManager.Instance.AddAP(1);
                StartCoroutine(activeSkill.HealAP());
                ToastManager.Instance.ShowToast($"사제 스킬 발동! AP를 추가로 1만큼 더 얻습니다.", currentPiece.transform, 0f);
                RuleEvents.TriggerRule("Priest_Active_ColorMatch");

                break;

            case "Thief":
                // 도둑 스킬 : 원하는 방향으로 1칸 움직임, 컨트롤러 한번 더 띄움
                StartCoroutine(activeSkill.FastMove(currentPiece));
                ToastManager.Instance.ShowToast("도둑 스킬 발동! 원하는 방향으로 1칸 더 이동 가능해집니다.", currentPiece.transform, 0f);
                RuleEvents.TriggerRule("Thief_Active_ColorMatch");

                break;

            case "Painter":
                // 화가 스킬: 원하는 보드 한칸에 색깔을 칠함

                StartCoroutine(activeSkill.Paint(currentPiece));
                ToastManager.Instance.ShowToast("화가 스킬 발동! 원하는 보드 한 칸의 색상 변경합니다.", currentPiece.transform, 0f);
                RuleEvents.TriggerRule("Painter_Active_ColorMatch");

                break;

            case "Logger":
                // 나무꾼 스킬 : 주변 8칸 중 한 칸에 나무 장애물을 만듬
                StartCoroutine(activeSkill.CreateWoodBox());
                ToastManager.Instance.ShowToast("나무꾼 스킬 발동! 원하는 보드 한 칸에 나무 장애물을 만듭니다.", currentPiece.transform, 0f);
                //RuleEvents.TriggerRule("");
                break;

            case "Wizard":
                // 마법사 스킬 : 기물간 위치변환
                StartCoroutine(activeSkill.SwapPieces(currentPiece));
                break;

            case "Berserker":
                // 광전사 스킬 : 기절하기
                StartCoroutine(activeSkill.SelfStun(currentPiece));
                break;
            default:
                Debug.LogError($"알 수 없는 클래스 : {classData.className}");

                break;
        }
    }

    #region 스킬 발동 시 깜빡임, 보드 색상 재배치 코루틴

    private IEnumerator SkillEffectCoroutine(SpriteRenderer pieceRenderer, Vector2Int position, List<Vector2Int> matchingTiles,PieceController pieceController)
    {
        if (PieceManager.Instance == null /*|| PieceManager.Instance.GetPiece() == null*/)
        {
            Debug.LogError("PieceManager or Piece is null!");
            yield break;
        }

        // 스킬이 발동된 타일과 매칭된 타일들의 SpriteRenderer 수집
        List<(SpriteRenderer renderer, Color originalColor)> renderers = new List<(SpriteRenderer, Color)>();

        // 피스의 SpriteRenderer 추가
        if (pieceRenderer != null)
        {
            renderers.Add((pieceRenderer, pieceRenderer.color));
        }
        else
        {
            Debug.LogError("Piece SpriteRenderer is null!");
        }

        // 스킬이 발동된 타일의 SpriteRenderer 추가
        if (position.x >= 0 && position.x <= BoardManager.Instance.boardSize &&
            position.y >= 0 && position.y <= BoardManager.Instance.boardSize &&
            BoardManager.Instance.Board[position.x, position.y] != null)
        {
            SpriteRenderer tileRenderer = BoardManager.Instance.Board[position.x, position.y].GetComponent<SpriteRenderer>();
            if (tileRenderer != null)
            {
                renderers.Add((tileRenderer, tileRenderer.color));
            }
            else
            {
                Debug.LogError($"SpriteRenderer is null for tile at {position}");
            }
        }
        else
        {
            Debug.LogError("Invalid tile position or tile is null!");
        }

        // 매칭된 타일들의 SpriteRenderer 추가
        foreach (Vector2Int tilePos in matchingTiles)
        {
            //// tilePos가 null인 경우 무시하고 다음 반복으로 이동
            //if (tilePos == null)
            //{
            //    Debug.LogWarning($"tilePos is null, skipping...");
            //    continue;
            //}

            if (tilePos.x >= 0 && tilePos.x <= BoardManager.Instance.boardSize &&
                tilePos.y >= 0 && tilePos.y <= BoardManager.Instance.boardSize &&
                BoardManager.Instance.Board[tilePos.x, tilePos.y] != null)
            {
                SpriteRenderer tileRenderer = BoardManager.Instance.Board[tilePos.x, tilePos.y].GetComponent<SpriteRenderer>();
                if (tileRenderer != null)
                {
                    renderers.Add((tileRenderer, tileRenderer.color));
                }
                else
                {
                    Debug.LogError($"SpriteRenderer is null for tile at {tilePos}");
                }
            }
            else
            {
                Debug.LogError($"Invalid position or null tile at {tilePos}");
            }
        }

        // 깜빡임 효과
        float blinkInterval = 0.25f; // 1초에 4번 깜빡임
        int blinkCount = Mathf.FloorToInt(blinkTime / blinkInterval);
        float elapsed = 0f;

        for (int i = 0; i < blinkCount; i++)
        {
            // 모든 SpriteRenderer를 검정색으로 변경
            foreach (var (renderer, originalColor) in renderers)
            {
                renderer.color = Color.black;
            }
            yield return new WaitForSeconds(blinkInterval / 2);

            // 모든 SpriteRenderer를 원래 색상으로 복원
            foreach (var (renderer, originalColor) in renderers)
            {
                renderer.color = originalColor;
            }
            yield return new WaitForSeconds(blinkInterval / 2);

            elapsed += blinkInterval;
        }

        // 정확히 1초가 되도록 남은 시간 대기
        if (elapsed < blinkTime)
        {
            yield return new WaitForSeconds(blinkTime - elapsed);
        }

        // 최종적으로 모든 SpriteRenderer를 원래 색상으로 복원
        foreach (var (renderer, originalColor) in renderers)
        {
            renderer.color = originalColor;
        }

        BoardManager.Instance.ReassignMatchingColorTiles(position, pieceController.GetTopFace().color);

        
    }
    #endregion
}