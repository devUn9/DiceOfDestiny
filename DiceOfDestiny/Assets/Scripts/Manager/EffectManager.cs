using System.Collections.Generic;
using UnityEngine;

public class EffectManager : Singletone<EffectManager>
{
    [SerializeField] private GameObject[] effectPrefab;
    Dictionary<string, GameObject> effectDic = new Dictionary<string, GameObject>();

    private void Start()
    {
        foreach (var effect in effectPrefab)
        {
            effectDic.Add(effect.name, effect);
        }
    }

    public void PlayEffect(string effectName, Vector3 position, Vector2Int dir, float duration = 1.0f)
    {
        if (effectDic.TryGetValue(effectName, out var effectPrefab))
        {
            var effect = Instantiate(effectPrefab, position, Quaternion.identity);
            if(effect.GetComponent<WindTest>() != null)
                effect.GetComponent<WindTest>().MoveDirection(dir, duration);
            Destroy(effect, duration);
        }
        else
        {
            Debug.LogError($"Effect '{effectName}' not found in EffectManager.");
        }
    }
}