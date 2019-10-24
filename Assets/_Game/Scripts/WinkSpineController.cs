using System;
using System.Collections;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class WinkSpineController : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private SkeletonGraphic _skeleton;
    [SerializeField] private string _winkAnimName = "idle_eye";
#pragma warning restore 649

    private void OnEnable()
    {
        StartCoroutine(WinkRoutine());
    }

    IEnumerator WinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(2.5f, 5f));
            _skeleton.AnimationState.SetAnimation(2, _winkAnimName, false);
        }
    }
}
