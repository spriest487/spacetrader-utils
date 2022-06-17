using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SpaceTrader.Util {
    public enum TransitionStateDirection {
        In,
        Out,
    }

    public class TransitionStateMachineBehaviour : StateMachineBehaviour {
        [SerializeField]
        private TransitionStateDirection direction;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        private bool done;

        public event Action<TransitionStateDirection> StateCompleted;

        public TransitionStateDirection Direction => this.direction;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            this.done = false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (!this.done) {
                this.StateCompleted?.Invoke(this.direction);
                this.done = true;
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            base.OnStateUpdate(animator, stateInfo, layerIndex);
            if (this.done) {
                return;
            }

            if (stateInfo.speed < 0) {
                this.done = stateInfo.normalizedTime <= 0;
            } else {
                this.done = stateInfo.normalizedTime >= 1;
            }

            if (this.done) {
                this.StateCompleted?.Invoke(this.Direction);
            }
        }
    }
}
