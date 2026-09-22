using Godot;

[GlobalClass]
public partial class CellObjectStates : Resource
{
    [Export] internal CellObject StartingCell;
    [Export] internal CellObject[] PossibleStates;

    internal CellObject[] GenerateRotatedStates()
    {
        CellObject RotateSockets(CellObject cell)
        {
            string tmp = cell.PosX;
            cell.PosX = cell.PosZ;
            cell.PosZ = cell.NegX;
            cell.NegX = cell.NegZ;
            cell.NegZ = tmp;

            return cell;
        }

        CellObject[] cells = new CellObject[PossibleStates.Length * 4];

        for (int i = 0; i < cells.Length; i++)
        {
            int rotation = Mathf.FloorToInt((float)i / PossibleStates.Length);
            cells[i] = PossibleStates[i % PossibleStates.Length].Duplicate() as CellObject;
            
            for (int j = 0; j < rotation; j++)
            {
                cells[i] = RotateSockets(cells[i]);
            }

            cells[i].Rotate90 = rotation % 2 != 0;
            cells[i].Rotate180 = rotation > 1;
        }

        return cells;
    }
}