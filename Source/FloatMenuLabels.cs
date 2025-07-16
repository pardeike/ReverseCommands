using System.Collections.Generic;
using Verse;

namespace ReverseCommands;

public class FloatMenuLabels : FloatMenu
{
	public FloatMenuLabels(List<FloatMenuOption> options) : base(options, null, false)
	{
		givesColonistOrders = false;
		vanishIfMouseDistant = true;
		closeOnClickedOutside = false;
	}
}