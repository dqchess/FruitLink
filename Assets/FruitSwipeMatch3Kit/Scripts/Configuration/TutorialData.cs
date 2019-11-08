using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialType
{
    Move,
    Crusher,
    Bomb,
    Swap,
    ColorBomb
}

[Serializable]
[CreateAssetMenu(fileName = "TutorialData", menuName = "Fruit Swipe Match 3 Kit/Tutorial", order = 1)]
public class TutorialData : ScriptableObject
{
    public TutorialType TutorialType = TutorialType.Move;
    public List<int> IdxList;
    public List<int> IdxStep2;
    public GameObject Hand;
}