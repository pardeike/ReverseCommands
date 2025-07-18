﻿using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReverseCommands;

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
		options
			.OfType<FloatMenuOptionPawn>()
			.Do(option =>
			{
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