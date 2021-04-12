using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public delegate void StateTransitionDelegate<T>(in StateTransition<T> transition);

    public class StateMachine<T> where T : IStateMachineState<T> {
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
        [Sirenix.OdinInspector.ListDrawerSettings(IsReadOnly = true)]
#endif
        private readonly Stack<T> stack;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public T DefaultState { get; }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public T Current => this.stack.Count == 0 ? this.DefaultState : this.stack.Peek();

        public event StateTransitionDelegate<T> BeforeStateChange;
        public event StateTransitionDelegate<T> StateChanged;

        public StateMachine(T defaultState) {
            this.DefaultState = defaultState;
            this.stack = new Stack<T>(4);

            if (defaultState != null) {
                defaultState.Enter(default);
                defaultState.Restore(default);
            }
        }

        public void Push(T newState) {
            var suspended = this.stack.Count > 0 ? this.stack.Peek() : this.DefaultState;

            var transition = new StateTransition<T>(suspended, newState, StateTransitionKind.Push);
            this.BeforeStateChange?.Invoke(in transition);

            SuspendState(suspended, newState);

            this.stack.Push(newState);
            EnterState(newState, suspended);
            RestoreState(newState, suspended);

            this.StateChanged?.Invoke(in transition);
        }

        public void Replace(T newState) {
            var replaced = this.stack.Count > 0 ? this.stack.Pop() : this.DefaultState;

            var transition = new StateTransition<T>(replaced, newState, StateTransitionKind.Replace);

            this.BeforeStateChange?.Invoke(in transition);

            SuspendState(replaced, newState);
            ExitState(replaced, newState);

            this.stack.Push(newState);
            EnterState(newState, replaced);
            RestoreState(newState, replaced);

            this.StateChanged?.Invoke(in transition);
        }

        public void Pop() {
            if (this.stack.Count == 0) {
                Debug.LogError("can't pop the empty state");
                return;
            }

            var popped = this.stack.Pop();
            var restored = this.stack.Count > 0 ? this.stack.Peek() : this.DefaultState;

            var transition = new StateTransition<T>(popped, restored, StateTransitionKind.Pop);

            this.BeforeStateChange?.Invoke(in transition);

            SuspendState(popped, restored);
            ExitState(popped, restored);

            RestoreState(restored, popped);

            this.StateChanged?.Invoke(in transition);
        }

        private static void EnterState(T state, T fromState) {
            try {
                state?.Enter(fromState);
            } catch (Exception ex) {
                Debug.LogErrorFormat("State Enter transition failed: {0}", ex);
            }
        }

        private static void RestoreState(T state, T fromState) {
            try {
                state?.Restore(fromState);
            } catch (Exception ex) {
                Debug.LogErrorFormat("State Restore transition failed: {0}", ex);
            }
        }

        private static void SuspendState(T state, T toState) {
            try {
                state?.Suspend(toState);
            } catch (Exception ex) {
                Debug.LogErrorFormat("State Suspend transition failed: {0}", ex);
            }
        }

        private static void ExitState(T state, T toState) {
            try {
                state?.Exit(toState);
            } catch (Exception ex) {
                Debug.LogErrorFormat("State Exit transition failed: {0}", ex);
            }
        }

        public void PopWhile(Func<T, bool> predicate) {
            while (predicate(this.Current) && this.stack.Count > 0) {
                this.Pop();
            }
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
