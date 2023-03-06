using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace SpaceTrader.Util {
    [MeansImplicitUse]
    public class TransitionRuleAttribute : PreserveAttribute {
        /// <summary>
        /// Kind of transition to trigger if this rule is successfully evaluated
        /// </summary>
        public RuleTransitionKind TransitionKind { get; }

        /// <summary>
        /// Name of a condition property belonging to the state, evaluating to the condition value. If set,
        /// the rule method itself will only be invoked when the condition value is not null or default.
        /// </summary>
        [CanBeNull]
        public string Condition { get; set; }

        public TransitionRuleAttribute(RuleTransitionKind transitionKind, [CanBeNull] string condition = null) {
            this.TransitionKind = transitionKind;
            this.Condition = condition;
        }
    }
}
