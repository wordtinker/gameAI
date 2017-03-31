using Engine.FSM;
using Engine.RND;
using Engine.MessagingSystem;
using System;

namespace GoalBasedAgent
{
    /// <summary>
    /// Blip state.
    /// Revert to previous state after execution.
    /// </summary>
    class DrinkFromMagicFlask : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            Console.WriteLine($"{entity.Name}: Time to have a sip!");
        }
        public int Execute(Miner entity)
        {
            // that action does not cost thirst or fatigue
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine($"{entity.Name}: ! ! !");
            Console.ResetColor();
            entity.StateMachine.RevertToPreviousState();
            // miner is ditracted
            return 0;
        }
        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity.Name}: Ahhh!");
        }
        public bool OnMessage(Miner entity, Telegram message)
        {
            return false;
        }
    }

    /// <summary>
    /// Tired state.
    /// Success rate of an action is lowered to 70
    /// Transitions -> Vigorous
    /// </summary>
    class Tired : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            Console.WriteLine($"{entity.Name}: I'm tired, man");
        }

        public int Execute(Miner entity)
        {
            // every action makes miner double thirsty and more exhausted
            entity.Thirst++;
            entity.Thirst++;
            entity.Fatigue++;
            Console.WriteLine($"{entity.Name}: in tired state");
            if (!entity.Fatigued() || !entity.Thirsty())
            {
                entity.StateMachine.State = entity.StateMachine.Vigorous;
            }
            // can do much less when tired
            return RND.Roll(70);
        }

        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity.Name}: Not so tired again, man");
        }

        public bool OnMessage(Miner entity, Telegram message)
        {
            return false;
            // TODO messaging
        }
    }

    /// <summary>
    /// Basic state.
    /// Transitions -> Tired
    /// </summary>
    class Vigorous : IState<Miner>
    {
        public void Enter(Miner entity)
        {
            Console.WriteLine($"{entity.Name}: It's all good, man");
        }

        public int Execute(Miner entity)
        {
            // every action makes miner thirsty and more exhausted
            entity.Thirst++;
            entity.Fatigue++;
            Console.WriteLine($"{entity.Name}: in vigorous state");
            if (entity.Fatigued() && entity.Thirsty())
            {
                entity.StateMachine.State = entity.StateMachine.Tired;
            }
            return RND.Roll(100);
        }

        public void Exit(Miner entity)
        {
            Console.WriteLine($"{entity.Name}: It's all not so good, man");
        }

        public bool OnMessage(Miner entity, Telegram message)
        {
            return false;
            // TODO messaging
        }
    }

    class GlobalMinerState : IState<Miner>
    {
        public void Enter(Miner entity) { }
        public int Execute(Miner entity)
        {
            // try to put miner into drinking state
            int result = RND.Roll(100);
            if (result >= 80)
            {
                entity.StateMachine.State = entity.StateMachine.DrinkFromMagicFlask;
            }
            return 0;
        }
        public void Exit(Miner entity){ }
        public bool OnMessage(Miner entity, Telegram message)
        {
            return false;
        }
    }

    /// <summary>
    /// Class that holds all possible states of the miner class.
    /// If same state used for several instances of the same or different
    /// classes, Singleton of Flyweight Factory could be used instead of that class.
    /// </summary>
    class MinerFSM : FSM<Miner>
    {
        internal IState<Miner> Global { get; } = new GlobalMinerState();
        internal IState<Miner> Vigorous { get; } = new Vigorous();
        internal IState<Miner> Tired { get; } = new Tired();
        internal IState<Miner> DrinkFromMagicFlask = new DrinkFromMagicFlask();

        public MinerFSM(Miner owner) : base(owner)
        {
            GlobalState = Global;
            // miner starts his life vigorously
            State = Vigorous;
        }
    }
}
