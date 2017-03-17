namespace WestWorld
{
    interface IState<T>
    {
        // one shot method on entering state
        void Enter(T entity);
        // main state method
        void Execute(T entity);
        // one shot method on exiting state
        void Exit(T entity);
    }

    /// Simple generic FSM with 2 inner states : current, global and memory of
    /// previous state.
    /// Current and global states are evaluated on each Update();
    class FSM<T>
    {
        // entity that FSM is holding state for
        private T owner;
        private IState<T> currentState;
        private IState<T> previousState;
        public IState<T> PreviousState { get; }
        public IState<T> GlobalState { get; set; }
        public IState<T> State
        {
            get { return currentState; }
            set
            {
                //keep a record of the previous state
                previousState = currentState;
                currentState?.Exit(owner);
                currentState = value;
                currentState?.Enter(owner);
            }
        }
        public FSM(T owner)
        {
            this.owner = owner;
        }
        public virtual void Update()
        {
            GlobalState?.Execute(owner);
            State?.Execute(owner);
        }
        public void RevertToPreviousState()
        {
            State = previousState;
        }
    }
}
