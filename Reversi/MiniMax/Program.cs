using System;

namespace MiniMax
{
    public interface IPlayer
    {
        Board Board { get; set; }
        char Symbol { get; set; }
        bool Passed { get; set; }
        Board Play();
    }

    public abstract class APlayer : IPlayer
    {
        public char Symbol { get; set; }
        public Board Board { get; set; }
        public bool Passed { get; set; }
        public abstract Board Play();
    }

    class Comp : APlayer
    {
        private Func<Board, Move> moveSelector;

        public override Board Play()
        {
            Move result = moveSelector.Invoke(Board);
            Board state;
            if (Board.MakeMove(result, out state))
            {
                return state;
            }
            else
            {
                throw new Exception("Brain is scorched. Something went wrong in the AI.");
            }
        }
        public Comp(Func<Board, Move> moveSelector)
        {
            this.moveSelector = moveSelector;
        }
    }

    class Player : APlayer
    {
        public override Board Play()
        {
            foreach (var item in Board.GetMoves())
            {
                Console.Write($"{item.ToString()} ");
            }
            Console.WriteLine();
            Console.WriteLine($"{Symbol}'s move.");
            do
            {
                string move = Console.ReadLine();
                Move result;
                Board state;
                if (Enum.TryParse<Move>(move, out result) && Board.MakeMove(result, out state))
                {
                    return state;
                }
                else
                {
                    Console.WriteLine("Wrong move, try again.");
                }
            } while (true);
        }
    }

    /// <summary>
    /// Table for 2p play with rendering or each turn.
    /// </summary>
    class GameTable
    {
        private Board board;
        private IPlayer playerOne;
        private IPlayer playerTwo;
        public GameTable(IPlayer playerOne, IPlayer playerTwo)
        {
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            board = new Board(playerOne, playerTwo);
            playerOne.Board = board;
            playerTwo.Board = board;
        }
        public void Run()
        {
            while (!board.IsGameOver())
            {
                board.Render();
                board = board.CurrentPlayer.Play();
                playerOne.Board = board;
                playerTwo.Board = board;
            }
            // show final score
            board.Render();
            board.ShowScore();
        }
    }

    /// <summary>
    /// Table that makes n consequtive autotests for given AI player against
    /// random AI player.
    /// </summary>
    class AutoTestingTable
    {
        private IPlayer playerOne;
        private IPlayer playerTwo;
        public AutoTestingTable(IPlayer playerOne, IPlayer playerTwo)
        {
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
        }
        private Tuple<int, int> DoTest()
        {
            Board board = new Board(playerOne, playerTwo);
            playerOne.Board = board;
            playerOne.Passed = false;
            playerTwo.Board = board;
            playerTwo.Passed = false;
            while (!board.IsGameOver())
            {
                board = board.CurrentPlayer.Play();
                playerOne.Board = board;
                playerTwo.Board = board;
            }
            // show final score
            return board.GetScore(playerOne);
        }
        public void Run()
        {
            int p1Win = 0;
            int tie = 0;
            int p1Lose = 0;
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i);
                Tuple<int, int> result =  DoTest();
                if (result.Item1 > result.Item2)
                {
                    p1Win++;
                }
                else if (result.Item1 < result.Item2)
                {
                    p1Lose++;
                }
                else
                {
                    tie++;
                }
            }
            Console.WriteLine($"Player 1: {p1Win} Tie: {tie} Player 2: {p1Lose}");
        }
    }

    class TrainingTable
    {
        private NeuralBrain network;
        private IPlayer playerOne;
        private IPlayer playerTwo;
        public TrainingTable(IPlayer playerOne, IPlayer playerTwo, NeuralBrain network)
        {
            this.playerOne = playerOne;
            this.playerTwo = playerTwo;
            this.network = network;
        }
        public  void Run()
        {
            for (int i = 0; i < 1000; i++)
            {
                Console.WriteLine(i);
                Board board = new Board(playerOne, playerTwo);
                playerOne.Board = board;
                playerOne.Passed = false;
                playerTwo.Board = board;
                playerTwo.Passed = false;

                while (!board.IsGameOver())
                {
                    // remember the state
                    Board currentState = board; 
                    // make a transition to new state
                    board = board.CurrentPlayer.Play();
                    playerOne.Board = board;
                    playerTwo.Board = board;
                    // Train network, backpropagation value of new state to old
                    network.Train(currentState, network.EvaluateState(board));
                }
                // train the final state from the real result of the game
                Tuple<int, int> result = board.GetScore(playerOne);
                if (result.Item1 > result.Item2)
                {
                    // first player won, use 1.
                    network.Train(board, 1.00);
                }
                else if (result.Item1 < result.Item2)
                {
                    // second player won, use 0;
                    network.Train(board, 0.00);
                }
                else
                {
                    // draw
                    network.Train(board, 0.5);
                }
            }
        }
    }

    class Program
    {
        static IPlayer ChooseCType()
        {
            do
            {
                Console.WriteLine("Select type:");
                Console.WriteLine("R - Random");
                Console.WriteLine("M - Minimax");
                Console.WriteLine("NM - Negamax");
                Console.WriteLine("AB - AB pruning");
                Console.WriteLine("MC - Monte Carlo Tree Search");
                Console.WriteLine("NN - Neural network");
                string result = Console.ReadLine();
                switch (result)
                {
                    case "R":
                        return new Comp(Brain.GetRandomMove);
                    case "M":
                        return new Comp(Brain.GetBestMoveMiniMax);
                    case "NM":
                        return new Comp(Brain.GetBestMoveNegaMax);
                    case "AB":
                        return new Comp(Brain.GetBestMoveABMiniMax);
                    case "MC":
                        return new Comp(Brain.GetBestMoveMCTS);
                    case "NN":
                        NeuralBrain brain = ChooseNetwork();
                        return new Comp(brain.GetBestMove);
                    default:
                        Console.WriteLine("Something went wrong");
                        break;
                }
            } while (true);
        }

        static IPlayer ChoosePlayer()
        {
            do
            {
                Console.WriteLine("Select a player: [H]uman or [C]omputer:");
                string result = Console.ReadLine();
                if (result == "H")
                {
                    return new Player();
                }
                else if (result == "C")
                {
                    return ChooseCType();
                }
                else
                {
                    Console.WriteLine("Something went wrong");
                }
            } while (true);
        }
        static NeuralBrain ChooseNetwork()
        {
            do
            {
                Console.WriteLine("1 - Create New");
                Console.WriteLine("2 - Load");
                string result = Console.ReadLine();
                if (result == "1")
                {
                    Console.WriteLine("Name the network: ...)");
                    string name = Console.ReadLine();
                    return IO.CreateANN(name);
                }
                else if(result == "2")
                {
                    Console.WriteLine("Existing networks:");
                    foreach (string n in IO.ListNetworks())
                    {
                        Console.WriteLine(n);
                    }
                    Console.WriteLine("Name the network: ...)");
                    string name = Console.ReadLine();
                    return IO.LoadANN(name);
                }
                else
                {
                    Console.WriteLine("Something went wrong");
                }
            } while (true);
        }
        static void Main(string[] args)
        {
            IPlayer playerOne;
            IPlayer playerTwo;
            Console.WriteLine("1 - Single game.");
            Console.WriteLine("2 - Multiple games");
            Console.WriteLine("3 - Train ANN");
            string decision = Console.ReadLine();
            switch (decision)
            {
                case "1":
                    playerOne = ChoosePlayer();
                    playerTwo = ChoosePlayer();
                    GameTable table = new GameTable(playerOne, playerTwo);
                    table.Run();
                    break;
                case "2":
                    playerOne = ChooseCType();
                    playerTwo = ChooseCType();
                    AutoTestingTable auto = new AutoTestingTable(playerOne, playerTwo);
                    auto.Run();
                    break;
                case "3":
                    NeuralBrain brain = ChooseNetwork();
                    // use same brain for training
                    playerOne = new Comp(brain.GetBestMove);
                    playerTwo = new Comp(brain.GetBestMove);
                    TrainingTable train = new TrainingTable(playerOne, playerTwo, brain);
                    train.Run();
                    IO.SaveANN(brain);
                    break;
                default:
                    break;
            }
        }
    }
}
