using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ReverseCommands
{
	public static class Tools
	{
		public static FloatMenuLabels labelMenu;
		public static FloatMenuColonists actionMenu;

		public static Dictionary<Pawn, PathInfo> PawnInfo = new Dictionary<Pawn, PathInfo>();

		public static void CloseLabelMenu(bool sound)
		{
			if (labelMenu != null)
			{
				_ = Find.WindowStack.TryRemove(labelMenu, sound);
				labelMenu = null;
			}
		}

		public static Dictionary<string, Dictionary<Pawn, FloatMenuOption>> GetPawnActions()
		{
			var result = new Dictionary<string, Dictionary<Pawn, FloatMenuOption>>();

			var firstSelectedPawn = Find.Selector.SelectedObjects?.FirstOrDefault(o => o is Pawn && (o as Pawn).drafter != null);
			if (firstSelectedPawn != null)
				return result;

			var map = Find.CurrentMap;
			if (map == null || map.mapPawns == null)
				return result;

			map.mapPawns.FreeColonists.Where(PawnUsable).Do(pawn =>
			{
				var list = FloatMenuMakerMap.ChoicesAtFor(UI.MouseMapPosition(), pawn);
				list.Where(option => option.Label != "Go here").Do(option =>
				{
					var dict = result.GetValueSafe(option.Label);
					if (dict == null)
					{
						dict = new Dictionary<Pawn, FloatMenuOption>();
						result[option.Label] = dict;
					}
					dict[pawn] = option;
				});
			});
			return result;
		}

		public static bool PawnUsable(Pawn pawn)
		{
			return pawn.IsColonistPlayerControlled
				&& pawn.Dead == false
				&& pawn.Spawned
				&& pawn.Downed == false
				&& pawn.Map == Find.CurrentMap;
		}

		public static FloatMenuOption MakeMenuItemForLabel(string label, Dictionary<Pawn, FloatMenuOption> dict)
		{
			var pawns = dict.Keys.ToList();
			var options = dict.Values.ToList();
			var labelFixed = pawns.Count() == 1 ? (pawns[0].Name.ToStringShort + ": " + label) : label;
			var option = new FloatMenuOptionNoClose(labelFixed, () =>
			{
				if (options.Count() == 1 && options[0].Disabled == false)
				{
					var action = options[0].action;
					if (action != null)
					{
						CloseLabelMenu(true);
						action();
					}
				}
				else
				{
					var i = 0;
					var actions = new List<FloatMenuOption>();
					pawns.OrderBy(pawn => -PathInfo.GetPath(pawn).TotalCost).Do(pawn =>
					{
						actions.Add(new FloatMenuOptionPawn(pawn, () =>
						{
							var pawnOption = dict[pawn];
							if (pawnOption != null)
							{
								actionMenu.Close(true);
								CloseLabelMenu(true);
								pawnOption.action();
							}
						},
						(MenuOptionPriority)i++, (Rect rect) => PathInfo.current = pawn));
					});
					actionMenu = new FloatMenuColonists(actions, null);
					Find.WindowStack.Add(actionMenu);
				}

			})
			{
				Disabled = options.All(o => o.Disabled)
			};
			return option;
		}
	}
}
