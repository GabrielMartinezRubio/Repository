using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
	public SquareGrid squareGrid;
	public MeshFilter walls;
	List<Vector3> vertices;
	List<int> triangles;
	Dictionary<int, List<Triangles>> trianglesDictionary = new Dictionary<int, List<Triangles>>(); //Con un valor
	List<List<int>> outLines = new List<List<int>>();
	HashSet<int> checkedVertices = new HashSet<int>(); //Sin un valor

	public void GenerateMesh(int[,] map, float squareSize)
	{
		trianglesDictionary.Clear();
		outLines.Clear();
		checkedVertices.Clear();
		squareGrid = new SquareGrid(map, squareSize);
		vertices = new List<Vector3>();
		triangles = new List<int>();
		for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
		{
			for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
			{
				TriangleSquare(squareGrid.squares[x, y]); 
			}
		}
		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();
		CreateWallMesh();
	}
	void CreateWallMesh()
	{
		CalculatedMeshOutLines();
		List<Vector3> wallVertices = new List<Vector3>();
		List<int> wallTriangles = new List<int>();
		Mesh wallMesh = new Mesh();
		float wallHeight = 5;
		foreach (List<int> outLine in outLines)
		{
			for (int i = 0; i < outLine.Count - 1; i++)
			{
				int startIndex = wallVertices.Count;
				wallVertices.Add(vertices[outLine[i]]); //Izquierda
				wallVertices.Add(vertices[outLine[i + 1]]); //Derecha
				wallVertices.Add(vertices[outLine[i]]-  Vector3.up * wallHeight); //Abajo Izquierda
				wallVertices.Add(vertices[outLine[i + 1]] - Vector3.up * wallHeight); //Abajo Derecha

				wallTriangles.Add(startIndex + 0);
				wallTriangles.Add(startIndex + 2);
				wallTriangles.Add(startIndex + 3);

				wallTriangles.Add(startIndex + 3);
				wallTriangles.Add(startIndex + 1);
				wallTriangles.Add(startIndex + 0);
			}
		}
		wallMesh.vertices = wallVertices.ToArray();
		wallMesh.triangles = wallTriangles.ToArray();
		walls.mesh = wallMesh;
	}
	void TriangleSquare(Square square)
	{
		switch (square.configuration)
		{
			case 0:
				break;
			case 1: //1 solo punto 
				MeshFromPoints(square.centreLeft, square.centreBotton, square.bottonLeft);
				break;
			case 2: //1 solo punto 
				MeshFromPoints(square.bottonRight, square.centreBotton, square.centreRight);
				break;
			case 4: //1 solo punto 
				MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
				break;
			case 8: //1 solo punto 
				MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
				break;
			//--------------------------------------------------------------------------
			case 3: //2 puntos
				MeshFromPoints(square.centreRight, square.bottonRight, square.bottonLeft, square.centreLeft);
				break;
			case 6: //2 puntos
				MeshFromPoints(square.centreTop, square.topRight, square.bottonRight, square.centreBotton);
				break;
			case 9: //2 puntos
				MeshFromPoints(square.topLeft, square.centreTop, square.centreBotton, square.bottonLeft);
				break;
			case 12: //2 puntos
				MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
				break;
			case 5: //2 puntos
				MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBotton, square.bottonLeft, square.centreLeft);
				break;
			case 10: //2 puntos
				MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottonRight, square.centreBotton, square.centreLeft);
				break;
			//--------------------------------------------------------------------------
			case 7: //3 puntos
				MeshFromPoints(square.centreTop, square.topRight, square.bottonRight, square.bottonLeft, square.centreLeft);
				break;			
			case 11: //3 puntos
				MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottonRight, square.bottonLeft);
				break;			
			case 13: //3 puntos
				MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBotton, square.bottonLeft);
				break;			
			case 14: //3 puntos
				MeshFromPoints(square.topLeft, square.topRight, square.bottonRight, square.centreBotton, square.centreLeft);
				break;
			//--------------------------------------------------------------------------
			case 15: //4 puntos
				MeshFromPoints(square.topLeft, square.topRight, square.bottonRight, square.bottonLeft);
				checkedVertices.Add(square.topLeft.vertexIndex);
				checkedVertices.Add(square.topRight.vertexIndex);
				checkedVertices.Add(square.bottonRight.vertexIndex);
				checkedVertices.Add(square.bottonLeft.vertexIndex);
				break;
		}
	}
	void MeshFromPoints(params Node[] points)
	{
		AssignVertices(points);
		if (points.Length >= 3)	CreateTriangles(points[0], points[1], points[2]);
		if (points.Length >= 4)	CreateTriangles(points[0], points[2], points[3]);
		if (points.Length >= 5)	CreateTriangles(points[0], points[3], points[4]);
		if (points.Length >= 6)	CreateTriangles(points[0], points[4], points[5]);
	}
	void AssignVertices(Node[] points)
	{
		for (int i = 0; i < points.Length; i++)
		{
			if (points[i].vertexIndex == -1)
			{
				points[i].vertexIndex = vertices.Count;
				vertices.Add(points[i].position);
			}
		}
	}
	void CreateTriangles(Node a, Node b, Node c)
	{
		triangles.Add(a.vertexIndex);
		triangles.Add(b.vertexIndex);
		triangles.Add(c.vertexIndex);
		Triangles triangle = new Triangles(a.vertexIndex, b.vertexIndex, c.vertexIndex);
		AddTrianglesToDictionary(triangle.vertexIndexA, triangle);
		AddTrianglesToDictionary(triangle.vertexIndexB, triangle);
		AddTrianglesToDictionary(triangle.vertexIndexC, triangle);
	}
	void AddTrianglesToDictionary(int vertexIndexKey, Triangles triangle)
	{
		if (trianglesDictionary.ContainsKey(vertexIndexKey))
		{
			trianglesDictionary[vertexIndexKey].Add(triangle);
		}
		else
		{
			List<Triangles> triangleList = new List<Triangles>();
			triangleList.Add(triangle);
			trianglesDictionary.Add(vertexIndexKey, triangleList);
		}
	}
	void CalculatedMeshOutLines() 
	{
		for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
		{
			if (!checkedVertices.Contains(vertexIndex))
			{
				int newOutLineVertex = GetConnectedOutLineVertex(vertexIndex);
				if (newOutLineVertex != -1)
				{
					checkedVertices.Add(vertexIndex);
					List<int> newOutLine = new List<int>();
					newOutLine.Add(vertexIndex);
					outLines.Add(newOutLine);
					FollowOutLine(vertexIndex, outLines.Count - 1);
					outLines[outLines.Count - 1].Add(vertexIndex);
				}
			}
		}
	}
	void FollowOutLine(int vertexIndex, int outLineIndex)
	{
		outLines[outLineIndex].Add(vertexIndex);
		checkedVertices.Add(vertexIndex);
		int nextVertexIndex = GetConnectedOutLineVertex(vertexIndex);
		if (nextVertexIndex != -1)
		{
			FollowOutLine(nextVertexIndex, outLineIndex);
		}
	}
	int GetConnectedOutLineVertex(int vertexIndex)
	{
		List<Triangles> trianglesContainingVertex = trianglesDictionary[vertexIndex];
		for (int i = 0; i < trianglesContainingVertex.Count; i++)
		{
			Triangles triangle = trianglesContainingVertex[i];
			for (int j = 0; j < 3; j++)
			{
				int vertexB = triangle[j];
				if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
				{
					if (IsOutLineEdge(vertexIndex, vertexB))
					{
						return vertexB;
					}
				}
			}
		}
		return -1;
	}
	bool IsOutLineEdge(int vertexA, int vertexB)
	{
		List<Triangles> trianglesContainingVertexA = trianglesDictionary[vertexA];
		int sharedTriangleCount = 0;
		for (int i = 0; i < trianglesContainingVertexA.Count; i++)
		{
			if (trianglesContainingVertexA[i].Contains(vertexB))
			{
				sharedTriangleCount++;
				if (sharedTriangleCount > 1)
				{
					break;
				}
			}
		}
		return sharedTriangleCount == 1;
	}
	struct Triangles
	{
		public int vertexIndexA;
		public int vertexIndexB;
		public int vertexIndexC;
		int[] vertices;
		public Triangles(int a, int b, int c)
		{
			vertexIndexA = a;
			vertexIndexB = b;
			vertexIndexC = c;
			vertices = new int[3];
			vertices[0] = a;
			vertices[1] = b;
			vertices[2] = c;
		}
		public int this [int i]
		{
			get
			{
				return vertices[i];
			}
		}
		public bool Contains(int vertexIndex)
		{
			return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
		}
	}
	public class SquareGrid
	{
		public Square[,] squares;
		public SquareGrid(int[,] map, float squareSize)
		{
			int nodeCountX = map.GetLength(0);
			int nodeCountY = map.GetLength(1);
			float mapWidth = nodeCountX * squareSize;
			float mapHeight = nodeCountY * squareSize;
			ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];
			for (int x = 0; x < nodeCountX; x++)
			{
				for (int y = 0; y < nodeCountY; y++)
				{
					Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, 0, -mapHeight / 2 + y * squareSize + squareSize / 2);
					controlNodes[x, y] = new ControlNode(pos, map[x, y] == 1, squareSize);
				}
			}
			squares = new Square[nodeCountX - 1, nodeCountY - 1];
			for (int x = 0; x < nodeCountX - 1; x++)
			{
				for (int y = 0; y < nodeCountY - 1; y++)
				{
					squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
					//print(x + " , " + y);
				}
			}
		}
	}
	public class Square
	{
		public ControlNode topLeft, topRight, bottonRight, bottonLeft;
		public Node centreTop, centreRight, centreBotton, centreLeft;
		public int configuration;
		public Square(ControlNode topL, ControlNode topR, ControlNode bottonR, ControlNode bottonL)
		{
			topLeft = topL;
			topRight = topR;
			bottonRight = bottonR;
			bottonLeft = bottonL;
			//----------
			centreTop = topLeft.right; 
			centreRight = bottonRight.above; 
			centreBotton = bottonLeft.right; 
			centreLeft = bottonLeft.above;
			if (topLeft.active) configuration += 8;
			if (topRight.active) configuration += 4;
			if (bottonRight.active) configuration += 2;
			if (bottonLeft.active) configuration += 1;
		}
	}
    public class Node
	{
		public Vector3 position;
		public int vertexIndex = -1;
		public Node(Vector3 pos)
		{
			position = pos;
		}
	}
	public class ControlNode : Node
	{
		public bool active;
		public Node above, right;
		public ControlNode(Vector3 pos, bool act, float squareSize) : base(pos)
		{
			active = act;
			above = new Node(position + Vector3.forward * squareSize / 2f);
			right = new Node(position + Vector3.right * squareSize / 2f);
		}
	}
}
