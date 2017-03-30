using Engine.MessagingSystem;

namespace Engine
{
    namespace FSM
    {
        interface IState<T>
        {
            // one shot method on entering state
            void Enter(T entity);
            // main state method, int denotes the success value of the action, range is 0-99
            int Execute(T entity);
            // one shot method on exiting state
            void Exit(T entity);
            // handler for messages from other entities
            bool OnMessage(T entity, Telegram message);
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
            public virtual int Update()
            {
                GlobalState?.Execute(owner);
                int? result = State?.Execute(owner);
                return result.GetValueOrDefault(0);
            }
            public void RevertToPreviousState()
            {
                State = previousState;
            }
            public bool HandleMessage(Telegram message)
            {
                // first see if the current state is valid and that it can handle
                // the message
                if (State != null && State.OnMessage(owner, message)) return true;
                // if not, and if a global state has been implemented, send
                // the message to the global state
                if (GlobalState != null && GlobalState.OnMessage(owner, message)) return true;
                return false;
            }
        }
    }
}
