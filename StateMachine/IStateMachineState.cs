namespace SpaceTrader.Util {
    public interface IStateMachineState<T> {
        void Enter(T fromState);
        void Exit(T toState);

        void Restore(T fromState);
        void Suspend(T toState);
    }
}