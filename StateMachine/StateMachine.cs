using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class StateMachine<T> where T : IStateMachineState<T> {
        private readonly Stack<T> stack;
        private readonly T defaultState;

        public T Current => this.stack.Count == 0 ? this.defaultState : this.stack.Peek();

        public event Action<StateTransition<T>> StateChanged;

        public StateMachine(T defaultState) {
            this.defaultState = defaultState;
            this.stack = new Stack<T>(4);
        }

        public void Push(T newState) {
            var suspended = this.stack.Count > 0 ? this.stack.Peek() : this.defaultState;
            suspended.Suspend(newState);

            this.stack.Push(newState);
            newState.Enter(suspended);
            newState.Restore(suspended);

            this.StateChanged?.Invoke(new StateTransition<T>(suspended, newState, StateTransitionKind.Push));
        }

        public void Replace(T newState) {
            var replaced = this.stack.Count > 0 ? this.stack.Pop() : this.defaultState;
            replaced.Suspend(newState);
            replaced.Exit(newState);

            this.stack.Push(newState);
            newState.Enter(replaced);
            newState.Restore(replaced);

            this.StateChanged?.Invoke(new StateTransition<T>(replaced, newState, StateTransitionKind.Replace));
        }

        public void Pop() {
            if (this.stack.Count == 0) {
                Debug.LogError("can't pop the empty state");
                return;
            }

            var popped = this.stack.Pop();
            var restored = this.stack.Count > 0 ? this.stack.Peek() : this.defaultState;

            popped.Suspend(restored);
            popped.Exit(restored);

            restored.Restore(popped);

            this.StateChanged?.Invoke(new StateTransition<T>(popped, restored, StateTransitionKind.Pop));
        }

        public void Reset(T newState) {
            if (this.stack.Count == 0) {
                this.Push(newState);
                return;
            }

            while (this.stack.Count > 1) {
                this.Pop();
            }

            this.Replace(newState);
        }
    }
}