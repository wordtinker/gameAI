using Engine.Goals;
using Engine.MessagingSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoalBasedAgent
{
    class WorkTheBlackSeamGoalEvaluator : GoalEvaluator<Miner>
    {
        public WorkTheBlackSeamGoalEvaluator(Miner owner) : base(owner) { }
        public override double Evaluate()
        {
            if (owner.StateMachine.State == owner.StateMachine.Vigorous)
            {
                return Math.Max(owner.ComfortLevel - owner.MoneyInBank, 0);
            }
            else
            {
                return 0;
            }
        }

        public override Goal<Miner> GetGoal()
        {
            return new WorkTheBlackSeam(owner);
        }
    }

    class RestingGoalEvaluator : GoalEvaluator<Miner>
    {
        public RestingGoalEvaluator(Miner owner) : base(owner) { }
        public override double Evaluate()
        {
            return Math.Max(owner.Fatigue - owner.TirednessThreshold, 0) +
                   Math.Max(owner.Thirst - owner.ThirstLevel, 0);
        }

        public override Goal<Miner> GetGoal()
        {
            return new Resting(owner);
        }
    }

    class Think : CompositeGoal<Miner>
    {
        private List<GoalEvaluator<Miner>> evaluators;
        public Think(Miner owner) : base(owner)
        {
            evaluators = new List<GoalEvaluator<Miner>>();
            evaluators.Add(new WorkTheBlackSeamGoalEvaluator(owner));
            evaluators.Add(new RestingGoalEvaluator(owner));
        }
        public override void Activate()
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("Ruminating about the life.");
            Console.ResetColor();
            status = Status.Active;
            Arbitrate();
        }
        public override bool HandleMessage(Telegram message)
        {
            // TODO messaging
            return false;
        }
        public override Status Process()
        {
            ActivateIfInactive();
            Status subgoalStatus = ProcessSubgoals();

            if (subgoalStatus == Status.Completed || subgoalStatus == Status.Failed)
            {
                RemoveAllSubgoals();
                Console.WriteLine("Brain is itchy.");
                // every goal is complete, do some rest till next tick
                status = Status.Inactive;
            }

            return status;
        }
        public override void Terminate()
        {
            // Called on death of a miner, since death
            // is not an option here, it's not implemented
            throw new NotImplementedException();
        }
        /// <summary>
        /// this method iterates through each goal option to determine which one has
        /// the highest desirability.
        /// </summary>
        private void Arbitrate()
        {
            var chosen = evaluators.OrderByDescending(e => e.Evaluate()).FirstOrDefault();
            if (chosen != null)
            {
                AddSubgoal(chosen.GetGoal());
            }
        }
    }
}
