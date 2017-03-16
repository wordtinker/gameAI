using System;

namespace WestWorld
{
    /// <summary>
    ///  miner will go home and sleep until his fatigue is decreased
    ///  sufficiently
    /// </summary>
    class GoHomeAndSleepTilRested : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            if (entity.Location != LocationType.shack)
            {
                Console.WriteLine($"{entity}: Walkin' home");
                entity.Location = LocationType.shack;
            }
        }

        public void Execute(Miner entity)
        {
            //if miner is not fatigued start to dig for nuggets again.
            if (!entity.Fatigued())
            {
                Console.WriteLine($"{entity}: What a God darn fantastic nap! Time to find more gold");
                entity.StateMachine.State = entity.StateMachine.EnterMineAndDigForNugget;
            }
            else
            {
                //sleep
                entity.DecreaseFatigue();
                Console.WriteLine($"{entity}: ZZZZ... ");
            }
        }

        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity}: Leaving the house");
        }
    }

    /// <summary>
    /// Entity will go to saloon and drink before going back to mine.
    /// </summary>
    class QuenchThirst : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            if (entity.Location != LocationType.saloon)
            {
                entity.Location = LocationType.saloon;
                Console.WriteLine($"{entity}: Boy, ah sure is thusty! Walking to the saloon");
            }
        }

        public void Execute(Miner entity)
        {
            entity.BuyAndDrinkAWhiskey();
            Console.WriteLine($"{entity}: That's mighty fine sippin liquer");
            entity.StateMachine.State = entity.StateMachine.EnterMineAndDigForNugget;
        }

        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity}: Leaving the saloon, feelin' good");
        }
    }

    /// <summary>
    /// Entity will go to a bank and deposit any nuggets he is carrying. If the 
    ///  miner is subsequently wealthy enough he'll walk home, otherwise he'll
    ///  keep going to get more gold
    /// </summary>
    class VisitBankAndDepositGold : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            //on entry the miner makes sure he is located at the bank
            if (entity.Location != LocationType.bank)
            {
                Console.WriteLine($"{entity}: Goin' to the bank. Yes siree");
                entity.Location = LocationType.bank;
            }
        }

        public void Execute(Miner entity)
        {
            //deposit the gold
            entity.AddToWealth(entity.GoldCarried);
            entity.GoldCarried = 0;
            Console.WriteLine($"{entity}: Depositing gold. Total savings now: {entity.MoneyInBank}");

            //wealthy enough to have a well earned rest?
            if (entity.MoneyInBank >= Miner.ComfortLevel)
            {
                Console.WriteLine($"{entity}: WooHoo! Rich enough for now. Back home to mah li'lle lady");
                entity.StateMachine.State = entity.StateMachine.GoHomeAndSleepTilRested;
            }
            //otherwise get more gold
            else
            {
                entity.StateMachine.State = entity.StateMachine.EnterMineAndDigForNugget;
            }
        }

        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity}: Leavin' the bank");
        }
    }

    /// <summary>
    /// In this state the miner should change location to be at the gold mine.
    /// Once at the gold mine he should dig for gold until his pockets are full,
    /// when he should change state to VisitBankAndDepositNugget.If the
    /// miner gets thirsty while digging he should change state to
    /// QuenchThirst.
    /// </summary>
    class EnterMineAndDigForNugget : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            //if the miner is not already located at the gold mine, he must
            //change location to the gold mine
            if (entity.Location != LocationType.goldmine)
            {
                Console.WriteLine($"{entity}: Walkin' to the gold mine.");
                entity.Location = LocationType.goldmine;
            }
        }

        public void Execute(Miner entity)
        {
            //the miner digs for gold until he is carrying in excess of MaxNuggets.
            //If he gets thirsty during his digging he stops work and
            //changes state to go to the saloon for a whiskey.
            entity.GoldCarried++;
            //diggin' is hard work
            entity.Fatigue++;
            Console.WriteLine($"{entity}: Pickin' up a nugget");
            //if enough gold mined, go and put it in the bank
            if (entity.PocketsFull())
            {
                entity.StateMachine.State = entity.StateMachine.VisitBankAndDepositGold;
            }
            //if thirsty go and get a whiskey
            else if (entity.Thirsty())
            {
                entity.StateMachine.State = entity.StateMachine.QuenchThirst;
            }
        }

        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity}: Ah'm leavin' the gold mine with mah pockets full o' sweet gold");
        }
    }

    /// <summary>
    /// Class that holds all possible states of the miner class.
    /// If same state used for several instances of the same or different
    /// classes, Singleton of Flyweight Factory could be used instead of that class.
    /// </summary>
    class MinerFSM : FSM<Miner>
    {
        internal IState<Miner> EnterMineAndDigForNugget { get; } = new EnterMineAndDigForNugget();
        internal IState<Miner> VisitBankAndDepositGold { get; } = new VisitBankAndDepositGold();
        internal IState<Miner> QuenchThirst { get; } = new QuenchThirst();
        internal IState<Miner> GoHomeAndSleepTilRested { get; } = new GoHomeAndSleepTilRested();

        public MinerFSM(Miner owner) : base(owner)
        {
            // miner starts his life in the goldmine
            State = EnterMineAndDigForNugget;
        }
    }

    class Miner : BaseGameEntity
    {
        // the place where the miner is currently situated
        internal LocationType Location { get; set; }
        //how many nuggets the miner has in his pockets
        internal int GoldCarried { get; set; }
        //the higher the value, the more tired the miner
        internal int Fatigue { get; set; }
        //how much money the miner has deposited in the bank
        internal int MoneyInBank { get; private set; }
        //the higher the value, the thirstier the miner
        private int thirst;

        private const int ThirstLevel = 5;
        private const int TirednessThreshold = 5;
        private const int Capacity = 5;
        public const int ComfortLevel = 15;

        // a pointer to FSM
        internal MinerFSM StateMachine { get; private set; }

        public Miner()
        {
            StateMachine = new MinerFSM(this);
        }

        public override void Update()
        {
            // every action makes miner thirsty
            thirst++;
            StateMachine.Update();
        }
        public bool Thirsty()
        {
            return thirst >= ThirstLevel;
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
            thirst = 0;
            MoneyInBank -= 2;
        }
    }
}
