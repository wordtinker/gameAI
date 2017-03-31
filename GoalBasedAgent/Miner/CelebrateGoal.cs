using Engine.Goals;
using Engine.MessagingSystem;
using System;
using World;

namespace GoalBasedAgent
{
    class Gamble : AtomicGoal<Miner>
    {
        public Gamble(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Let's ride!");
            status = Status.Active;
        }
        public override Status Process()
        {
            int saloonWage = 4;
            ActivateIfInactive();
            if (owner.Location != LocationType.saloon)
            {
                status = Status.Failed;
                return Status.Failed;
            }
            int result = owner.StateMachine.Update();
            // saloon always wins
            if (owner.MoneyInBank >= saloonWage)
            {
                owner.MoneyInBank -= saloonWage;
            }
            return Status.Completed;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The dreams are broken.");
        }
    }

    class GoToSaloon : AtomicGoal<Miner>
    {
        public GoToSaloon(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Opted for going to saloon.");
            status = Status.Active;
        }
        public override Status Process()
        {
            ActivateIfInactive();

            if (owner.Location == LocationType.saloon)
            {
                status = Status.Completed;
                return Status.Completed;
            }

            int taskComplexity = 10;
            int result = owner.StateMachine.Update();
            if (result >= taskComplexity)
            {
                Console.WriteLine($"{owner.Name}: Got to the saloon.");
                owner.Location = LocationType.saloon;
                status = Status.Completed;
                return Status.Completed;
            }
            else
            {
                Console.WriteLine($"{owner.Name}: Trying to go to the saloon: {result}");
                status = Status.Failed;
                return Status.Failed;
            }
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The boots are shiny.");
        }
    }

    class Celebrate : CompositeGoal<Miner>
    {
        public Celebrate(Miner owner) : base(owner){ }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Time to get some fun.");
            status = Status.Active;
            AddSubgoal(new Gamble(owner));
            AddSubgoal(new GoToSaloon(owner));
        }
        public override Status Process()
        {
            ActivateIfInactive();

            Status result = ProcessSubgoals();
            if (result == Status.Failed)
            {
                // really really really have to visit saloon
                // take another time and prevent failed goal from cleanup
                subgoals.Peek().Activate();
                return Status.Active;
            }
            else if (result == Status.Completed)
            {
                status = Status.Completed;
                RemoveAllSubgoals();
            }
            return result;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: Abandon fun.");
        }
    }
}
