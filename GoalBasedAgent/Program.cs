﻿using System;
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
            for (int i = 0; i < 200; i++)
            {
                bob.Update();
                jim.Update();
                Thread.Sleep(100);
                Console.WriteLine();
            }
            bob.Print();
            Console.WriteLine("---------");
            jim.Print();
        }
    }
}
