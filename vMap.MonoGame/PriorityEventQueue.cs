using System;
using System.Collections.Generic;

namespace vMap.MonoGame
{
	public class PriorityEventQueue<T>
	{
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