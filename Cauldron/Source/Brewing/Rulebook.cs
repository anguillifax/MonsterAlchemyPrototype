using System;
using System.Collections.Generic;
using System.IO;

namespace Alchemy.Brewing
{
	public class Rulebook
	{
		public Rulebook(string elementSchema, string ruleSchema)
		{
		}

		public void RegisterPredicate(string name, Predicate<string> predicate)
		{

		}

		public void RegisterAction(string name, Action<string> action)
		{

		}

		public void RegisterAlias(Func<string, string> remapper)
		{

		}
	}
}