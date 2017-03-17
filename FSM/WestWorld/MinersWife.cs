using System;

namespace WestWorld
{
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
    }

    class MinersWifeFSM : FSM<MinersWife>
    {
        internal IState<MinersWife> WifesGlobalState { get; } = new WifesGlobalState();
        internal IState<MinersWife> DoHouseWork { get; } = new DoHouseWork();
        internal IState<MinersWife> VisitBathroom { get; } = new VisitBathroom();

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
    }
}
