using UnityEngine;

public class RookArrow : MonoBehaviour
{
    private Vector2 dir;
    [SerializeField] private float speed = 5f;

    private void Start()
    {
        Destroy(this.gameObject, 3f);
    }

    private void Update()
    {
        transform.position += (Vector3)dir * speed * Time.deltaTime;
    }
    public void Init(Vector2Int _direction)
    {
        dir = _direction;
    }
}