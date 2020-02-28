using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ReverseCommands
{
	[StaticConstructorOnStartup]
	static class Main
	{
		static Main()
		{
			var harmony = new Harmony("net.pardeike.reversecommands");
			harmony.PatchAll();
		}
	}

	[HarmonyPatch(typeof(Game))]
	[HarmonyPatch("FinalizeInit")]
	static class Game_FinalizeInit_Patch
	{
		public static void Postfix()
		{
			ModCounter.Trigger();
		}
	}

	[HarmonyPatch(typeof(PawnPathPool))]
	[HarmonyPatch("GetEmptyPawnPath")]
	class Patch0
	{
		public static int AllPawnsSpawnedCountx2(MapPawns instance)
		{
			// all we want is some extra allocation
			return instance.AllPawnsSpawnedCount * 5 + 2;
		}

		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			return instructions.MethodReplacer(
				AccessTools.Method(typeof(MapPawns), "get_AllPawnsSpawnedCount"),
				AccessTools.Method(typeof(Patch0), "AllPawnsSpawnedCountx2")
			);
		}
	}

	[HarmonyPatch(typeof(DynamicDrawManager))]
	[HarmonyPatch("DrawDynamicThings")]
	class Patch1
	{
		public static void Prefix()
		{
			if (PathInfo.current != null)
			{
				var path = PathInfo.GetPath(PathInfo.current);
				if (path != null) path.DrawPath(null);
			}
			PathInfo.current = null;
		}
	}

	[HarmonyPatch(typeof(MainTabsRoot))]
	[HarmonyPatch("HandleLowPriorityShortcuts")]
	class Patch2
	{
		public static bool Prefix()
		{
			if (Event.current.type != EventType.MouseDown) return true;
			Tools.CloseLabelMenu(true);
			if (Event.current.button != 1) return true;
			return !Tools.GetPawnActions().Any();
		}
	}

	[HarmonyPatch(typeof(Selector))]
	[HarmonyPatch("HandleMapClicks")]
	class Patch3
	{
		public static bool Prefix()
		{
			if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
				Tools.CloseLabelMenu(true);

			if (Event.current.type != EventType.MouseDown) return true;
			if (Event.current.button != 1) return true;

			var labeledPawnActions = Tools.GetPawnActions();
			if (!labeledPawnActions.Any()) return true;

			var cell = UI.MouseCell();
			Find.CurrentMap.mapPawns.FreeColonists.Where(Tools.PawnUsable).Do(pawn => PathInfo.AddInfo(pawn, cell));

			var items = labeledPawnActions.Keys.Select(label =>
			{
				var dict = labeledPawnActions[label];
				return Tools.MakeMenuItemForLabel(label, dict);
			}).ToList();

			Tools.labelMenu = new FloatMenuLabels(items);
			Find.WindowStack.Add(Tools.labelMenu);

			Event.current.Use();
			return false;
		}
	}
}