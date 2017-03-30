using System;

namespace Engine
{
    namespace RND
    {
        /// <summary>
        /// Random generator.
        /// </summary>
        static class RND
        {
            private static Random seed = new Random();

            public static int Roll(int upperBound, int plus=0)
            {
                return seed.Next(upperBound) + plus;
            }
        }
    }
}
