namespace WestWorld
{
    // TODO comment
    interface IState<T>
    {
        // one shot method on entering state
        void Enter(T entity);
        // main state method
        void Execute(T entity);
        // one shot method on exiting state
        void Exit(T entity);
    }

    // TODO comment
    class FSM<T>
    {
        // entity that FSM is holding state for
        private T owner;
        // TODO
        //public Global GlobalState { get; } = new Global();
        private IState<T> currentState;
        public IState<T> State
        {
            get { return currentState; }
            set
            {
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
            // TODO
            //GlobalState?.Execute(this.owner);
            State?.Execute(owner);
        }
    }
}
