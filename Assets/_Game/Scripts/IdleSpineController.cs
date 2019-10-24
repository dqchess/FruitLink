using System.Collections;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class IdleSpineController : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private SkeletonAnimation[] _skeletons;
    [SerializeField] private SkeletonGraphic _skeleton;
    [SerializeField] private string _winkAnimName = "idle_eye";
    [SerializeField] private float _min = 2.5f;
    [SerializeField] private float _max = 5f;
#pragma warning restore 649

    private void OnEnable()
    {
        StartCoroutine(WinkRoutine(_skeleton != null));
    }

    IEnumerator WinkRoutine(bool useGraphic)
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_min, _max));
            if(useGraphic) _skeleton.AnimationState.SetAnimation(2, _winkAnimName, false);
            else
            {
                for (int i = 0; i < _skeletons.Length; i++)
                {
                    _skeletons[i].AnimationState.SetAnimation(2, _winkAnimName, false);
                }
            }
        }
    }
}
