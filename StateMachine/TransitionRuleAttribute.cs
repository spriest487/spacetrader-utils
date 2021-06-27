using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace SpaceTrader.Util {
    [MeansImplicitUse]
    public class TransitionRuleAttribute : PreserveAttribute {
        public RuleTransitionKind TransitionKind { get; }

        public TransitionRuleAttribute(RuleTransitionKind transitionKind) {
            this.TransitionKind = transitionKind;
        }
    }
}
