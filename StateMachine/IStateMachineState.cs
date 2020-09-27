namespace SpaceTrader.Util {
    public interface IStateMachineState<in T> where T : class {
        void Enter(T fromState);
        void Exit(T toState);

        void Restore(T fromState);
        void Suspend(T toState);
    }
}
