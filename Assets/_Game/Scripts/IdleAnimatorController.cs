using System.Collections;
using UnityEngine;

public class IdleAnimatorController : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private Animator _animator;
    [SerializeField] private string _triggerName = "Flash";
    [SerializeField] private float _minDelay = 10f;
    [SerializeField] private float _maxDelay = 20f;
#pragma warning restore 649

    private void OnEnable()
    {
        StartCoroutine(IdleRoutine());
    }

    IEnumerator IdleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minDelay, _maxDelay));
            _animator.SetTrigger(_triggerName);
        }
    }
}
