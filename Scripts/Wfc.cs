using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Wfc : Node
{
    [Export] string _groundTag = "Ground";
    [Export] string _airTag = "Air";
    [Export] CellObjectStates _cellsList;
    [Export] Vector3I _generateSize = new(5, 3, 5);
    [Export] float _separation = 5f;

    public override void _Ready()
    {
        SolveWFC();
    }

    internal struct Cell
    {
        internal Vector3I Position;
        internal int Entropy;
        internal CellObject State;
    }

    void SolveWFC()
    {
        int center = (_generateSize.X * _generateSize.Y * _generateSize.Z - _generateSize.Y) / 2;

        Cell[] InitGrid(Vector3I size)
        {
            Cell[] cells = new Cell[size.X * size.Y * size.Z];
            int i = 0;
            for (int x = 0; x < size.X; x++)
            {
                for (int z = 0; z < size.Z; z++)
                {
                    for (int y = 0; y < size.Y; y++)
                    {
                        cells[i].Position = new Vector3I(x, y, z);
                        cells[i].Entropy = _cellsList.PossibleStates.Length * 4;
                        i++;
                    }
                }
            }

            return cells;
        }

        Cell[] Collapse(Cell[] Grid, CellObject[] possibleStates)
        {
            string FetchSocket(int i, CellObject check)
            {
                switch (i)
                {
                    case 0: return check.PosX;
                    case 1: return check.PosY;
                    case 2: return check.PosZ;
                    case 3: return check.NegX;
                    case 4: return check.NegY;
                    default: return check.NegZ;
                }
            }

            int[] GetNeighbors(int i)
            {
                int[] neighbors = new int[6];

                int xOffset = _generateSize.Y * _generateSize.Z;

                neighbors[0] = i + xOffset; // +X
                neighbors[1] = i + 1; // +Y
                neighbors[2] = i + _generateSize.Y; // +Z
                neighbors[3] = i - xOffset; // -X
                neighbors[4] = i - 1; // -Y
                neighbors[5] = i - _generateSize.Y; // -Z

                for (int j = 0; j < 6; j++)
                {
                    neighbors[j] = Mathf.Min(Mathf.Max(0, neighbors[j]), Grid.Length - 1);
                }

                return neighbors;
            }

            CellObject[] GetPossibleCells(int index)
            {
                bool MatchYAxis(int yPos, CellObject cellObject)
                {
                    if (yPos == 0 && cellObject.NegY != _groundTag && _groundTag != "")
                        return false;
                    if (yPos == _generateSize.Y - 1 && cellObject.PosY != _airTag && _airTag != "")
                        return false;

                    return true;
                }

                bool OnAxis(int yPos, int t)
                {
                    if ((yPos == 0 && t == 4) || (yPos == _generateSize.Y - 1 && t == 1))
                        return true;

                    return false;
                }

                List<CellObject> possible = possibleStates.ToList();

                int[] nthNeighbors = GetNeighbors(index);
                for (int t = 0; t < 6; t++)
                {
                    if (OnAxis(Grid[index].Position.Y, t))
                    {
                        for (int j = possible.Count - 1; j >= 0; j--)
                        {
                            if (!MatchYAxis(Grid[index].Position.Y, possible[j]))
                                possible.RemoveAt(j);
                        }

                        continue;
                    }

                    if (Grid[index].Position[t % 3] != Grid[nthNeighbors[t]].Position[t % 3] + t / 3 * 2 - 1)
                        continue;

                    if (Grid[nthNeighbors[t]].State == null)
                        continue;

                    string socket = FetchSocket((t + 3) % 6, Grid[nthNeighbors[t]].State);
                    for (int j = possible.Count - 1; j >= 0; j--)
                    {
                        bool correctRot = false;
                        if (t % 3 == 1)
                            correctRot = possible[j].Rotate90 != Grid[nthNeighbors[t]].State.Rotate90 || possible[j].Rotate180 != Grid[nthNeighbors[t]].State.Rotate180;

                        if (FetchSocket(t, possible[j]) != socket || correctRot)
                            possible.RemoveAt(j);
                    }
                }

                return possible.ToArray();
            }

            void ReduceEntropy(int index)
            {
                int[] nInd = GetNeighbors(index);

                for (int i = 0; i < 6; i++)
                {
                    if (Grid[index].Position[i % 3] != Grid[nInd[i]].Position[i % 3] + i / 3 * 2 - 1 || Grid[nInd[i]].Entropy > possibleStates.Length)
                        continue;

                    CellObject[] possible = GetPossibleCells(nInd[i]);

                    Grid[nInd[i]].Entropy = possible.Length;
                }
            }

            int FindLowestEntropy()
            {
                int notDone = 0;
                List<int> ties = new List<int>();
                int lowestIndex = 0;
                for (int i = 0; i < Grid.Length; i++)
                {
                    if (Grid[i].Entropy == Grid[lowestIndex].Entropy)
                    {
                        ties.Add(i);
                        continue;
                    }

                    if (Grid[i].Entropy > Grid[lowestIndex].Entropy) continue;

                    lowestIndex = i;
                    ties.Clear();
                }

                if (ties.Count == 0) return lowestIndex;

                var rng = new RandomNumberGenerator();

                ties.Add(lowestIndex);
                return ties[rng.RandiRange(0, ties.Count - 1)];
            }

            Grid[center].State = _cellsList.StartingCell;
            Grid[center].Entropy = possibleStates.Length + 1;

            ReduceEntropy(center);

            while (true)
            {
                int check = FindLowestEntropy();

                if (Grid[check].Entropy > possibleStates.Length)
                    break;


                var rng = new RandomNumberGenerator();
                CellObject[] possible = GetPossibleCells(check);

                if (possible.Length == 0)
                {
                    GD.Print("AAA");
                    
                    int[] neighbors = GetNeighbors(check);

                    for (int i = 0; i < neighbors.Length; i++)
                    {
                        if (Grid[neighbors[i]].State == _cellsList.StartingCell)
                            continue;

                        Grid[neighbors[i]].Entropy = _cellsList.PossibleStates.Length * 4;
                        Grid[neighbors[i]].State = null;
                    }

                    ReduceEntropy(check);
                    continue;
                }

                Grid[check].State = possible[rng.RandiRange(0, possible.Length - 1)];

                Grid[check].Entropy = possibleStates.Length + 1;

                ReduceEntropy(check);
            }

            return Grid;
        }

        Cell[] grid = Collapse(InitGrid(_generateSize), _cellsList.GenerateRotatedStates());

        void InstanceCells(Cell[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                CellNode node = new CellNode();
                AddChild(node);

                Vector3 positionOffset = new Vector3(_generateSize.X - 1, 0, _generateSize.Z - 1) * _separation * 0.5f;
                node.GlobalPosition = (Vector3)cells[i].Position * _separation - positionOffset;
                node.Name = "Cell " + i;

                Node3D obj = cells[i].State.Object.Instantiate() as Node3D;

                float rot = cells[i].State.Rotate90 ? 90 : 0;
                rot += cells[i].State.Rotate180 ? 180 : 0;

                obj.RotationDegrees += new Vector3(0, rot, 0);
                node.AddChild(obj);
            }
        }

        InstanceCells(grid);
    }
}