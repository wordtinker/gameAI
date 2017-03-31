using Engine.Goals;
using System;

namespace GoalBasedAgent
{
    class Hide : AtomicGoal<Miner>
    {
        public Hide(Miner owner) : base(owner) { }
        public override void Activate()
        {
            status = Status.Active;
            Console.WriteLine($"{owner.Name}: Let's hide behind the rock.");
        }
        public override Status Process()
        {
            ActivateIfInactive();
            Console.WriteLine($"{owner.Name}: Stay behind.");
            status = Status.Completed;
            return Status.Completed;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: Back to work.");
        }
    }
    class Fight : AtomicGoal<Miner>
    {
        public Fight(Miner owner) : base(owner) { }
        public override void Activate()
        {
            status = Status.Active;
            Console.WriteLine($"{owner.Name}: Take the sword.");
        }
        public override Status Process()
        {
            ActivateIfInactive();
            Console.WriteLine($"{owner.Name}: Kill the goblin.");
            status = Status.Completed;
            return Status.Completed;
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: Clean up the blood.");
        }
    }
}
