using Engine.Entity;
using Engine.MessagingSystem;
using Engine.Goals;
using World;

namespace GoalBasedAgent
{
    class Miner : BaseGameEntity
    {
        internal string Name { get; set; }
        // the place where the miner is currently situated
        internal LocationType Location { get; set; }
        // no place like home
        internal LocationType Home { get; set; }
        // Miner's wife
        internal BaseGameEntity Wife { get; set; }
        //how many nuggets the miner has in his pockets
        internal int GoldCarried { get; set; }
        //how much money the miner has deposited in the bank
        internal int MoneyInBank { get; set; }
        //the higher the value, the more tired the miner
        internal int Fatigue { get; set; }
        //the higher the value, the thirstier the miner
        internal int Thirst { get; set; }

        // every miner is a bit different
        internal int ThirstLevel { get; set; } = 8;
        internal int TirednessThreshold { get; set; } = 8;
        internal int Capacity { get; set; } = 5;
        internal int ComfortLevel { get; set; } = 20;

        // a pointer to FSM
        internal MinerFSM StateMachine { get; private set; }
        // a pointer to Goal
        internal Goal<Miner> Brain { get; private set; }

        public Miner()
        {
            Brain = new Think(this);
            StateMachine = new MinerFSM(this);
        }

        public override void Update()
        {
            Brain.Process();
        }
        public bool Thirsty()
        {
            return Thirst >= ThirstLevel;
        }
        public bool Fatigued()
        {
            return Fatigue >= TirednessThreshold;
        }
        public void DecreaseFatigue()
        {
            Fatigue = 0;
        }
        public bool PocketsFull()
        {
            return GoldCarried >= Capacity;
        }
        public void AddToWealth(int gold)
        {
            MoneyInBank += gold;
        }
        public void BuyAndDrinkAWhiskey()
        {
            DecreaseThirst();
            MoneyInBank -= 2;
        }
        public void DecreaseThirst()
        {
            Thirst = 0;
        }
        public override bool HandleMessage(Telegram message)
        {
            return Brain.HandleMessage(message);
            // TODO messaging
            //return StateMachine.HandleMessage(message);
        }
    }
}
