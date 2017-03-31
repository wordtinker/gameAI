using Engine.Goals;
using Engine.MessagingSystem;
using World;
using System;

namespace GoalBasedAgent
{
    class DepositGold : AtomicGoal<Miner>
    {
        public DepositGold(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Counting nuggets.");
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
            if (owner.Location != LocationType.bank)
            {
                status = Status.Failed;
                return Status.Failed;
            }

            int taskComplexity = 0;
            int result = owner.StateMachine.Update();
            if (result >= taskComplexity)
            {
                //deposit the gold
                owner.AddToWealth(owner.GoldCarried);
                owner.GoldCarried = 0;
                Console.WriteLine($"{owner.Name}: Depositing gold. Total savings now: {owner.MoneyInBank}");
                status = Status.Completed;
                return Status.Completed;
            }
            else
            {
                Console.WriteLine($"{owner.Name}: Failed to deposit: {result}");
                status = Status.Failed;
                return Status.Failed;
            }
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The purses are emptied.");
        }
    }

    class GoToBank : AtomicGoal<Miner>
    {
        public GoToBank(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Opted for going to bank.");
            status = Status.Active;
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();

            if (owner.Location == LocationType.bank)
            {
                status = Status.Completed;
                return Status.Completed;
            }

            int taskComplexity = 20;
            int result = owner.StateMachine.Update();
            if (result >= taskComplexity)
            {
                Console.WriteLine($"{owner.Name}: Got to the bank.");
                owner.Location = LocationType.bank;
                status = Status.Completed;
                return Status.Completed;
            }
            else
            {
                Console.WriteLine($"{owner.Name}: Trying to go to the bank: {result}");
                status = Status.Failed;
                return Status.Failed;
            }
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The boots are soggy.");
        }
    }

    class WorkInTheMine : AtomicGoal<Miner>
    {
        public WorkInTheMine(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Picking up the axe.");
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
            if (owner.Location != LocationType.goldmine)
            {
                status = Status.Failed;
                return Status.Failed;
            }

            int taskComplexity = 40;
            int result = owner.StateMachine.Update();
            if (result >= taskComplexity)
            {
                owner.GoldCarried++;
                Console.WriteLine($"{owner.Name}: Pickin' up a nugget");
                if (result >= taskComplexity * 2)
                {
                    Console.WriteLine($"{owner.Name}: A big one!");
                    MessageBroker.Instance.Dispatch(new Telegram
                    {
                        Delay = 0,
                        Sender = owner,
                        Receiver = owner.Rival,
                        Message = (int)Messages.FoundAGreatOne
                    });
                }
                if (owner.PocketsFull())
                {
                    status = Status.Completed;
                    return Status.Completed;
                }
                else
                {
                    return Status.Active;
                }
            }
            else
            {
                Console.WriteLine($"{owner.Name}: Failed to get gold: {result}");
                status = Status.Failed;
                return Status.Failed;
            }
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The axes are broken.");
        }
    }

    class GoToMine : AtomicGoal<Miner>
    {
        public GoToMine(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Opted for going to mine.");
            status = Status.Active;
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();

            if (owner.Location == LocationType.goldmine)
            {
                status = Status.Completed;
                return Status.Completed;
            }

            int taskComplexity = 20;
            int result = owner.StateMachine.Update();
            if (result >= taskComplexity)
            {
                Console.WriteLine($"{owner.Name}: Got to the mine.");
                owner.Location = LocationType.goldmine;
                status = Status.Completed;
                return Status.Completed;
            }
            else
            {
                Console.WriteLine($"{owner.Name}: Trying to go to the mine: {result}");
                status = Status.Failed;
                return Status.Failed;
            }
        }
        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: The boots are dusty.");
        }
    }

    class WorkTheBlackSeam : CompositeGoal<Miner>
    {
        private int attempts;

        public WorkTheBlackSeam(Miner owner) : base(owner) { }
        public override void Activate()
        {
            Console.WriteLine($"{owner.Name}: Let's work the seam.");
            status = Status.Active;
            AddSubgoal(new DepositGold(owner));
            AddSubgoal(new GoToBank(owner)); // can fail
            AddSubgoal(new WorkInTheMine(owner)); // can fail
            AddSubgoal(new GoToMine(owner)); // can fail
        }
        public override bool HandleMessage(Telegram message)
        {
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();
            Status result = ProcessSubgoals();
            if (result == Status.Failed)
            {
                attempts++;
                if (attempts >= 10)
                {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{owner.Name}: Something went wrong during mining.");
                    Console.ResetColor();
                    status = Status.Failed;
                    RemoveAllSubgoals();
                    return Status.Failed;
                }
                else
                {
                    // take another time and prevent failed goal from cleanup
                    subgoals.Peek().Activate();
                    return Status.Active;
                }
            }
            else if (result == Status.Completed)
            {
                RemoveAllSubgoals();
            }
            return result;
        }

        public override void Terminate()
        {
            Console.WriteLine($"{owner.Name}: Abandon the seam.");
        }
    }
}
