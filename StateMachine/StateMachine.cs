using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class StateMachine<T> where T : IStateMachineState<T> {
        private readonly Stack<T> stack;
        public T DefaultState { get; }

        public T Current => this.stack.Count == 0 ? this.DefaultState : this.stack.Peek();

        public event Action<StateTransition<T>> BeforeStateChange;
        public event Action<StateTransition<T>> StateChanged;

        public StateMachine(T defaultState) {
            this.DefaultState = defaultState;
            this.stack = new Stack<T>(4);

            defaultState?.Enter(default);
        }

        public void Push(T newState) {
            var suspended = this.stack.Count > 0 ? this.stack.Peek() : this.DefaultState;

            var transition = new StateTransition<T>(suspended, newState, StateTransitionKind.Push);
            this.BeforeStateChange?.Invoke(transition);

            suspended.Suspend(newState);

            this.stack.Push(newState);
            newState.Enter(suspended);
            newState.Restore(suspended);

            this.StateChanged?.Invoke(transition);
        }

        public void Replace(T newState) {
            var replaced = this.stack.Count > 0 ? this.stack.Pop() : this.DefaultState;

            var transition = new StateTransition<T>(replaced, newState, StateTransitionKind.Replace);

            this.BeforeStateChange?.Invoke(transition);

            replaced.Suspend(newState);
            replaced.Exit(newState);

            this.stack.Push(newState);
            newState.Enter(replaced);
            newState.Restore(replaced);

            this.StateChanged?.Invoke(transition);
        }

        public void Pop() {
            if (this.stack.Count == 0) {
                Debug.LogError("can't pop the empty state");
                return;
            }

            var popped = this.stack.Pop();
            var restored = this.stack.Count > 0 ? this.stack.Peek() : this.DefaultState;

            var transition = new StateTransition<T>(popped, restored, StateTransitionKind.Pop);

            this.BeforeStateChange?.Invoke(transition);

            popped.Suspend(restored);
            popped.Exit(restored);

            restored.Restore(popped);

            this.StateChanged?.Invoke(transition);
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
