using System;
using System.Collections.Generic;

namespace Alchemy.Brewing
{
	public class QString
	{
		public QString(string raw)
		{
			Element = new ElementId("TODO");
			Quantity = 1;
		}

		public ElementId Element { get; private set; }
		public int Quantity { get; private set; }
	}
}