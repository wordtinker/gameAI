using Engine.MessagingSystem;
using System;
using System.Collections.Generic;

namespace Engine
{
    namespace Goals
    {
        /// <summary>
        /// By comparing the Evaluate results brain can decide
        /// which goal to pursue.
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        public abstract class GoalEvaluator<T>
        {
            protected T owner;
            public GoalEvaluator(T owner)
            {
                this.owner = owner;
            }
            public abstract double Evaluate();
            public abstract Goal<T> GetGoal();
        }

        /// <summary>
        /// Completion status of the goal.
        /// </summary>
        public enum Status
        {
            Inactive, // The goal is waiting to be activated.
            Active,   // The goal has been activated and will be processed each update step.
            Completed,// The goal has completed and will be removed on next update. 
            Failed    // The goal has failed and will either replan or be removed on the next update.
        }

        /// <summary>
        /// Goal object. Uses composite pattern.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public abstract class Goal<T>
        {
            protected T owner;
            protected Status status;

            // shortcuts for cross inheritance requests
            internal bool IsCompleted { get { return status == Status.Completed; } }
            internal bool IsFailed { get { return status == Status.Failed; } }

            // planning phase of goal. can be called any number of times
            public abstract void Activate();
            // main loop of the goal
            public abstract Status Process();
            // cleanup phase.
            public abstract void Terminate();
            // handler for messages from other entities
            public abstract bool HandleMessage(Telegram message);
            public abstract void AddSubgoal(Goal<T> goal);
            public void ActivateIfInactive()
            {
                if (status == Status.Inactive)
                {
                    Activate();
                }
            }
            public Goal(T owner)
            {
                this.owner = owner;
            }
        }

        public abstract class AtomicGoal<T> : Goal<T>
        {
            public AtomicGoal(T owner) : base(owner){ }
            public override void AddSubgoal(Goal<T> goal)
            {
                throw new NotImplementedException();
            }
        }

        public abstract class CompositeGoal<T> : Goal<T>
        {
            protected Stack<Goal<T>> subgoals = new Stack<Goal<T>>();

            public CompositeGoal(T owner) : base(owner) { }
            public override void AddSubgoal(Goal<T> goal)
            {
                subgoals.Push(goal);
            }
            public Status ProcessSubgoals()
            {
                //remove all completed and failed goals from the front of the subgoal list
                while (subgoals.Count != 0 &&
                    (subgoals.Peek().IsCompleted || subgoals.Peek().IsFailed))
                {
                    subgoals.Pop().Terminate();
                }
                //if any subgoals remain, process the one at the front of the list
                if (subgoals.Count != 0)
                {
                    //grab the status of the frontmost subgoal
                    Status statusOfSubGoals = subgoals.Peek().Process();
                    //we have to test for the special case where the frontmost subgoal
                    //reports "completed" and the subgoal list contains additional goals.
                    //When this is the case, to ensure the parent keeps processing its
                    //subgoal list,the "active" status is returned
                    if (statusOfSubGoals == Status.Completed && subgoals.Count > 1)
                    {
                        return Status.Active;
                    }
                    return statusOfSubGoals;
                }
                //no more subgoals to process - return "completed"
                else
                {
                    return Status.Completed;
                }
            }
            // kept private. composite goal should handle loose ends by itself
            protected void RemoveAllSubgoals()
            {
                foreach (Goal<T> g in subgoals)
                {
                    g.Terminate();
                }
                subgoals.Clear();
            }
        }
    }
}
