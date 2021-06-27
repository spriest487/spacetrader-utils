using System;
using System.Collections.Generic;
using System.Reflection;

namespace SpaceTrader.Util {
    public enum RuleTransitionKind {
        None = default,
        Push,
        Pop,
        Replace,
        Reset,
    }

    internal struct RuleTransition<T> where T : IStateMachineState<T> {
        public T ToState { get; set; }
        public RuleTransitionKind TransitionKind { get; set; }
    }

    public class TransitionRuleStateMachine<T> : StateMachine<T> where T : IStateMachineState<T> {
        private delegate void TransitionRuleDelegate(T fromState, ref RuleTransition<T> transition);

        private readonly Dictionary<Type, List<TransitionRuleDelegate>> transitionRules;

        public TransitionRuleStateMachine(T defaultState) : base(defaultState) {
            this.transitionRules = new Dictionary<Type, List<TransitionRuleDelegate>>(8);

            this.BeforeStateChange += this.OnBeforeStateChange;
        }

        private void OnBeforeStateChange(in StateTransition<T> transition) {
            if (!(transition.NextState is {} nextState)) {
                return;
            }

            var stateType = nextState.GetType();
            if (this.transitionRules.TryGetValue(stateType, out var rulesList)) {
                return;
            }

            rulesList = new List<TransitionRuleDelegate>();

            const BindingFlags ruleMethodBindingFlags = BindingFlags.Instance
                | BindingFlags.InvokeMethod
                | BindingFlags.Public
                | BindingFlags.NonPublic;
            foreach (var method in stateType.GetMethods(ruleMethodBindingFlags)) {
                if (!(method.GetCustomAttribute<TransitionRuleAttribute>() is { } ruleAttribute)) {
                    continue;
                }

                switch (ruleAttribute.TransitionKind) {
                    case RuleTransitionKind.Pop: {
                        void ExecuteTransitionRuleMethod(T fromState, ref RuleTransition<T> ruleTransition) {
                            using var profilerSegment = new ProfilerSegment("TransitionRuleStateMachine - Execute Transition Rule");
                            var result = (bool)method.Invoke(fromState, null);
                            if (result) {
                                ruleTransition.TransitionKind = RuleTransitionKind.Pop;
                                ruleTransition.ToState = default;
                            }
                        }

                        rulesList.Add(ExecuteTransitionRuleMethod);
                        break;
                    }

                    case RuleTransitionKind.Push:
                    case RuleTransitionKind.Replace:
                    case RuleTransitionKind.Reset: {
                        void ExecuteTransitionRuleMethod(T fromState, ref RuleTransition<T> ruleTransition) {
                            using var profilerSegment = new ProfilerSegment("TransitionRuleStateMachine - Execute Transition Rule");

                            if (method.Invoke(fromState, null) is T toState) {
                                ruleTransition.TransitionKind = ruleAttribute.TransitionKind;
                                ruleTransition.ToState = toState;
                            }
                        }

                        rulesList.Add(ExecuteTransitionRuleMethod);
                        break;
                    }
                }
            }

            this.transitionRules.Add(stateType, rulesList);
        }

        public void Update() {
            bool anyTransition;
            do {
                anyTransition = false;
                if (!(this.Current is { } currentState)) {
                    break;
                }

                var transition = new RuleTransition<T>();
                if (this.transitionRules.TryGetValue(currentState.GetType(), out var rulesList)) {
                    foreach (var rule in rulesList) {
                        rule(currentState, ref transition);

                        if (this.DoTransition(ref transition)) {
                            anyTransition = true;
                            break;
                        }
                    }
                }
            } while (anyTransition);
        }

        private bool DoTransition(ref RuleTransition<T> ruleTransition) {
            switch (ruleTransition.TransitionKind) {
                case RuleTransitionKind.Pop: {
                    this.Pop();
                    return true;
                }

                case RuleTransitionKind.Push: {
                    this.Push(ruleTransition.ToState);
                    return true;
                }

                case RuleTransitionKind.Replace: {
                    this.Replace(ruleTransition.ToState);
                    return true;
                }

                case RuleTransitionKind.Reset: {
                    this.Reset(ruleTransition.ToState);
                    return true;
                }

                default: {
                    return false;
                }
            }
        }
    }
}
