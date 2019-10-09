using FruitSwipeMatch3Kit;
using UnityEngine;

public class SelectionLine : MonoBehaviour
{
    #pragma warning disable 649
    [SerializeField] private SpriteRenderer _sprite;
    #pragma warning restore 649 
    public void SetColor(Color color)
    {
        _sprite.color = color;
    }
}
