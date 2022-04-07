using System;

namespace Substitution
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			bool verbose = Array.FindIndex(args, x => x == "-v") != -1;

			var engine = new Alchemy.SubstitutionEngine(args[0], args[1], verbose);
			engine.Verbose = verbose;

			var inputs = Console.ReadLine().Split(",");
			var output = engine.Execute(inputs);

			Console.WriteLine();
			Console.WriteLine(output == null ? "Failed to substitute." : string.Join("\n", output));
			Console.WriteLine();
		}
	}
}
