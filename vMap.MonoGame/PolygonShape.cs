using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace vMap.MonoGame
{
	public class PolygonShape
	{
		private readonly GraphicsDevice		   _graphicsDevice;
		private readonly VertexPositionColor[] _vertices;
		private readonly VertexPositionColor[] _triangulatedVertices;
		private readonly int[]				   _indices;
		private			 bool				   _triangulated;
		private			 Vector3			   _centerPoint;
		private			 Color				   _color;

		public PolygonShape(GraphicsDevice graphicsDevice, VertexPositionColor[] vertices, Color color)
		{
			_graphicsDevice       = graphicsDevice;
			_vertices             = vertices;
			_triangulated         = false;
			_triangulatedVertices = new VertexPositionColor[vertices.Length * 3];
			_indices              = new int[vertices.Length];
			_color                = color;
		}
		public void Draw(BasicEffect effect)
		{
			try
			{
				if(!_triangulated)
					this.Triangulate();

				effect.CurrentTechnique.Passes[0].Apply();
				_graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _triangulatedVertices, 0, _vertices.Length);
			}
			catch(Exception exc)
			{
				throw new Exception("Something went wrong while drawing the PolygonShape.", exc);
			}
		}
		public VertexPositionColor[] Triangulate()
		{
			this.CalculateCenterPoint();
			this.SetupIndexes();

			foreach(var i in _indices)
				this.SetupDrawableTriangle(i);

			_triangulated = true;

			return _triangulatedVertices;
		}
		private void CalculateCenterPoint()
		{
			var xCount = 0f;
			var yCount = 0f;

			foreach (var vertice in _vertices)
			{
				xCount += vertice.Position.X;
				yCount += vertice.Position.Y;
			}

			_centerPoint = new Vector3(xCount / _vertices.Length, yCount / _vertices.Length, 0);
		}
		private void SetupIndexes()
		{
			for (var i = 1; i < _triangulatedVertices.Length; i = i + 3)
				_indices[i / 3] = i - 1;
		}
		private void SetupDrawableTriangle(int index)
		{
			_triangulatedVertices[index] = _vertices[index / 3];

			if (index / 3 != _vertices.Length - 1)
				_triangulatedVertices[index + 1] = _vertices[(index / 3) + 1];
			else
				_triangulatedVertices[index + 1] = _vertices[0];

			_triangulatedVertices[index + 2].Position = _centerPoint;
			_triangulatedVertices[index + 2].Color    = _color;
		}
	}
}