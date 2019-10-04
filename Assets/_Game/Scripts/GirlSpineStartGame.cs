using Spine.Unity;
using UnityEngine;

public class GirlSpineStartGame : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField] private SkeletonGraphic _skeleton;
    [SerializeField] private string _armLeftAnim = "idle_armleft";
#pragma warning restore 649
    
    private void OnEnable()
    {
        _skeleton.AnimationState.AddAnimation(1, _armLeftAnim, false, 0.4f);
    }
}
