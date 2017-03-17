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

    abstract class BaseGameEntity
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

        public override string ToString()
        {
            return $"{GetType()} with id: {id}";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BaseGameEntity miner = new Miner();
            BaseGameEntity minersWife = new MinersWife();
            for (int i = 0; i < 40; i++)
            {
                miner.Update();
                minersWife.Update();
                Thread.Sleep(500);
                System.Console.WriteLine();
            }
        }
    }
}
