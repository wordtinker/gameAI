using Engine.Goals;
using Engine.MessagingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using World;

namespace GoalBasedAgent
{
    class CelebrateGoalEvaluator : GoalEvaluator<Miner>
    {
        public CelebrateGoalEvaluator(Miner owner) : base(owner) { }
        public override double Evaluate()
        {
            if (owner.StateMachine.State == owner.StateMachine.Vigorous)
            {
                return Math.Max(owner.MoneyInBank - owner.ComfortLevel, 0);
            }
            else
            {
                return 0;
            }
        }
        public override Goal<Miner> GetGoal()
        {
            return new Celebrate(owner);
        }
    }

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
            evaluators.Add(new CelebrateGoalEvaluator(owner));
        }
        public override void Activate()
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"{owner.Name}: Ruminating about the life.");
            Console.ResetColor();
            status = Status.Active;
            Arbitrate();
        }
        public override Status Process()
        {
            ActivateIfInactive();
            Status subgoalStatus = ProcessSubgoals();
            // if finished last goal
            if (subgoalStatus == Status.Completed || subgoalStatus == Status.Failed)
            {
                RemoveAllSubgoals();
                Console.WriteLine($"{owner.Name}: Brain is itchy.");
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
            // Choose next goal only if no goal is present
            if (subgoals.Count == 0)
            {
                var chosen = evaluators.OrderByDescending(e => e.Evaluate()).FirstOrDefault();
                if (chosen != null)
                {
                    AddSubgoal(chosen.GetGoal());
                }
            }
        }
        /// <summary>
        /// Respond to invasion by starting fight
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool HandleMessage(Telegram message)
        {
            // ask subgoals
            if (base.HandleMessage(message)) return true;
            // respond locally
            if (message.Message == (int)Messages.Invasion)
            {
                // start fighting
                AddSubgoal(new Fight(owner));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
