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
        
        private const BindingFlags ConditionMemberBindingFlags = BindingFlags.Public 
            | BindingFlags.NonPublic 
            | BindingFlags.Instance;

        private const BindingFlags RuleMethodBindingFlags = BindingFlags.Public
            | BindingFlags.NonPublic
            | BindingFlags.Instance
            | BindingFlags.InvokeMethod;

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

        private void ProcessTransitionRules(T state) {
            var stateType = state.GetType();

            this.FindRulesForType(stateType);
        }

        private List<TransitionRuleDelegate> FindRulesForType(Type type) {
            if (this.transitionRules.TryGetValue(type, out var rules)) {
                return rules;
            }

            rules = new List<TransitionRuleDelegate>();

            var baseType = type.BaseType;
            if (baseType != null) {
                rules.AddRange(this.FindRulesForType(baseType));
            }
                
            var ruleMethods = type.GetMethods(RuleMethodBindingFlags);

            foreach (var method in ruleMethods) {
                var ruleDelegate = this.ProcessTransitionRule(type, method);
                if (ruleDelegate != null) {
                    rules.Add(ruleDelegate);
                }
            }

            this.transitionRules.Add(type, rules);

            return rules;
        }

        [CanBeNull]
        private TransitionRuleDelegate ProcessTransitionRule(Type stateType, MethodInfo method) {
            if (method.GetCustomAttribute<TransitionRuleAttribute>() is not { } ruleAttribute) {
                return null;
            }

            var conditionMember = (MemberInfo)null;
            var conditionValueType = (Type)null;
            var conditionDefaultVal = (object)null;

            if (ruleAttribute.Condition != null) {
                var conditionProperty = stateType.GetProperty(ruleAttribute.Condition, ConditionMemberBindingFlags);
                if (conditionProperty != null) {
                    conditionMember = conditionProperty;
                    conditionValueType = conditionProperty.PropertyType;
                } else {
                    var conditionField = stateType.GetField(ruleAttribute.Condition, ConditionMemberBindingFlags);
                    if (conditionField != null) {
                        conditionMember = conditionField;
                        conditionValueType = conditionField.FieldType;
                    }
                }
                
                if (conditionMember == null) {
                    Debug.LogErrorFormat(
                        "could not find condition member {0} of {1} for transition rule {2}",
                        ruleAttribute.Condition,
                        stateType,
                        method.Name
                    );
                } else if (conditionValueType.IsValueType) {
                    try {
                        conditionDefaultVal = Activator.CreateInstance(conditionValueType);
                    } catch (Exception e) {
                        Debug.LogErrorFormat(
                            "failed to create default value for condition member {0} of {1} for transition rule {2}: {3}",
                            ruleAttribute.Condition,
                            stateType,
                            method.Name, 
                            e
                        );
                        conditionMember = null;
                        conditionValueType = null;
                    }
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

            bool IsConditionActive(T state) {
                switch (conditionMember) {
                    case PropertyInfo conditionProperty: {
                        var conditionValue = conditionProperty.GetValue(state);
                        return !Equals(conditionDefaultVal, conditionValue);
                    }

                    case FieldInfo conditionField: {
                        var conditionValue = conditionField.GetValue(state);
                        return !Equals(conditionDefaultVal, conditionValue);
                    }
                }

                return true;
            }

            void ExecuteTransitionRuleMethod(T fromState, ref RuleTransition<T> ruleTransition) {
                using var profilerSegment = new ProfilerSegment("TransitionRuleStateMachine - Execute Transition Rule");

                if (!IsConditionActive(fromState)) {
                    return;
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

                if (!IsConditionActive(fromState)) {
                    return;
                }

                bool result;
                if (method.ReturnType == typeof(void)) {
                    result = true;
                } else {
                    result = (bool)method.Invoke(fromState, null);
                }

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
                case RuleTransitionKind.Pop when this.Count > 0: {
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
