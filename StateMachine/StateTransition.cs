namespace SpaceTrader.Util {
    public readonly struct StateTransition<T> {
        public T PreviousState { get; }
        public T NextState { get; }

        public StateTransition(T previousState, T nextState) {
            this.PreviousState = previousState;
            this.NextState = nextState;
        }

        public override string ToString() {
            return $"{this.PreviousState} => {this.NextState}";
        }
    }
}