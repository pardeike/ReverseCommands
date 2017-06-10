using System.Reflection;
using Harmony;
using RimWorld;
using UnityEngine;
using Verse;
using System.Linq;
using Verse.AI;
using System.Collections.Generic;

namespace ReverseCommands
{
	[StaticConstructorOnStartup]
	static class Main
	{
		static Main()
		{
			var harmony = HarmonyInstance.Create("net.pardeike.reversecommands");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}
	}

	[HarmonyPatch(typeof(PawnPathPool))]
	[HarmonyPatch("GetEmptyPawnPath")]
	class Patch0
	{
		static int AllPawnsSpawnedCountx2(MapPawns instance)
		{
			// all we want is some extra allocation
			return instance.AllPawnsSpawnedCount * 5 + 2;
		}

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
		static void Prefix()
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
		static bool Prefix()
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
		static bool Prefix()
		{
			if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
				Tools.CloseLabelMenu(true);

			if (Event.current.type != EventType.MouseDown) return true;
			if (Event.current.button != 1) return true;

			var labeledPawnActions = Tools.GetPawnActions();
			if (!labeledPawnActions.Any()) return true;

			var cell = UI.MouseCell();
			Find.VisibleMap.mapPawns.FreeColonists.Where(Tools.PawnUsable).Do(pawn => PathInfo.AddInfo(pawn, cell));

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