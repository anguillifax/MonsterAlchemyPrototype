using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Alchemy
{
	public class SubstitutionEngine
	{
		// =========================================================
		// Data
		// =========================================================

		public bool Verbose { get; set; }

		private const int MaxIterations = 10_000;

		private struct Rule
		{
			public int[] inputs;
			public int[] outputs;
		}

		private readonly Dictionary<string, int> strToId;
		private readonly List<string> idToStr;
		private readonly List<Rule> rules;
		private readonly int attrCount;

		// =========================================================
		// Util
		// =========================================================

		private void Log(string message)
		{
			Console.WriteLine("[AlchemyEngine] " + message);
		}

		private void Warn(string message)
		{
			Console.Error.WriteLine("[AlchemyEngine] Warning: " + message);
		}

		private List<string> FreqToList(int[] freq)
		{
			List<string> o = new List<string>();
			for (int i = 0; i < attrCount; i++)
			{
				if (freq[i] > 0)
				{
					o.Add(idToStr[i] + ":" + freq[i]);
				}
			}
			return o;
		}

		private string FreqToString(int[] freq)
		{
			return string.Join(", ", FreqToList(freq));
		}

		private bool FreqEqual(int[] x, int[] y)
		{
			for (int i = 0; i < attrCount; i++)
			{
				if (x[i] != y[i])
				{
					return false;
				}
			}
			return true;
		}

		// =========================================================
		// Initialization
		// =========================================================

		public SubstitutionEngine(string attributesPath, string rulesPath, bool debug = false)
		{
			// ----- Attribute Table -----

			strToId = new Dictionary<string, int>();
			idToStr = new List<string>();

			foreach (var line in File.ReadLines(attributesPath))
			{
				if (ShouldIgnore(line))
				{
					continue;
				}

				var str = Sanitize(line);
				if (str.Contains(' ') || str.Contains('\t'))
				{
					Warn($"Invalid whitespace `{str}`.");
					continue;
				}
				if (strToId.ContainsKey(str))
				{
					Warn($"Duplicate entry `{str}`.");
					continue;
				}

				strToId.Add(str, idToStr.Count);
				idToStr.Add(str);
			}

			attrCount = idToStr.Count;

			// ----- Rules Table -----

			rules = new List<Rule>();

			int lineNum = 0;
			foreach (var line in File.ReadLines(rulesPath))
			{
				++lineNum;
				if (ShouldIgnore(line))
				{
					continue;
				}

				var parts = line.Split("->");
				if (parts.Length != 2)
				{
					Warn($"Failed to split input from output on line {lineNum}.");
					continue;
				}

				if (!TryTranslate(parts[0], out int[] inputs, lineNum))
				{
					continue;
				}

				if (inputs.All(x => x == 0))
				{
					Warn($"Rejected no-input recipe on line {lineNum}.");
					continue;
				}

				if (!TryTranslate(parts[1], out int[] outputs, lineNum))
				{
					continue;
				}

				if (FreqEqual(inputs, outputs))
				{
					Warn($"Rejected self->self rule on {lineNum}.");
					continue;
				}


				rules.Add(new Rule() { inputs = inputs, outputs = outputs });
			}

			// ----- Logging -----

			if (debug)
			{
				Log($"TOTAL_ATTR: {attrCount}");
				Log("STR_TO_ID: " + string.Join(" | ", strToId.Select(x => $"{x.Key}:{x.Value}")));
				Log("ID_TO_STR: " + string.Join(" | ", idToStr));
				Log($"RULE_COUNT: {rules.Count}");
				Log("RULE_PRINTOUT_FULL");
				foreach (var r in rules)
				{
					Log($"  {FreqToString(r.inputs)} -> {FreqToString(r.outputs)}");
				}
			}
		}

		private static bool ShouldIgnore(string s) => string.IsNullOrEmpty(s) || (s.Length > 0 && s[0] == '#');

		private static string Sanitize(string s) => s.Trim();

		private bool TryTranslate(string input, out int[] output, int lineNum)
		{
			output = new int[attrCount];
			var split = input.Split(",", StringSplitOptions.RemoveEmptyEntries);

			foreach (var item in split)
			{
				var s = Sanitize(item);
				if (strToId.TryGetValue(s, out int i))
				{
					++output[i];
				}
				else
				{
					Warn($"Unrecognized attribute `{s}` on line {lineNum}.");
					return false;
				}
			}
			return true;
		}

		// =========================================================
		// Execute
		// =========================================================

		public IList<string> Execute(params string[] inputs)
		{
			if (Verbose)
			{
				Log($"IN: {string.Join(";", inputs)}");
			}

			int[] freq = new int[attrCount];

			foreach (var input in inputs)
			{
				var s = Sanitize(input);
				if (strToId.TryGetValue(s, out int id))
				{
					++freq[id];
				}
				else
				{
					Warn($"Omitting unrecognized attribute `{s}`.");
				}
			}

			if (Verbose)
			{
				Log($"FREQ: {FreqToString(freq)}");
				Log(string.Empty);
			}

			int iterations = 0;
			while (true)
			{
				bool matched = false;

				foreach (var rule in rules)
				{
					if (CanSubstitute(freq, rule.inputs))
					{
						if (Verbose)
						{
							Log($"RULE  : {FreqToString(rule.inputs)} -> {FreqToString(rule.outputs)}");
							Log($"BEFORE: {FreqToString(freq)}");
						}

						matched = true;
						for (int i = 0; i < attrCount; i++)
						{
							freq[i] -= rule.inputs[i];
							freq[i] += rule.outputs[i];
						}

						if (Verbose)
						{
							Log($"AFTER : {FreqToString(freq)}");
							Log(string.Empty);
						}
					}
				}

				if (!matched)
				{
					break;
				}

				++iterations;
				if (iterations > MaxIterations)
				{
					Warn($"(FATAL) Recipe exceeded maximum iteration count ({MaxIterations}). Inputs [ {string.Join(", ", inputs)} ].");
					return null;
				}
			}

			if (Verbose)
			{
				Log($"OUT: {FreqToString(freq)}");
			}

			return FreqToList(freq);
		}

		private bool CanSubstitute(int[] source, int[] check)
		{
			for (int i = 0; i < attrCount; i++)
			{
				if (source[i] < check[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}