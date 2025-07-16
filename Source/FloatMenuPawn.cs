using System;
using UnityEngine;
using Verse;

namespace ReverseCommands;

public class FloatMenuOptionPawn : FloatMenuOption
{
	public readonly Pawn pawn;

	public FloatMenuOptionPawn(FloatMenuOption option, Pawn pawn, Action action, MenuOptionPriority priority, Action<Rect> mouseoverGuiAction)
		: base("", action, option.iconTex, option.iconColor, priority, mouseoverGuiAction, option.revalidateClickTarget, option.extraPartWidth, option.extraPartOnGUI, option.revalidateWorldClickTarget, option.playSelectionSound, option.orderInPriority, option.iconJustification, option.extraPartRightJustified)
	{
		this.pawn = pawn;
		iconThing = option.iconThing;
		thingStyle = option.thingStyle;
		shownItem = option.shownItem;
		forceBasicStyle = option.forceBasicStyle;
		graphicIndexOverride = option.graphicIndexOverride;
		drawPlaceHolderIcon = option.drawPlaceHolderIcon;
	}

	public override bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
	{
		_ = base.DoGUI(rect, colonistOrdering, floatMenu);
		return false; // don't close after an item is selected
	}
}