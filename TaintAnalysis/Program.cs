using Newtonsoft.Json;
using System;

namespace TaintAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var tester = Tester.Create())
            {
                tester.GraphvizExecutable.BinariesPath = @"C:\Program Files (x86)\Graphviz2.38\bin";
                tester.Builder.Model = new Model()
                {
                    //CoinJoinBehavior = new CoinJoinBehavior()
                    //{
                    //    MinAnonymitySet = 3,
                    //    MaxAnonymitySet = 6
                    //},
                    //MergeBehavior = new MergeBehavior()
                    //{
                    //    MinMergedCoins = 2,
                    //    MaxMergedCoins = 5
                    //}
                };

                Console.WriteLine("Behavior model:");
                Console.Write(JsonConvert.SerializeObject(tester.Builder.Model, Formatting.Indented));
                Console.WriteLine("-----");

                Console.WriteLine("Test model:");
                var testModel =
                new
                {
                    participants = 10,
                    mixingRounds = 20,
                    merges = 10,
                };
                Console.WriteLine(JsonConvert.SerializeObject(testModel, Formatting.Indented));
                Console.WriteLine("-----");

                Console.WriteLine($"This simulation will create {testModel.participants} participants with 1 UTXO tainted to them, then we will randomly coinjoin them {testModel.mixingRounds} times. And at the end, {testModel.merges} merges will happen.");
                for (int u = 0; u < 10; u++)
                {
                    Console.WriteLine("-------------------------");
                    Console.WriteLine($"Test {u}");
                    tester.Builder.SetPalette(Palettes.Warm[testModel.participants]);
                    for (int i = 0; i < testModel.participants; i++)
                    {
                        tester.Builder.NewTaintedUTXO();
                        tester.Builder.AddTaint(100.0m);
                    }

                    for (int i = 0; i < testModel.mixingRounds; i++)
                    {
                        tester.Builder.MakeCoinjoin();
                    }
                    for (int i = 0; i < testModel.merges; i++)
                    {
                        tester.Builder.Merge();
                    }
                    Console.WriteLine("Dotfile: " + tester.DotFilePath);
                    Console.WriteLine("Image: " + tester.ImageFilePath);
                    tester.CreateImage(open: true); // This will open the result
                    tester.ResetBuilder(u + 1);
                }
            }
        }
    }
}
