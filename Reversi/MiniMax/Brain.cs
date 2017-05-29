
using System;
using System.Collections.Generic;

namespace MiniMax
{
    public static class Brain
    {
        private static Random rnd = new Random();
        public static Move GetRandomMove(this Board board)
        {
            List<Move> moves = board.GetMoves();
            int r = rnd.Next(moves.Count);
            return moves[r];
        }

        public static Move GetBestMoveMiniMax(this Board board)
        {
            // Get the result of a minimax run and return the move
            int thinkAsDeepAs = 5;
            Move move;
            board.MiniMax(board.CurrentPlayer, out move, maxDepth:thinkAsDeepAs);
            return move;
        }
        public static Move GetBestMoveNegaMax(this Board board)
        {
            // Get the result of a negamax run and return the move
            int thinkAsDeepAs = 6;
            Move move;
            board.NegaMax(out move, maxDepth: thinkAsDeepAs);
            return move;
        }
        public static Move GetBestMoveABMiniMax(this Board board)
        {
            int thinkAsDeepAs = 7;
            Move move;
            board.ABMiniMax(board.CurrentPlayer, out move, maxDepth: thinkAsDeepAs);
            return move;
        }

        /// <summary>
        /// Recursive implementation.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <param name="move"></param>
        /// <param name="maxDepth"></param>
        /// <param name="currentDepth"></param>
        /// <returns></returns>
        public static int MiniMax(this Board board, IPlayer player, out Move move, int maxDepth = 5, int currentDepth = 0)
        {
            // Check if we’re done recursing
            if (board.IsGameOver() || currentDepth == maxDepth)
            {
                move = Move.Pass;
                return board.Evaluate(player);
            }
            // Otherwise bubble up values from below
            Move bestMove = Move.Pass;
            int bestScore;
            // if in that board state it's players move start maximizing
            if (board.CurrentPlayer == player)
            {
                bestScore = int.MinValue;
            }
            // start mnimizing
            else
            {
                bestScore = int.MaxValue;
            }

            foreach (Move m in board.GetMoves())
            {
                Board newBoard;
                board.MakeMove(m, out newBoard);
                // Recurse
                Move currentMove;
                int currentScore = newBoard.MiniMax(player, out currentMove, maxDepth: maxDepth, currentDepth: currentDepth + 1);
                // Update the best score
                if (board.CurrentPlayer == player)
                {
                    if (currentScore > bestScore)
                    {
                        bestScore = currentScore;
                        bestMove = m;
                    }
                }
                else
                {
                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        bestMove = m;
                    }
                }
            }
            // Return the score and the best move
            move = bestMove;
            return bestScore;
        }
        /// <summary>
        /// Minimax with AB pruning.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="player"></param>
        /// <param name="move"></param>
        /// <param name="maxDepth"></param>
        /// <param name="currentDepth"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public static int ABMiniMax(this Board board, IPlayer player, out Move move,
                                    int maxDepth = 5, int currentDepth = 0,
                                    int alpha = int.MinValue, int beta = int.MaxValue)
        {
            // Check if we’re done recursing
            if (board.IsGameOver() || currentDepth == maxDepth)
            {
                move = Move.Pass;
                return board.Evaluate(player);
            }
            // Otherwise bubble up values from below
            Move bestMove = Move.Pass;
            int bestScore;
            // if in that board state it's players move start maximizing
            if (board.CurrentPlayer == player)
            {
                bestScore = alpha;
                foreach (Move m in board.GetMoves())
                {
                    Board newBoard;
                    board.MakeMove(m, out newBoard);
                    // Recurse
                    Move currentMove;
                    int currentScore = newBoard.ABMiniMax(player, out currentMove,
                                                          maxDepth: maxDepth, currentDepth: currentDepth + 1,
                                                          alpha: bestScore, beta: beta);
                    // Update the best score
                    if (currentScore > bestScore)
                    {
                        bestScore = currentScore;
                        bestMove = m;
                    }
                    // Prune
                    // Opponent already has beta value lesser than this newfound move
                    // he won't let it happen. No reason to dig deeper
                    if (beta <= bestScore)
                    {
                        break;
                    }
                }
            }
            // start mnimizing
            else
            {
                bestScore = beta;
                foreach (Move m in board.GetMoves())
                {
                    Board newBoard;
                    board.MakeMove(m, out newBoard);
                    // Recurse
                    Move currentMove;
                    int currentScore = newBoard.ABMiniMax(player, out currentMove,
                                                          maxDepth: maxDepth, currentDepth: currentDepth + 1,
                                                          alpha: alpha, beta: bestScore);
                    // Update the best score
                    if (currentScore < bestScore)
                    {
                        bestScore = currentScore;
                        bestMove = m;
                    }
                    // Prune
                    // we already have alpha value bigger than newfound opponent move
                    // we won't let him use it.
                    if (bestScore <= alpha)
                    {
                        break;
                    } 
                }
            }
            // Return the score and the best move
            move = bestMove;
            return bestScore;
        }

        /// <summary>
        /// Simplified version of minimax
        /// Apply only if :
        /// a) 2 player game
        /// b) zero sum game
        /// </summary>
        /// <param name="board"></param>
        /// <param name="move"></param>
        /// <param name="maxDepth"></param>
        /// <param name="currentDepth"></param>
        /// <returns></returns>
        public static int NegaMax(this Board board, out Move move, int maxDepth = 5, int currentDepth = 0)
        {
            // Check if we’re done recursing
            if (board.IsGameOver() || currentDepth == maxDepth)
            {
                move = Move.Pass;
                return board.Evaluate();
            }
            // Otherwise bubble up values from below
            Move bestMove = Move.Pass;
            // use minvalue, we'll be maxing soon
            int bestScore = int.MinValue;
            foreach (Move m in board.GetMoves())
            {
                Board newBoard;
                board.MakeMove(m, out newBoard);
                // Recurse
                Move currentMove;
                // use negated value
                int currentScore = -newBoard.NegaMax(out currentMove, maxDepth: maxDepth, currentDepth: currentDepth + 1);
                // Maximize the best score
                if (currentScore > bestScore)
                {
                    bestScore = currentScore;
                    bestMove = m;
                }
            }
            // Return the score and the best move
            move = bestMove;
            return bestScore;
        }

        /// <summary>
        /// Calculates the score for the current board position for the
        /// player that is CALLING minimax regardless of the current player
        /// that lies on the bottom tier of the tree.
        /// Player chips score +1, opponent chips score -1
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public static int Evaluate(this Board board, IPlayer player)
        {
            Tuple<int, int> scores = board.GetScore(player);
            return scores.Item1 - scores.Item2;
        }

        /// <summary>
        /// Nagamax version.
        /// It uses the current player point of view.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public static int Evaluate(this Board board)
        {
            return board.Evaluate(board.CurrentPlayer);
        }
    }
}
