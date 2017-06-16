using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Persist;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MiniMax
{
    class NeuralBrain
    {
        internal BasicNetwork network;
        public string Name { get; set; }

        /// <summary>
        /// Takes a state of the board and returns probability of 
        /// first player win.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public double EvaluateState(Board board)
        {
            //IMLData input = ANNAdapter.Adapt(board);
            IMLData input = ANNAdapter.Adapt192(board);
            IMLData output = network.Compute(input);
            return output[0];
        }
        /// <summary>
        /// Trains state inside neural network to generate new value function.
        /// </summary>
        /// <param name="currentState"></param>
        /// <param name="v"></param>
        public void Train(Board board, double v)
        {
            BasicMLDataSet trainingSet = new BasicMLDataSet();
            BasicMLData ideal = new BasicMLData(1);
            ideal[0] = v;
            //trainingSet.Add(ANNAdapter.Adapt(board), ideal);
            trainingSet.Add(ANNAdapter.Adapt192(board), ideal);
            IMLTrain train = new ResilientPropagation(network, trainingSet);
            train.Iteration();
        }
        public Move GetBestMove(Board board)
        {
            // Find all future states and their values
            List<Tuple<Move, double>> possibleStates = new List<Tuple<Move, double>>();
            foreach (Move move in board.GetMoves())
            {
                Board projectedState;
                board.MakeMove(move, out projectedState);
                possibleStates.Add(Tuple.Create(move, EvaluateState(projectedState)));
            }

            if (board.CurrentPlayer.Symbol == board.FirstPlayerSymbol)
            {
                // first player moves, should maximize outcome
                return possibleStates.Aggregate((i1, i2) => i1.Item2 > i2.Item2 ? i1 : i2).Item1;
            }
            else
            {
                return possibleStates.Aggregate((i1, i2) => i1.Item2 < i2.Item2 ? i1 : i2).Item1;
            }
        }
        public NeuralBrain()
        {
            // create a neural network, without using a factory
            network = new BasicNetwork();
            // input layer
            network.AddLayer(new BasicLayer(null, false, 192));
            // hidden layer
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 60));
            // second hidden layer
            //network.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 8));
            // output layer
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 1));
            network.Structure.FinalizeStructure();
            network.Reset();
        }
        public NeuralBrain(BasicNetwork network)
        {
            this.network = network;
        }
    }

    static class IO
    {
        private static string EXT = ".eg";
        public static IEnumerable<string> ListNetworks()
        {
            return Directory.EnumerateFiles(Directory.GetCurrentDirectory())
                .Where(file => Path.GetExtension(file) == EXT)
                .Select(file => Path.GetFileNameWithoutExtension(file));
        }
        public static NeuralBrain CreateANN(string name)
        {
            NeuralBrain brain = new NeuralBrain();
            brain.Name = name;
            FileInfo file = new FileInfo(Path.ChangeExtension(name, EXT));
            EncogDirectoryPersistence.SaveObject(file, brain.network);
            return brain;
        }
        public static void SaveANN(NeuralBrain brain)
        {
            FileInfo file = new FileInfo(Path.ChangeExtension(brain.Name, EXT));
            EncogDirectoryPersistence.SaveObject(file, brain.network);
        }
        public static NeuralBrain LoadANN(string name)
        {
            FileInfo file = new FileInfo(Path.ChangeExtension(name, EXT));
            BasicNetwork network = (BasicNetwork)EncogDirectoryPersistence.LoadObject(file);
            NeuralBrain brain = new NeuralBrain(network);
            brain.Name = name;
            return brain;
        }
    }

    static class ANNAdapter
    {
        /// <summary>
        /// Adapts Board state to vector of doubles.
        /// Every position on board consists of two inputs
        /// 'x' is 1-0
        /// 'o' is 0-1
        /// no piece is 0-0
        /// Number of input neurons is 8*8*2=128
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public static BasicMLData Adapt(Board state)
        {
            BasicMLData MLData = new BasicMLData(128);
            for (int i = 0; i < state.board.Length; i++)
            {
                for (int j = 0; j < state.board[i].Length; j++)
                {
                    int pos = 16 * i + 2 * j;
                    if (state.board[i][j] == state.FirstPlayerSymbol)
                    {
                        MLData[pos] = 1;
                        MLData[pos + 1] = 0;
                    }
                    else if (state.board[i][j] == state.SecondPlayerSymbol)
                    {
                        MLData[pos] = 0;
                        MLData[pos + 1] = 1;
                    }
                    else
                    {
                        MLData[pos] = 0;
                        MLData[pos + 1] = 0;
                    }
                }
            }
            return MLData;
        }
        /// <summary>
        /// Adapts Board state to vector of doubles.
        /// Every position on board consists of three inputs
        /// 'x' is 1-0-0
        /// 'o' is 0-1-0
        /// no piece is 0-0-1
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static BasicMLData Adapt192(Board state)
        {
            BasicMLData MLData = new BasicMLData(192);
            for (int i = 0; i < state.board.Length; i++)
            {
                for (int j = 0; j < state.board[i].Length; j++)
                {
                    int pos = 24 * i + 3 * j;
                    if (state.board[i][j] == state.FirstPlayerSymbol)
                    {
                        MLData[pos] = 1;
                    }
                    else if (state.board[i][j] == state.SecondPlayerSymbol)
                    {
                        MLData[pos + 1] = 1;
                    }
                    else
                    {
                        MLData[pos + 2] = 1;
                    }
                }
            }
            return MLData;
        }
    }
}
