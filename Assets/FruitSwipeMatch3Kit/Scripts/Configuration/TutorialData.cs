using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "TutorialData", menuName = "Fruit Swipe Match 3 Kit/Tutorial", order = 1)]
public class TutorialData : ScriptableObject
{
    public List<int> IdxList;
    public GameObject Hand;
}