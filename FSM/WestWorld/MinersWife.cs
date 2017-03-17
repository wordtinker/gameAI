using System;

namespace WestWorld
{

    class CookStew : IState<MinersWife>
    {
        public void Enter(MinersWife entity)
        {
            //if not already cooking put the stew in the oven
            if (!entity.Cooking)
            {
                Console.WriteLine($"{entity}: Puttin' the stew in the oven");
                //send a delayed message to myself so that I know when to take the stew
                //out of the oven
                MessageBroker.Instance.Dispatch(new Telegram
                {
                    Delay = 1,
                    Sender = entity,
                    Receiver = entity,
                    Message = (int)Messages.StewReady
                });
                entity.Cooking = true;
            }
        }

        public void Execute(MinersWife entity)
        {
            Console.WriteLine($"{entity}: Watch it cooking");
        }
        public void Exit(MinersWife entity) { /* Do nothing */ }
        public bool OnMessage(MinersWife entity, Telegram message)
        {
            if (message.Message == (int)Messages.StewReady)
            {
                Console.WriteLine($"{entity}: Stew ready! Let's eat");
                //let hubby know the stew is ready
                MessageBroker.Instance.Dispatch(new Telegram
                {
                    Delay = 0,
                    Sender = entity,
                    Receiver = entity.Husband,
                    Message = (int)Messages.StewReady
                });
                entity.Cooking = false;
                entity.StateMachine.State = entity.StateMachine.DoHouseWork;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Blip state. Any state can force entity to that state.
    /// Reverts to previous state after evaluation.
    /// </summary>
    class VisitBathroom : IState<MinersWife>
    {
        public void Enter(MinersWife entity)
        {
            Console.WriteLine($"{entity}:  Walkin' to the can. Need to powda mah pretty li'lle nose");
        }

        public void Execute(MinersWife entity)
        {
            Console.WriteLine($"{entity}: Ahhhhhh! Sweet relief!");
            entity.StateMachine.RevertToPreviousState();
        }

        public void Exit(MinersWife entity)
        {
            Console.WriteLine($"{entity}: Leavin' the Jon");
        }

        public bool OnMessage(MinersWife entity, Telegram message) { return false; }
    }

    class DoHouseWork : IState<MinersWife>
    {
        public void Enter(MinersWife entity) { /* Do nothing */ }
        public void Execute(MinersWife entity)
        {
            Random rnd = new Random();
            switch (rnd.Next(0, 3))
            {
                case 0:
                    Console.WriteLine($"{entity}: Moppin' the floor");
                    break;
                case 1:
                    Console.WriteLine($"{entity}: Washin' the dishes");
                    break;
                case 2:
                    Console.WriteLine($"{entity}: Makin' the bed");
                    break;
                default:
                    break;
            }
        }
        public void Exit(MinersWife entity) { /* Do nothing */ }
        public bool OnMessage(MinersWife entity, Telegram message) { return false; }
    }

    /// <summary>
    /// State that is evaluated every time. 
    /// </summary>
    class WifesGlobalState : IState<MinersWife>
    {
        public void Enter(MinersWife entity) { /* Do nothing */ }
        public void Execute(MinersWife entity)
        {
            Random rnd = new Random();
            // go to bathroom at random from ANY state
            if (rnd.NextDouble() < 0.1)
            {
                entity.StateMachine.State = entity.StateMachine.VisitBathroom;
            }
        }
        public void Exit(MinersWife entity) { /* Do nothing */ }
        public bool OnMessage(MinersWife entity, Telegram message)
        {
            // Starts making stew from any state.
            if (message.Message == (int)Messages.HiHoneyImHome)
            {
                Console.WriteLine($"{entity}: Hi honey. Let me make you some of mah fine country stew");
                entity.StateMachine.State = entity.StateMachine.CookStew;
                return true;
            }
            return false;
        }
    }

    class MinersWifeFSM : FSM<MinersWife>
    {
        internal IState<MinersWife> WifesGlobalState { get; } = new WifesGlobalState();
        internal IState<MinersWife> DoHouseWork { get; } = new DoHouseWork();
        internal IState<MinersWife> VisitBathroom { get; } = new VisitBathroom();
        internal IState<MinersWife> CookStew { get; } = new CookStew();

        public MinersWifeFSM(MinersWife owner) : base(owner)
        {
            State = DoHouseWork;
            GlobalState = WifesGlobalState;
        }
    }

    class MinersWife : BaseGameEntity
    {
        // the place where the miner is currently situated
        internal LocationType Location { get; set; }
        // are we cooking
        internal bool Cooking { get; set; }
        // miner
        internal BaseGameEntity Husband { get; set; }
        // a pointer to FSM
        internal MinersWifeFSM StateMachine { get; private set; }
        public MinersWife()
        {
            StateMachine = new MinersWifeFSM(this);
        }
        public override void Update()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            StateMachine.Update();
            Console.ResetColor();
        }
        public override bool HandleMessage(Telegram message)
        {
            return StateMachine.HandleMessage(message);
        }
    }
}
