namespace SpaceTrader.Util {
    public enum StateTransitionKind {
        Push,
        Pop,
        Replace,
    }

    public readonly struct StateTransition<T> {
        public T PreviousState { get; }
        public T NextState { get; }
        public StateTransitionKind Kind { get; }

        public StateTransition(T previousState, T nextState, StateTransitionKind kind) {
            this.PreviousState = previousState;
            this.NextState = nextState;
            this.Kind = kind;
        }

        public override string ToString() {
            return $"{this.Kind} {this.PreviousState} => {this.NextState}";
        }
    }
}