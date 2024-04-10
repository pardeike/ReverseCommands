using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ReverseCommands
{
	public class PathInfo(Pawn pawn, IntVec3 cell)
	{
		public static Pawn current;
		static readonly Dictionary<Pawn, PathInfo> storage = [];

		public static List<Pawn> GetPawns()
		{
			return [.. storage.Keys];
		}

		public static void AddInfo(Pawn pawn, IntVec3 cell)
		{
			storage[pawn] = new PathInfo(pawn, cell);
		}

		public static PawnPath GetPath(Pawn pawn)
		{
			var val = storage.GetValueSafe(pawn);
			if (val == null)
				return null;
			return val.GetPath();
		}

		public static string GetJobReport(Pawn pawn)
		{
			if (pawn == null)
				return "";
			var val = storage.GetValueSafe(pawn);
			if (val == null)
				return "";
			return val.GetJobReport();
		}

		public static void Clear()
		{
			current = null;

			storage.Values.Do(info =>
			{
				if (info != null)
				{
					var path = info.path;
					path?.ReleaseToPool();
				}
			});
			storage.Clear();
		}

		public Pawn pawn = pawn;
		public IntVec3 cell = cell;

		public PawnPath path;
		public IntVec3 lastPawnLocation = IntVec3.Invalid;

		public PawnPath GetPath()
		{
			var pos = pawn.Position;
			if (lastPawnLocation != pos)
			{
				var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false);
				path?.ReleaseToPool();
				path = pawn.Map.pathFinder.FindPath(pawn.Position, cell, traverseParams, PathEndMode.Touch);
				lastPawnLocation = pos;
			}
			return path;
		}

		public string GetJobReport()
		{
			var m = Math.Floor(GetPath(pawn).TotalCost * 60f / GenDate.TicksPerHour);
			var mins = "";
			if (m > 0)
				mins = ", [" + m + " min]";
			var job = pawn.jobs.curJob != null ? (", " + pawn.jobs.curDriver.GetReport()) : "";
			if (job.EndsWith(".", StringComparison.Ordinal))
				job = job.Substring(0, job.Length - 1);
			return pawn.Name.ToStringShort + job + mins;
		}
	}
}
