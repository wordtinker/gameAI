using System.Collections.Generic;
using System.Linq;

namespace WestWorld
{

    class Telegram
    {
        // Delay is tick based
        public int Delay { get; set; }
        public IEventHandler Sender { get; set; }
        public IEventHandler Receiver { get; set; }
        // int is used for simplicity
        public int Message { get; set; }
    }

    sealed class MessageBroker
    {
        // Singletone
        private static readonly MessageBroker instance = new MessageBroker();
        private MessageBroker() { }
        public static MessageBroker Instance
        {
            get
            {
                return instance;
            }
        }
        // Instance fields
        // List of delayed messages, no priority, each message is dispatched
        // on proper tick in FIFO order
        List<Telegram> delayedMessages = new List<Telegram>();
        // Methods
        public void Dispatch(Telegram message)
        {
            //if there is no delay, route the telegram immediately
            if (message.Delay <= 0)
            {
                //send the telegram to the recipient
                Discharge(message);
            }
            else
            {
                delayedMessages.Add(message);
            }
        }
        public void DispatchDelayedMessages()
        {
            for (int i = delayedMessages.Count - 1; i >= 0; i--)
            {
                Telegram message = delayedMessages[i];
                message.Delay--;
                if (message.Delay <= 0)
                {
                    //send the telegram to the recipient
                    Discharge(message);
                    delayedMessages.RemoveAt(i);
                }
            }
        }
        private void Discharge(Telegram message)
        {
            message.Receiver?.HandleMessage(message);
        }
    }

    interface IEventHandler
    {
        bool HandleMessage(Telegram message);
    }

    interface IState<T>
    {
        // one shot method on entering state
        void Enter(T entity);
        // main state method
        void Execute(T entity);
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
        public virtual void Update()
        {
            GlobalState?.Execute(owner);
            State?.Execute(owner);
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
