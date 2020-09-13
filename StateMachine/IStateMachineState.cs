namespace SpaceTrader.Util {
    public interface IStateMachineState<in T> {
        void Enter(T fromState);
        void Exit(T toState);

        void Restore(T fromState);
        void Suspend(T toState);
    }
}
