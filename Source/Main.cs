using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
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

	[HarmonyPatch(typeof(Game), nameof(Game.FinalizeInit))]
	static class Game_FinalizeInit_Patch
	{
		public static void Postfix()
		{
			ModCounter.Trigger();
		}
	}

	[HarmonyPatch(typeof(PawnPathPool), nameof(PawnPathPool.GetEmptyPawnPath))]
	class PawnPathPool_GetEmptyPawnPath_Patch
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
				SymbolExtensions.GetMethodInfo(() => AllPawnsSpawnedCountx2(default))
			);
		}
	}

	[HarmonyPatch(typeof(DynamicDrawManager), nameof(DynamicDrawManager.DrawDynamicThings))]
	class DynamicDrawManager_DrawDynamicThings_Patch
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

	[HarmonyPatch(typeof(MainTabsRoot), nameof(MainTabsRoot.HandleLowPriorityShortcuts))]
	class MainTabsRoot_HandleLowPriorityShortcuts_Patch
	{
		public static bool Prefix()
		{
			if (WorldRendererUtility.WorldRenderedNow) return true;
			if (Event.current.type != EventType.MouseDown) return true;
			Tools.CloseLabelMenu(true);
			if (Event.current.button != 1) return true;
			return !Tools.GetPawnActions().Any();
		}
	}

	[HarmonyPatch(typeof(Selector), nameof(Selector.HandleMapClicks))]
	class Selector_HandleMapClicks_Patch
	{
		public static bool Prefix()
		{
			if (WorldRendererUtility.WorldRenderedNow) return true;

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
