using System;
using System.Collections.Generic;
using System.Drawing;

namespace vMap.Voronoi
{
	public sealed class AStarSearch
	{
		public event EventHandler SiteUpdated;
		public event EventHandler SearchComplete;

		private readonly IWeightedGraph<Site, Coordinate, PointF> _graph;
		private readonly PriorityQueue<Site> _frontier;
		public Dictionary<Site, Site> CameFrom = new Dictionary<Site, Site>();
		public Dictionary<Site, int> CostSoFar = new Dictionary<Site, int>();
		public Site Start;
		public Site Goal;
		
		public static int Heuristic(Site a, Site b)
		{
			return (int)(Math.Abs(a.Point.X - b.Point.X) + Math.Abs(a.Point.Y - b.Point.Y));
		}
		public AStarSearch(IWeightedGraph<Site, Coordinate, PointF> graph, Site start, Site goal)
		{
			_graph = graph;
			this.Start = start;
			this.Goal = goal;
			_frontier = new PriorityQueue<Site>();
			this.Start.AddState(SiteState.AStarSearchStart);
			this.Goal.AddState(SiteState.AStarSearchGoal);
		}
		public void Search()
		{
			try
			{
				_frontier.Enqueue(Start, 0);

				this.CameFrom[Start] = Start;
				this.CostSoFar[Start] = 0;

				_graph.Visited.Add(Start);

				while(_frontier.Count > 0)
				{
					var current = _frontier.Dequeue();

					if(current == null)
						continue;

					if(current.Equals(this.Goal))
						break;

					foreach(var next in _graph.PrimaryNeighbors(current.Point))
					{
						if (this.CameFrom.ContainsKey(next))
							continue;

						var newCost = this.CostSoFar[current] + _graph.Cost(current, next);

						if(this.CostSoFar.ContainsKey(next) && (newCost >= CostSoFar[next]))
							continue;

						this.CostSoFar[next] = newCost;
						_frontier.Enqueue(next, newCost + AStarSearch.Heuristic(next, this.Goal));
						this.CameFrom[next] = current;
						_graph.Visited.Add(next);
					}
				}
			}
			catch(Exception exc)
			{
				Utilities.Debug(exc.Message, VoronoiTraceLevel.Error);
			}
			finally
			{
				this.SearchComplete?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}