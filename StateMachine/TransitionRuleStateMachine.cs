using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace SpaceTrader.Util {
    public enum RuleTransitionKind {
        None = default,
        Push,
        Pop,
        Replace,
        Reset,
    }

    internal struct RuleTransition<T>
        where T : IStateMachineState<T> {
        public T ToState { get; set; }
        public RuleTransitionKind TransitionKind { get; set; }
    }

    public delegate void TransitionRuleTriggeredDelegate(Type stateType, string transitionRuleName);

    public class TransitionRuleStateMachine<T> : StateMachine<T>
        where T : IStateMachineState<T> {
        private delegate void TransitionRuleDelegate(T fromState, ref RuleTransition<T> transition);

        private readonly Dictionary<Type, List<TransitionRuleDelegate>> transitionRules;

        public event TransitionRuleTriggeredDelegate RuleTriggered;

        public TransitionRuleStateMachine(T defaultState) : base(defaultState) {
            this.transitionRules = new Dictionary<Type, List<TransitionRuleDelegate>>(8);

            this.BeforeStateChange += this.OnBeforeStateChange;

            if (this.DefaultState != null) {
                this.ProcessTransitionRules(this.DefaultState);
            }
        }

        private void OnBeforeStateChange(in StateTransition<T> transition) {
            if (transition.NextState is not { } nextState) {
                return;
            }

            this.ProcessTransitionRules(nextState);
        }

        [CanBeNull]
        private TransitionRuleDelegate ProcessTransitionRules(Type stateType, MethodInfo method) {
            if (method.GetCustomAttribute<TransitionRuleAttribute>() is not { } ruleAttribute) {
                return null;
            }

            var conditionProperty = (PropertyInfo)null;
            var conditionDefaultVal = (object)null;
            if (ruleAttribute.Condition != null) {
                conditionProperty = stateType.GetProperty(
                    ruleAttribute.Condition,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );
                if (conditionProperty == null) {
                    Debug.LogErrorFormat(
                        "could not find condition property {0} of {1} for transition rule {2}",
                        ruleAttribute.Condition,
                        stateType,
                        method.Name
                    );
                } else if (conditionProperty.PropertyType.IsValueType) {
                    conditionDefaultVal = Activator.CreateInstance(conditionProperty.PropertyType);
                }
            }

            switch (ruleAttribute.TransitionKind) {
                case RuleTransitionKind.Pop: {
                    return ExecutePopTransitionRuleMethod;
                }

                case RuleTransitionKind.Push:
                case RuleTransitionKind.Replace:
                case RuleTransitionKind.Reset: {
                    return ExecuteTransitionRuleMethod;
                }

                default: {
                    throw new ArgumentOutOfRangeException(
                        nameof(TransitionRuleAttribute.TransitionKind),
                        ruleAttribute.TransitionKind.ToString()
                    );
                }
            }

            void ExecuteTransitionRuleMethod(T fromState, ref RuleTransition<T> ruleTransition) {
                using var profilerSegment = new ProfilerSegment("TransitionRuleStateMachine - Execute Transition Rule");
                
                if (conditionProperty != null) {
                    var conditionValue = conditionProperty.GetValue(fromState);

                    if (Equals(conditionDefaultVal, conditionValue)) {
                        return;
                    }
                }

                if (method.Invoke(fromState, null) is T toState) {
                    try {
                        this.RuleTriggered?.Invoke(stateType, method.Name);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }

                    ruleTransition.TransitionKind = ruleAttribute.TransitionKind;
                    ruleTransition.ToState = toState;
                }
            }

            void ExecutePopTransitionRuleMethod(T fromState, ref RuleTransition<T> ruleTransition) {
                using var profilerSegment = new ProfilerSegment("TransitionRuleStateMachine - Execute Transition Rule");

                if (conditionProperty != null) {
                    var conditionValue = conditionProperty.GetValue(fromState);

                    if (Equals(conditionDefaultVal, conditionValue)) {
                        return;
                    }
                }

                var result = (bool)method.Invoke(fromState, null);
                if (result) {
                    try {
                        this.RuleTriggered?.Invoke(stateType, method.Name);
                    } catch (Exception e) {
                        Debug.LogException(e);
                    }

                    ruleTransition.TransitionKind = RuleTransitionKind.Pop;
                    ruleTransition.ToState = default;
                }
            }
        }

        private void ProcessTransitionRules(T state) {
            var stateType = state.GetType();
            if (this.transitionRules.TryGetValue(stateType, out var rulesList)) {
                return;
            }

            rulesList = new List<TransitionRuleDelegate>();

            const BindingFlags ruleMethodBindingFlags = BindingFlags.Instance
                | BindingFlags.InvokeMethod
                | BindingFlags.Public
                | BindingFlags.NonPublic;

            var ruleMethods = stateType.GetMethods(ruleMethodBindingFlags);

            foreach (var method in ruleMethods) {
                var ruleDelegate = this.ProcessTransitionRules(stateType, method);
                if (ruleDelegate != null) {
                    rulesList.Add(ruleDelegate);
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

                if (!this.transitionRules.TryGetValue(currentState.GetType(), out var rulesList)) {
                    continue;
                }

                var transition = new RuleTransition<T>();
                foreach (var rule in rulesList) {
                    rule(currentState, ref transition);

                    if (this.DoTransition(ref transition)) {
                        anyTransition = true;
                        break;
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
