using System.Collections;
using UnityEngine;

namespace PhoenixQuest
{
    public static class AnimatorUtility
    {
        public static IEnumerator WaitForState(this Animator animator, int layerIndex, string state)
        {
            while (!animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(state))
            {
                yield return null;
            }
        }
    }
}
