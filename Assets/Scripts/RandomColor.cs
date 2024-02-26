using UnityEngine;

public class RandomColor : MonoBehaviour
{
    private void Start()
    {
        GetComponent<SpriteRenderer>().color = OrbTypeUtility.TypeColors[Random.Range(1, 7)];
    }
}
