using System;

namespace SpaceTrader.Util {
    [Serializable]
    public class StateMachineScriptBuildReport {
        public string AssetPath;
        public StateMachineScriptBuildReportRule[] Rules;
    }
    
    [Serializable]
    public class StateMachineScriptBuildReportRule {
        public string Name;
    
        public bool RuleMethodReflected;
        public bool ConditionReflected;
    }
}
