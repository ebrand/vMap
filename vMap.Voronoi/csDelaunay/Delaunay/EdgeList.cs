using System;
using vMap.Voronoi.csDelaunay.Geom;

namespace vMap.Voronoi.csDelaunay.Delaunay
{
	[Serializable]
	public class EdgeList
	{
		private readonly float _deltaX;
		private readonly float _xMin;
		private readonly int _hashSize;
		private Halfedge[] _hash;

		public EdgeList(float xMin, float deltaX, int sqrtSitesNb)
		{
			_xMin = xMin;
			_deltaX = deltaX;
			_hashSize = 2 * sqrtSitesNb;
			_hash = new Halfedge[_hashSize];

			// Two dummy Halfedges:
			this.LeftEnd = Halfedge.CreateDummy();
			this.RightEnd = Halfedge.CreateDummy();
			this.LeftEnd.EdgeListLeftNeighbor = null;
			this.LeftEnd.EdgeListRightNeighbor = this.RightEnd;

			this.RightEnd.EdgeListLeftNeighbor = this.LeftEnd;
			this.RightEnd.EdgeListRightNeighbor = null;

			_hash[0] = this.LeftEnd;
			_hash[_hashSize - 1] = this.RightEnd;
		}

		public Halfedge LeftEnd { get; private set; }
		public Halfedge RightEnd { get; private set; }

		public void Dispose()
		{
			var halfedge = this.LeftEnd;
			while (halfedge != this.RightEnd)
			{
				var prevHe = halfedge;
				halfedge = halfedge.EdgeListRightNeighbor;
				prevHe.Dispose();
			}
			this.LeftEnd = null;
			this.RightEnd.Dispose();
			this.RightEnd = null;
			_hash = null;
		}
		public void Insert(Halfedge leftNeighbor, Halfedge newHalfedge)
		{
			newHalfedge.EdgeListLeftNeighbor = leftNeighbor;
			newHalfedge.EdgeListRightNeighbor = leftNeighbor.EdgeListRightNeighbor;
			leftNeighbor.EdgeListRightNeighbor.EdgeListLeftNeighbor = newHalfedge;
			leftNeighbor.EdgeListRightNeighbor = newHalfedge;
		}
		public void Remove(Halfedge halfedge)
		{
			// This function only removes the Halfedge from the left-right list.
			// We cannot dispose it yet because we are still using it.
			// param halfEdge

			halfedge.EdgeListLeftNeighbor.EdgeListRightNeighbor = halfedge.EdgeListRightNeighbor;
			halfedge.EdgeListRightNeighbor.EdgeListLeftNeighbor = halfedge.EdgeListLeftNeighbor;
			halfedge.Edge = Edge.DELETED;
			halfedge.EdgeListLeftNeighbor = halfedge.EdgeListRightNeighbor = null;
		}
		public Halfedge GetLeftNeighbor(Vector2f p)
		{
			//  Find the rightmost Halfedge that is still elft of p

			// Use hash table to get close to desired halfedge
			var bucket = (int)((p.X - _xMin) / _deltaX * _hashSize);

			if (bucket < 0)
				bucket = 0;

			if (bucket >= _hashSize)
				bucket = _hashSize - 1;

			var halfedge = this.GetHash(bucket);

			if (halfedge == null)
			{
				for (var i = 0; true; i++)
				{
					if ((halfedge = this.GetHash(bucket - i)) != null)
						break;

					if ((halfedge = this.GetHash(bucket + i)) != null)
						break;
				}
			}
			
			// Now search linear list of haledges for the correct one
			if ((halfedge == this.LeftEnd) || ((halfedge != this.RightEnd) && halfedge.IsLeftOf(p)))
			{
				do
					halfedge = halfedge.EdgeListRightNeighbor;
				while ((halfedge != this.RightEnd) && halfedge.IsLeftOf(p));

				halfedge = halfedge.EdgeListLeftNeighbor;
			}
			else {
				do
					halfedge = halfedge.EdgeListLeftNeighbor;
				while ((halfedge != this.LeftEnd) && !halfedge.IsLeftOf(p));
			}

			// Update hash table and reference counts
			if ((bucket > 0) && (bucket < _hashSize - 1))
				_hash[bucket] = halfedge;

			return halfedge;
		}

		private Halfedge GetHash(int b)
		{
			// Get entry from the has table, pruning any deleted nodes

			if ((b < 0) || (b >= _hashSize))
				return null;

			var halfedge = _hash[b];

			if ((halfedge != null) && (halfedge.Edge == Edge.DELETED))
			{
				// Hash table points to deleted halfedge. Patch as necessary
				_hash[b] = null;
				
				// Still can't dispose halfedge yet!
				return null;
			}
			else
				return halfedge;
		}
	}
}