using UnityEngine;
using DG.Tweening;

public class WindTest : MonoBehaviour
{
    public void EndEffect()
    {
        Destroy(this);
    }

    public void MoveDirection(Vector2Int dir, float duration = 1.0f)
    {
        transform.DOMove(transform.position + new Vector3(dir.x*30, dir.y*30, 0), duration).SetEase(Ease.Linear).OnComplete(EndEffect);
    }
}
