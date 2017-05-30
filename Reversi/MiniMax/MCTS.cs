using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniMax
{
    interface IPolicy
    {
        void AddState(Board s);
        bool ContainsState(Board s);
        Move GetMove(Board s);
        void Update(Board s, Move a, double r);
        Dictionary<Move, StateActionPolicy> A(Board s);
    }
    class StateActionPolicy
    {
        // state action value
        public double Value { get; set; }
        // number of times state action has been visited
        public int N { get; set; }
        // number of winning times
        public int W { get; set; }
        // marks action as used by Policy
        public bool InPolicy { get { return P > 0; } }
        // probability of activation in policy
        public double P { get; set; }
        public StateActionPolicy()
        {
            Value = 0;
        }
    }
    class PolicyState
    {
        // the exploration parameter
        private double c = 0.7;
        private Random rnd;
        // number of times state has been visited
        public int N { get; set; }
        // list of actions
        public Dictionary<Move, StateActionPolicy> Actions { get; }
        public void Rebalance()
        {
            // using upper confidence bound as value
            foreach (StateActionPolicy p in Actions.Values)
            {
                if (p.N == 0)
                {
                    p.Value = double.PositiveInfinity;
                }
                else
                {
                    p.Value = p.W / (double)p.N + c * Math.Sqrt(Math.Log(N) / p.N);
                }
            }

            // Update probabilities of calling action
            double max = Actions.Values.Max(p => p.Value);
            double size = Actions.Where(kvp => kvp.Value.Value == max).Count();
            // policy is greedy
            foreach (var item in Actions)
            {
                item.Value.P = item.Value.Value == max ? 1 / size : 0;
            }

        }
        public Move NextAction
        {
            get
            {
                // choose action randomly
                // any policy is supported(greedy, soft-epsilon)
                double ra = rnd.NextDouble();
                double accumulator = 0;
                foreach (var kvp in Actions.Where(sap => sap.Value.InPolicy))
                {
                    accumulator += kvp.Value.P;
                    if (ra < accumulator)
                    {
                        return kvp.Key;
                    }
                }
                // normally it would have returned already, but if
                // due to uneven division of probabilities
                // ra was ever less than accumulated probability
                // return last key
                return Actions.Keys.Last();
            }
        }
        public PolicyState(Board s)
        {
            rnd = new Random();
            Actions = new Dictionary<Move, StateActionPolicy>();
            double size = s.GetMoves().Count;
            foreach (Move a in s.GetMoves())
            {
                Actions[a] = new StateActionPolicy();
                Actions[a].P = 1 / size;
            }
        }
    }

    class RandomPolicy : IPolicy
    {
        private Random rnd = new Random();

        public Dictionary<Move, StateActionPolicy> A(Board s)
        {
            throw new NotImplementedException();
        }

        public void AddState(Board s)
        {
            throw new NotImplementedException();
        }
        public bool ContainsState(Board s)
        {
            return true;
        }
        public Move GetMove(Board s)
        {
            List<Move> moves = s.GetMoves();
            int r = rnd.Next(moves.Count);
            return moves.ElementAt(r);
        }
        public void Update(Board s, Move a, double r)
        {
            throw new NotImplementedException();
        }
    }

    class UCBPolicy : IPolicy
    {
        public Dictionary<Board, PolicyState> p = new Dictionary<Board, PolicyState>();

        public Dictionary<Move, StateActionPolicy> A(Board s)
        {
            return p[s].Actions;
        }

        public void AddState(Board s)
        {
            p[s] = new PolicyState(s);
        }

        public bool ContainsState(Board s)
        {
            return p.ContainsKey(s);
        }

        public Move GetMove(Board s)
        {
            return p[s].NextAction;
        }

        public void Update(Board s, Move a, double r)
        {
            p[s].N++;
            p[s].Actions[a].N++;
            if (r > 0)
            {
                p[s].Actions[a].W++;

            }
            p[s].Rebalance();
        }
    }

    class MCTS
    {
        private Board board;
        private Board startState;
        private IPolicy policy;
        private IPolicy simulator;
        private Stack<Tuple<Board, Move>> episode;

        private double? Selection()
        {
            Board s = startState;
            while (policy.ContainsState(s))
            {
                Move a = policy.GetMove(s);
                // make move. board switches players by itself
                Board newBoard;
                board.MakeMove(a, out newBoard);
                episode.Push(Tuple.Create(s, a));
                s = newBoard;
                if (s.IsGameOver()) return s.Evaluate(startState.CurrentPlayer);
            }
            // Phase 2. Expand tree
            Expansion(s);
            return null;
        }
        private void Expansion(Board s)
        {
            policy.AddState(s);
        }
        private double Simulation()
        {
            Board s = episode.Peek().Item1;
            do
            {
                Move a = simulator.GetMove(s);
                Board newBoard;
                s.MakeMove(a, out newBoard);
                s = newBoard;
            } while (!s.IsGameOver());
            return s.Evaluate(startState.CurrentPlayer);
        }
        private void Update(double reward)
        {
            while (episode.Count > 0)
            {
                var sa = episode.Pop();
                policy.Update(sa.Item1, sa.Item2, reward);
            }
        }
        public void UpdatePolicy(int n)
        {
            for (int i = 0; i < n; i++)
            {
                double? finalReward = null;
                // Phase 1. Select moves while we have state in policy
                finalReward = Selection();
                // Phase 3. Simulate the tail of episode
                if (!finalReward.HasValue) finalReward = Simulation();
                // Phase 4. Back-propagation
                Update(finalReward.Value);
            }
        }
        public MCTS(Board board, IPolicy policy, IPolicy simulator)
        {
            this.board = board;
            this.startState = board;
            this.policy = policy;
            this.simulator = simulator;
            episode = new Stack<Tuple<Board, Move>>();
            policy.AddState(startState);
        }
    }
}
