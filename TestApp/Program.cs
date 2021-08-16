using System;
using CSUID;

namespace TestApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            switch (args[0])
            {
                case "generate":
                {
                    var generator = new IdGenerator("dev", "example");
                    for (var i = 0; i < int.Parse(args[1]); i++)
                        Console.WriteLine(generator.CreateId());
                    break;
                }
                case "parse":
                {
                    var id = new Id(args[1]);
                    Console.WriteLine($"Timestamp: {id.Timestamp}");
                    Console.WriteLine($"Sequence: {id.Sequence}");
                    Console.WriteLine($"ProcessId: {id.ProcessId}");
                    Console.Write("Mac: ");
                    foreach (var b in id.Mac)
                        Console.Write(":" + b.ToString("X2"));
                    Console.WriteLine();
                    Console.WriteLine($"System: {id.System}");
                    Console.WriteLine($"Environment: {id.Environment}");

                    break;
                }
                default:
                {
                    Console.WriteLine($"Unknown operation {args[0]}");
                    break;
                }
            }

            Console.ReadLine();
        }
    }
}
