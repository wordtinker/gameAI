using System;
using System.Threading;

namespace WestWorld
{
    enum LocationType
    {
        shack,
        goldmine,
        bank,
        saloon
    }

    enum Messages
    {
        HiHoneyImHome,
        StewReady
    }

    abstract class BaseGameEntity : IEventHandler
    {
        //this is the next valid ID. Each time a BaseGameEntity is instantiated
        //this value is updated
        private static int nextValidId;

        //every entity has a unique identifying number
        protected int id;
        //ctor
        public BaseGameEntity()
        {
            // Lazy implementation as we know that Entities will not be saved/loaded.
            id = nextValidId++;
        }
        // Methods
        public abstract void Update();
        abstract public bool HandleMessage(Telegram message);
        public override string ToString()
        {
            return $"{GetType()} with id: {id}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Miner miner = new Miner();
            MinersWife minersWife = new MinersWife();
            miner.Wife = minersWife;
            minersWife.Husband = miner;
            for (int i = 0; i < 40; i++)
            {
                miner.Update();
                minersWife.Update();
                MessageBroker.Instance.DispatchDelayedMessages();
                Thread.Sleep(500);
                Console.WriteLine();
            }
        }
    }
}
