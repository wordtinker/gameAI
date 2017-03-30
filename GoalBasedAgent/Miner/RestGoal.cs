using Engine.Goals;
using Engine.MessagingSystem;
using System;

namespace GoalBasedAgent
{
    class Sleep : AtomicGoal<Miner>
    {
        public Sleep(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Time to sleep!");
            status = Status.Active;
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();
            // sanity check
            if (owner.Location != owner.Home)
            {
                status = Status.Failed;
                return Status.Failed;
            }
            // sleep
            owner.DecreaseFatigue();
            owner.StateMachine.Update();
            Console.WriteLine($"{owner.Name}: ... ... ...");
            status = Status.Completed;
            return Status.Completed;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The slippers are sleepy.");
        }
    }

    class EatDinner : AtomicGoal<Miner>
    {
        public EatDinner(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Mmm dinner!");
            status = Status.Active;
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();
            // sanity check
            if (owner.Location != owner.Home)
            {
                status = Status.Failed;
                return Status.Failed;
            }
            // eat dinner
            owner.DecreaseThirst();
            owner.StateMachine.Update();
            Console.WriteLine($"{owner.Name}: Munch munch munch.");
            status = Status.Completed;
            return Status.Completed;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The saucers are dirty.");
        }
    }

    class GoHome : AtomicGoal<Miner>
    {
        public GoHome(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Opted for going home.");
            status = Status.Active;
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();
            // check if we are at home already
            if (owner.Location == owner.Home)
            {
                status = Status.Completed;
                return Status.Completed;
            }

            int taskComplexity = 5;
            int result = owner.StateMachine.Update();
            if (result >= taskComplexity)
            {
                Console.WriteLine($"{owner.Name}: Got to the shack.");
                owner.Location = owner.Home;
                status = Status.Completed;
                return Status.Completed;
            }
            else
            {
                Console.WriteLine($"{owner.Name}: Trying to go home: {result}");
                // Can't fail that task, try harder
                status = Status.Active;
                return Status.Active;
            }
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The steps are shaky.");
        }
    }

    class Resting : CompositeGoal<Miner>
    {
        public Resting(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Time to get some rest.");
            status = Status.Active;
            AddSubgoal(new Sleep(owner));
            AddSubgoal(new EatDinner(owner));
            AddSubgoal(new GoHome(owner));
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();

            Status result = ProcessSubgoals();
            // not catching Failed, all subgoals are infailable
            if (result == Status.Completed)
            {
                RemoveAllSubgoals();
            }
            return result;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: Abandon resting.");
        }
    }
}
