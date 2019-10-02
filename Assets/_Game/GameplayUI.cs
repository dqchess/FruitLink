using FruitSwipeMatch3Kit;
using TMPro;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    private static GameplayUI _instance;
    public static GameplayUI Instance => _instance;

    public GoalsWidget GoalsWidget;
    public ScoreWidget ScoreWidget;

    public TextMeshProUGUI MoveLeftText;
    

    private void Awake()
    {
        _instance = this;
    }
}
