using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace SpaceTrader.Util {
    [RequireComponent(typeof(Animator))]
    public class AnimatorEventTrigger : MonoBehaviour {
        [Serializable]
        private class Trigger {
            public string Key;
            public UnityEvent<AnimatorEventTrigger> Action;
        }

        [SerializeField]
        private List<Trigger> triggers;

        private void OnDestroy() {
            foreach (var trigger in this.triggers) {
                trigger.Action.RemoveAllListeners();
            }
        }

        public void AddListener(string key, UnityAction<AnimatorEventTrigger> action) {
            if (!this.GetTrigger(key, out var trigger)) {
                trigger = new Trigger {
                    Key = key,
                    Action = new UnityEvent<AnimatorEventTrigger>(),
                };

                this.triggers.Add(trigger);
            }
            
            trigger.Action.AddListener(action);
        }

        public void RemoveListener(string key, UnityAction<AnimatorEventTrigger> action) {
            if (this.GetTrigger(key, out var trigger)) {
                trigger.Action.RemoveListener(action);
            }
        }

        private bool GetTrigger(string key, [MaybeNullWhen(false)] out Trigger keyTrigger) {
            foreach (var trigger in this.triggers) {
                if (string.Equals(key, trigger.Key, StringComparison.Ordinal)) {
                    keyTrigger = trigger;
                    return true;
                }
            }

            keyTrigger = default;
            return false;
        }

        [UsedImplicitly]
        private void OnAnimationEvent(string key) {
            if (this.GetTrigger(key, out var trigger)) {
                trigger.Action.Invoke(this);
            }
        }
    }
}
