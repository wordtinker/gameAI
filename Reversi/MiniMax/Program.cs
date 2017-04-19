using System;
using System.Collections.Generic;

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

    class RandomAI : APlayer
    {
        static Random rnd = new Random();
        public override Board Play()
        {
            List<Move> moves = Board.GetMoves();
            int r = rnd.Next(moves.Count);
            Move selectedMove = moves[r];
            Board state;
            if (Board.MakeMove(selectedMove, out state))
            {
                return state;
            }
            else
            {
                throw new Exception("Brain is scorched. Something went wrong in random AI.");
            }
        }
    }

    class Comp : APlayer
    {
        public override Board Play()
        {
            // TODO NB  
            //Move result = Board.GetBestMoveMiniMax();
            //Move result = Board.GetBestMoveNegaMax();
            Move result = Board.GetBestMoveABMiniMax();
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
    /// Table for 2p play with rendering or eahc turn.
    /// </summary>
    class GameTable
    {
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
                    return new Comp();
                }
                else
                {
                    Console.WriteLine("Something went wrong");
                }
            } while (true);
        }

        private Board board;
        private IPlayer playerOne;
        private IPlayer playerTwo;
        public GameTable()
        {
            playerOne = ChoosePlayer();
            playerTwo = ChoosePlayer();
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
        private Board board;
        private IPlayer randomAIPlayer;
        private IPlayer playerTwo;
        public AutoTestingTable()
        {
            randomAIPlayer = new RandomAI();
        }
        private Tuple<int, int> DoTest()
        {
            // TODO NB creation of concrete class here
            // TODO random ai is always first player
            playerTwo = new Comp();
            board = new Board(randomAIPlayer, playerTwo);
            randomAIPlayer.Board = board;
            playerTwo.Board = board;
            while (!board.IsGameOver())
            {
                board = board.CurrentPlayer.Play();
                randomAIPlayer.Board = board;
                playerTwo.Board = board;
            }
            // show final score
            return board.GetScore(randomAIPlayer);
        }
        public void Run()
        {
            int randomWin = 0;
            int tie = 0;
            int randomLose = 0;
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine(i);
                Tuple<int, int> result =  DoTest();
                if (result.Item1 > result.Item2)
                {
                    randomWin++;
                }
                else if (result.Item1 < result.Item2)
                {
                    randomLose++;
                }
                else
                {
                    tie++;
                }
            }
            Console.WriteLine($"Random: {randomWin} Tie: {tie} AI: {randomLose}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //GameTable table = new GameTable();
            AutoTestingTable table = new AutoTestingTable();
            table.Run();
        }
    }
}
