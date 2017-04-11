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
        public override Board Play()
        {
            Move result = Board.GetBestMove();
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

    class Program
    {
        static void Main(string[] args)
        {
            GameTable table = new GameTable();
            table.Run();
        }
    }
}
