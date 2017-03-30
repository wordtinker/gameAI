using System.Collections.Generic;

namespace Engine
{
    namespace MessagingSystem
    {
        public interface IEventHandler
        {
            bool HandleMessage(Telegram message);
        }
        public class Telegram
        {
            // Delay is tick based
            public int Delay { get; set; }
            public IEventHandler Sender { get; set; }
            public IEventHandler Receiver { get; set; }
            // int is used for simplicity
            public int Message { get; set; }
        }
        public sealed class MessageBroker
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
    }
}
