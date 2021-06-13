using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ReverseCommands
{
	public class FloatMenuColonists : FloatMenu
	{
		public FloatMenuColonists(List<FloatMenuOption> options, string label) : base(options, label, false)
		{
			givesColonistOrders = true;
			vanishIfMouseDistant = true;
			closeOnClickedOutside = true;
		}

		public override void DoWindowContents(Rect rect)
		{
			options.Do(o =>
			{
				var option = o as FloatMenuOptionPawn;
				option.Label = PathInfo.GetJobReport(option.pawn);
				option.SetSizeMode(FloatMenuSizeMode.Normal);
			});
			windowRect = new Rect(windowRect.x, windowRect.y, InitialSize.x, InitialSize.y);
			base.DoWindowContents(windowRect);
		}

		public override void PostClose()
		{
			base.PostClose();

			Tools.CloseLabelMenu(false);
			PathInfo.Clear();
		}
	}

	public class FloatMenuOptionPawn : FloatMenuOption
	{
		public Pawn pawn;

		public FloatMenuOptionPawn(Pawn pawn, Action action, MenuOptionPriority priority, Action mouseOverAction)
			: base("", action, priority, mouseOverAction, null, 0, null, null)
		{
			this.pawn = pawn;
		}

		public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
		{
			_ = base.DoGUI(rect, colonistOrdering, floatMenu);
			return false; // don't close after an item is selected
		}
	}
}
