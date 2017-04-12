using System;
using System.Collections.Generic;

namespace MiniMax
{
    public enum Move
    {
        Pass = 0,
        A1 = 101, A2, A3, A4, A5, A6, A7, A8,
        B1 = 201, B2, B3, B4, B5, B6, B7, B8,
        C1 = 301, C2, C3, C4, C5, C6, C7, C8,
        D1 = 401, D2, D3, D4, D5, D6, D7, D8,
        E1 = 501, E2, E3, E4, E5, E6, E7, E8,
        F1 = 601, F2, F3, F4, F5, F6, F7, F8,
        G1 = 701, G2, G3, G4, G5, G6, G7, G8,
        H1 = 801, H2, H3, H4, H5, H6, H7, H8
    }

    public class Board : ICloneable
    {
        private char[][] board;
        public IPlayer Opponent { get; private set; }
        public IPlayer CurrentPlayer { get; private set; }
        private Move ToMove(int i, int j)
        {
            int move = (i + 1) * 100 + (j + 1);
            return (Move)move;
        }
        private Tuple<int, int> FromMove(Move move)
        {
            int coord = (int)move;
            int i = coord / 100 - 1;
            int j = coord % 100 - 1;
            return Tuple.Create(i, j);
        }

        private bool IsMoveLegal(int i, int j, out List<Tuple<int, int>> chips, bool multi = false)
        {
            chips = new List<Tuple<int, int>>();
            // assume i and j are legal
            // if it's not empty - return
            if (char.IsLetter(board[i][j])) return false;
            // For each of the 8 directions horizontal, vertical, and diagonal
            for (int dX = -1; dX <= 1; dX++)
            {
                for (int dY = -1; dY <= 1; dY++)
                {
                    int tempX = i;
                    int tempY = j;
                    List<Tuple<int, int>> oneMove = new List<Tuple<int, int>>();
                    // perform the scan
                    do
                    {
                        // look at one direction
                        tempX += dX;
                        tempY += dY;
                        // if (tempx,tempy) is an illegal coordinate i.e.tempx or tempy < 0 or > 7
                        if (tempX < 0 || tempX >= board.Length || tempY < 0 || tempY >= board.Length)
                        {
                            oneMove.Clear();
                            break;
                        }
                        // if (tempx,tempy) is empty, no opponent chips will be flipped in this direction
                        if (!char.IsLetter(board[tempX][tempY]))
                        {
                            oneMove.Clear();
                            break;
                        }
                        // if (tempx,tempy) is of opponents color, move could be done
                        if (board[tempX][tempY] == Opponent.Symbol)
                        {
                            oneMove.Add(Tuple.Create(tempX, tempY));
                            continue;
                        }
                        // if (tempx,tempy) is player's own color break and finilize
                        if (board[tempX][tempY] == CurrentPlayer.Symbol) break;
                    } while (true);
                    // if something could be flipped the move is legal
                    if (oneMove.Count > 0)
                    {
                        chips.AddRange(oneMove);
                        // if in multi mode continue search of new chips to be flipped
                        if (!multi) return true;
                    }
                }
            }
            return chips.Count > 0;
        }
        /// <summary>
        /// Moves for a current player.
        /// </summary>
        /// <returns></returns>
        public List<Move> GetMoves()
        {
            List<Move> legalMoves = new List<Move>();
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    List<Tuple<int, int>> _;
                    if (IsMoveLegal(i, j, out _))
                    {
                        legalMoves.Add(ToMove(i, j));
                    }
                }
            }
            if (legalMoves.Count == 0)
            {
                legalMoves.Add(Move.Pass);
            }
            return legalMoves;
        }
        private void SwitchPlayers()
        {
            // switch players
            IPlayer temp = Opponent;
            Opponent = CurrentPlayer;
            CurrentPlayer = temp;
        }

        public bool MakeMove(Move move, out Board newState)
        {
            // make a new state
            newState = (Board)this.Clone();

            if (move == Move.Pass)
            {
                newState.CurrentPlayer.Passed = true;
                newState.SwitchPlayers();
                return true;
            }
            else
            {
                // forget that we might have passed earlier
                newState.CurrentPlayer.Passed = false;
                // move chips
                Tuple<int, int> coords = FromMove(move);
                int i = coords.Item1;
                int j = coords.Item2;
                List<Tuple<int, int>> toFlip;
                if (newState.IsMoveLegal(i, j, out toFlip, multi: true))
                {
                    // put new chip and flip the others
                    newState.board[i][j] = newState.CurrentPlayer.Symbol;
                    foreach (var chip in toFlip)
                    {
                        newState.board[chip.Item1][chip.Item2] = CurrentPlayer.Symbol;
                    }
                    newState.SwitchPlayers();
                    return true;
                }
            }
            return false;
        }
        public bool IsGameOver()
        {
            return CurrentPlayer.Passed && Opponent.Passed;
        }

        public Tuple<int, int> GetScore(IPlayer player)
        {
            int score = 0;
            int oppScore = 0;
            for (int i = 0; i < board.Length; i++)
            {
                for (int j = 0; j < board[i].Length; j++)
                {
                    if (board[i][j] == player.Symbol)
                    {
                        score++;
                    }
                    else if (char.IsLetter(board[i][j]))
                    {
                        oppScore++;
                    }
                }
            }
            return Tuple.Create(score, oppScore);
        }
        private void RenderTopLine()
        {
            Console.WriteLine("     1   2   3   4   5   6   7   8");
        }
        private void RenderSeparator()
        {
            Console.WriteLine("   +---+---+---+---+---+---+---+---+");
        }
        private void RenderLine(int row)
        {
            string line = $"{Convert.ToChar(row + 65)}  |";
            for (int i = 0; i < board[row].Length; i++)
            {
                line += " ";
                char x = board[row][i];
                x = (!char.IsLetter(x)) ? ' ' : x;
                line += x;
                line += " |";
            }
            Console.WriteLine(line);
        }
        public void Render()
        {
            Console.Clear();
            RenderTopLine();
            RenderSeparator();
            for (int i = 0; i < board.Length; i++)
            {
                RenderLine(i);
                RenderSeparator();
            }
        }
        public void ShowScore()
        {
            Tuple<int, int> scores = GetScore(CurrentPlayer);
            Console.WriteLine($"{CurrentPlayer.Symbol}'s score: {scores.Item1}");
            Console.WriteLine($"{Opponent.Symbol}'s score: {scores.Item2}");
        }

        public object Clone()
        {
            Board newBoard = new Board(this);
            newBoard.CurrentPlayer = CurrentPlayer;
            newBoard.Opponent = Opponent;
            return newBoard;
        }

        private Board(Board oldBoard)
        {
            board = new char[8][];
            for (int i = 0; i < 8; i++)
            {
                board[i] = (char[])oldBoard.board[i].Clone();
            }
        }

        public Board(IPlayer p1, IPlayer p2)
        {
            board = new char[8][];
            for (int i = 0; i < 8; i++)
            {
                board[i] = new char[8];
            }
            p1.Symbol = 'x';
            p2.Symbol = 'o';
            board[3][3] = p1.Symbol;
            board[4][3] = p2.Symbol;
            board[3][4] = p2.Symbol;
            board[4][4] = p1.Symbol;
            CurrentPlayer = p1;
            Opponent = p2;
        }
    }
}
