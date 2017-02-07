using Verse;
using System.Linq;
using System.Collections.Generic;
using Harmony;
using RimWorld;

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
				Find.WindowStack.TryRemove(labelMenu, sound);
				labelMenu = null;
			}
		}

		public static Dictionary<string, Dictionary<Pawn, FloatMenuOption>> GetPawnActions()
		{
			var result = new Dictionary<string, Dictionary<Pawn, FloatMenuOption>>();

			var firstSelectedPawn = Find.Selector.SelectedObjects.FirstOrDefault(o => o is Pawn && (o as Pawn).IsColonist);
			if (firstSelectedPawn != null) return result;

			Find.VisibleMap.mapPawns.FreeColonists.Where(PawnUsable).Do(pawn =>
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
				       && pawn.Map == Find.VisibleMap;
		}

		public static FloatMenuOption MakeMenuItemForLabel(IntVec3 cell, string label, Dictionary<Pawn, FloatMenuOption> dict)
		{
			var pawns = dict.Keys.ToList();
			var options = dict.Values.ToList();
			var labelFixed = pawns.Count() == 1 ? (pawns[0].NameStringShort + ": " + label) : label;
			var option = new FloatMenuOptionNoClose(labelFixed, () => {

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
					int i = 0;
					var actions = new List<FloatMenuOption>();
					pawns.OrderBy(pawn => -PathInfo.GetPath(pawn).TotalCost).Do(pawn =>
					{
						actions.Add(new FloatMenuOptionPawn(pawn, () => {
							var pawnOption = dict[pawn];
							if (pawnOption != null)
							{
								actionMenu.Close(true);
								CloseLabelMenu(true);
								pawnOption.action();
							}
						}, (MenuOptionPriority)i++, () => {
							PathInfo.current = pawn;
						}));
					});
					actionMenu = new FloatMenuColonists(actions, null);
					Find.WindowStack.Add(actionMenu);
				}

			});
			option.Disabled = options.All(o => o.Disabled);
			return option;
		}
	}
}