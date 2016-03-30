using System;
using System.Collections.Generic;

namespace vMap.Voronoi
{
	public class PriorityQueue<T>
	{
		// I'm using an unsorted array for this example, but ideally this
		// would be a binary heap. Find a binary heap class:
		// * https://bitbucket.org/BlueRaja/high-speed-priority-queue-for-c/wiki/Home
		// * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
		// * http://xfleury.github.io/graphsearch.html
		// * http://stackoverflow.com/questions/102398/priority-queue-in-net

		private readonly List<Tuple<T, int>> _elements = new List<Tuple<T, int>>();
		public int Count => _elements.Count;
		public void Enqueue(T item, int priority)
		{
			_elements.Add(Tuple.Create(item, priority));
		}
		public T Dequeue()
		{
			var bestIndex = 0;
			for (var i = 0; i < _elements.Count; i++)
				if (_elements[i].Item2 < _elements[bestIndex].Item2)
					bestIndex = i;

			var bestItem = _elements[bestIndex].Item1;
			_elements.RemoveAt(bestIndex);

			return bestItem;
		}
	}
}
