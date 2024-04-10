﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ReverseCommands
{
	public class FloatMenuLabels : FloatMenu
	{
		public FloatMenuLabels(List<FloatMenuOption> options) : base(options, null, false)
		{
			givesColonistOrders = false;
			vanishIfMouseDistant = true;
			closeOnClickedOutside = false;
		}
	}

	public class FloatMenuOptionNoClose(string label, Action action) : FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0, null, null)
	{
		public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
		{
			_ = base.DoGUI(rect, colonistOrdering, floatMenu);
			return false; // don't close after an item is selected
		}
	}
}
