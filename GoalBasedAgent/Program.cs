using Engine.MessagingSystem;
using Engine.RND;
using System;
using System.Threading;
using World;

namespace GoalBasedAgent
{
    static class Printer
    {
        public static void Print(this Miner miner)
        {
            Console.WriteLine($"Miner: {miner.Name}");
            Console.WriteLine($"Fatigue: {miner.Fatigue}");
            Console.WriteLine($"Thirst: {miner.Thirst}");
            Console.WriteLine($"Gold in pockets: {miner.GoldCarried}");
            Console.WriteLine($"Gold in bank: {miner.MoneyInBank}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Miner bob = new Miner()
            {
                Name = "Bob", Location = LocationType.shack1, Home = LocationType.shack1,
                Capacity = 10, ComfortLevel = 20,
                TirednessThreshold = 10, ThirstLevel = 12
            };
            Miner jim = new Miner()
            {
                Name = "Jim", Location = LocationType.shack2, Home = LocationType.shack2,
                Capacity = 4, ComfortLevel = 25,
                TirednessThreshold = 15, ThirstLevel = 15
            };
            // connect them directly, simplistic solution
            bob.Rival = jim;

            for (int i = 0; i < 200; i++)
            {
                // provoke monster invasion
                if (RND.Roll(100) >= 80)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invasion is coming.");
                    Console.ResetColor();
                    MessageBroker.Instance.Dispatch(new Telegram
                    {
                        Delay = 0,
                        Message = (int)Messages.Invasion,
                        Receiver = bob,
                        Sender = null
                    });
                    MessageBroker.Instance.Dispatch(new Telegram
                    {
                        Delay = 0,
                        Message = (int)Messages.Invasion,
                        Receiver = jim,
                        Sender = null
                    });
                }
                bob.Update();
                jim.Update();
                MessageBroker.Instance.DispatchDelayedMessages();
                Thread.Sleep(100);
                Console.WriteLine();
            }
            bob.Print();
            Console.WriteLine("---------");
            jim.Print();
        }
    }
}
