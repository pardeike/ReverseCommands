using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace ReverseCommands
{
	public class PathInfo
	{
		public static Pawn current;
		static readonly Dictionary<Pawn, PathInfo> storage = new Dictionary<Pawn, PathInfo>();

		public static List<Pawn> GetPawns()
		{
			return storage.Keys.ToList();
		}

		public static void AddInfo(Pawn pawn, IntVec3 cell)
		{
			storage[pawn] = new PathInfo(pawn, cell);
		}

		public static PawnPath GetPath(Pawn pawn)
		{
			var val = storage.GetValueSafe(pawn);
			if (val == null) return null;
			return val.GetPath();
		}

		public static string GetJobReport(Pawn pawn)
		{
			var val = storage.GetValueSafe(pawn);
			if (val == null) return "";
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
					if (path != null) path.ReleaseToPool();
				}
			});
			storage.Clear();
		}

		public Pawn pawn;
		public IntVec3 cell;

		public PawnPath path;
		public IntVec3 lastPawnLocation = IntVec3.Invalid;

		//public string jobDescription;
		//public Job lastJob;

		public PathInfo(Pawn pawn, IntVec3 cell)
		{
			this.pawn = pawn;
			this.cell = cell;
		}

		public PawnPath GetPath()
		{
			var pos = pawn.Position;
			if (lastPawnLocation != pos)
			{
				var traverseParams = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.PassDoors, false);
				if (path != null) path.ReleaseToPool();
				path = pawn.Map.pathFinder.FindPath(pawn.Position, cell, traverseParams, PathEndMode.Touch);
				lastPawnLocation = pos;
			}
			return path;
		}

		public string GetJobReport()
		{
			var m = Math.Floor(GetPath(pawn).TotalCost * 60f / GenDate.TicksPerHour);
			var mins = "";
			if (m > 0) mins = ", [" + m + " min]";
			var job = pawn.jobs.curJob != null ? (", " + pawn.jobs.curDriver.GetReport()) : "";
			if (job.EndsWith(".", StringComparison.Ordinal)) job = job.Substring(0, job.Length - 1);
			return pawn.Name.ToStringShort + job + mins;
		}
	}
}