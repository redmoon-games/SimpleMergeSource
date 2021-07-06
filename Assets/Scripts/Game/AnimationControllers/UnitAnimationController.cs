using System;
using UnityEngine;

namespace Game.AnimationControllers
{
    public class UnitAnimationController : StateMachineBehaviour
    {
        public event Action UnitSpawned;

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(stateInfo.IsName("UnitSpawn"))
            {
                UnitSpawned?.Invoke();
            }
        }
    }
}
