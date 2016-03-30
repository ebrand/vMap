using System;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class HalfedgePriorityQueue
	{
		private readonly int _hashSize;
		private readonly float _ymin;
		private readonly float _deltaY;
		private Halfedge[] _hash;
		private int _count;
		private int _minBucket;

		public HalfedgePriorityQueue(float ymin, float deltaY, int sqrtSitesNb)
		{
			_ymin = ymin;
			_deltaY = deltaY;
			_hashSize = 4 * sqrtSitesNb;

			this.Init();
		}

		public bool IsEmpty()
		{
			return _count == 0;
		}
		private bool IsEmpty(int bucket)
		{
			return _hash[bucket].NextInPriorityQueue == null;
		}
		public Vector2f GetMin()
		{
			// return coordinates of the Halfedge's vertex in V*, the transformed Voronoi diagram

			this.AdjustMinBucket();
			var halfEdge = _hash[_minBucket].NextInPriorityQueue;
			return new Vector2f(halfEdge.Vertex.X, halfEdge.Ystar);
		}
		public Halfedge ExtractMin()
		{
			// Remove and return the min Halfedge

			// Get the first real Halfedge in minBucket
			var halfEdge = _hash[_minBucket].NextInPriorityQueue;

			_hash[_minBucket].NextInPriorityQueue = halfEdge.NextInPriorityQueue;
			_count--;
			halfEdge.NextInPriorityQueue = null;

			return halfEdge;
		}
		public void Dispose()
		{
			for (var i = 0; i < _hashSize; i++)
				_hash[i].Dispose();

			_hash = null;
		}
		public void Init()
		{
			_count = 0;
			_minBucket = 0;
			_hash = new Halfedge[_hashSize];
			
			// Dummy Halfedge at the top of each hash
			for (var i = 0; i < _hashSize; i++)
			{
				_hash[i] = Halfedge.CreateDummy();
				_hash[i].NextInPriorityQueue = null;
			}
		}
		public void Insert(Halfedge halfedge)
		{
			if(halfedge == null)
				throw new ArgumentNullException(nameof(halfedge));

			Halfedge next;
			var insertionBucket = GetBucket(halfedge);

			if (insertionBucket < _minBucket)
				_minBucket = insertionBucket;

			var previous = _hash[insertionBucket];

			while(
				((next = previous.NextInPriorityQueue) != null)
				&& (
					(halfedge.Ystar > next.Ystar)
					|| ((Math.Abs(halfedge.Ystar - next.Ystar) < Utilities.EPSILON) && (halfedge.Vertex.X > next.Vertex.X))
				)
			){
				previous = next;
			}

			halfedge.NextInPriorityQueue = previous.NextInPriorityQueue;
			previous.NextInPriorityQueue = halfedge;
			_count++;
		}
		public void Remove(Halfedge halfedge)
		{
			if(halfedge == null)
				throw new ArgumentNullException(nameof(halfedge));

			var removalBucket = this.GetBucket(halfedge);

			if (halfedge.Vertex == null)
				return;

			var previous = _hash[removalBucket];

			while (previous.NextInPriorityQueue != halfedge)
				previous = previous.NextInPriorityQueue;

			previous.NextInPriorityQueue = halfedge.NextInPriorityQueue;
			_count--;

			// after removal from the hash table, dispose of the provided halfEdge
			halfedge.Vertex = null;
			halfedge.NextInPriorityQueue = null;
			halfedge.Dispose();
		}
		private int GetBucket(Halfedge halfedge)
		{
			var bucket = (int)((halfedge.Ystar - _ymin) / _deltaY * _hashSize);
			if (bucket < 0) bucket = 0;
			if (bucket >= _hashSize) bucket = _hashSize - 1;

			return bucket;
		}
		private void AdjustMinBucket()
		{
			// move minBucket until it contains an actual Halfedge (not just the dummy at the top);

			while ((_minBucket < _hashSize - 1) && this.IsEmpty(_minBucket))
				_minBucket++;
		}
	}
}