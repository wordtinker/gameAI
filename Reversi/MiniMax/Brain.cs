
namespace MiniMax
{
    public static class Brain
    {
        public static Move GetBestMove(this Board board)
        {
            // Get the result of a minimax run and return the move
            int thinkAsDeepAs = 7;
            Move move;
            board.MiniMax(board.CurrentPlayer, out move, maxDepth:thinkAsDeepAs);
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
                return board.Evaluate();
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
        /// Calculates the score for the current board position for the
        /// current player. player chips score +1, opponent chips score -1
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public static int Evaluate(this Board board)
        {
            // TODO later. not effective, have to rewrite GetScore to prevent double calling
            return board.GetScore(board.CurrentPlayer) - board.GetScore(board.Opponent);
        }
    }
}
