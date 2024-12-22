using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DevTools.ExtraVariables;
using static UnityEngine.Object;

namespace DevTools.Extensions;

public static class CellsExtensions // Most of the extensions were created specifically for my NullBossStart cutscene
{
    ///<summary>
    /// Returns a dictionary containing the direction with the maximum count of free cells reachable from the specified cell.
    ///</summary>
    ///<param name="fromCell">The starting cell.</param>
    ///<returns>A dictionary with the direction and list of cells with the maximum free cells.</returns>
    public static Dictionary<Direction, List<Cell>> DirectionWithMaxFreeCells(this Cell fromCell)
    {
        var targetPair = new KeyValuePair<Direction, List<Cell>>();
        int maxCount = int.MinValue;

        foreach (var pair in fromCell.GetCellsInAllDirections())
        {
            if (pair.Value.Count > maxCount)
            {
                maxCount = pair.Value.Count;
                targetPair = pair;
            }
        }
        return new Dictionary<Direction, List<Cell>>() { { targetPair.Key, targetPair.Value } };
    }
    ///<summary>
    /// Returns a direction with the maximum count of free cells reachable from the specified position.
    ///</summary>
    ///<param name="fromCell">The starting cell.</param>
    ///<returns>The direction with the maximum free cells.</returns>
    public static Dictionary<Direction, List<Cell>> DirectionWithMaxFreeCells(this Vector3 fromPos) => ec.CellFromPosition(fromPos).DirectionWithMaxFreeCells();
    ///<summary>
    /// Finds the cell of the specified shape with the maximum count of free cells in the main hallway.
    ///</summary>
    ///<param name="shape">The shape of the starting cell.</param>
    ///<returns>The cell of the specified shape with the maximum count of free cells.</returns>
    public static Cell GetCellOfShape_WithMaxFreeCells(TileShapeMask shape)
    {
        var cornersInHallway = (from x in ec.mainHall.GetTilesOfShape(shape, true) select x).ToList();

        int max = int.MinValue;
        foreach (var a in cornersInHallway)
        {
            int count = a.DirectionWithMaxFreeCells().ElementAt(0).Value.Count;
            if (count > max) max = count;
        }

        return (from x in cornersInHallway where x.DirectionWithMaxFreeCells().ElementAt(0).Value.Count == max select x).ElementAt(0);
    }
    ///<summary>
    /// Returns a list with all cells in the specified direction, starting from a specific cell
    ///</summary>
    ///<param name="startCell"></param>
    ///<param name="dir"></param>
    ///<returns>List with all cells in the specified direction</returns>
    public static List<Cell> GetCellsInDirection(this Cell startCell, Direction dir)
    {
        var nextCell = ec.CellFromPosition(startCell.position + dir.ToIntVector2());
        var cellsInDir = new List<Cell>();

        while (nextCell != null && !nextCell.HasWallInDirection(dir))
        {
            cellsInDir.Add(nextCell);
            nextCell = ec.CellFromPosition(nextCell.position + dir.ToIntVector2());
        }
        cellsInDir.Add(nextCell); // Add the last cell that doesn't contains dir to AllOpenNavDirections

        return cellsInDir;
    }
    ///<summary>
    /// Gets a dictionary of cells in all open navigation directions from the start cell.
    ///</summary>
    ///<param name="startCell">The starting cell.</param>
    ///<returns>A dictionary containing cells in all open navigation directions.</returns>
    public static Dictionary<Direction, List<Cell>> GetCellsInAllDirections(this Cell startCell)
    {
        Dictionary<Direction, List<Cell>> result = new();

        foreach (var direction in startCell.AllOpenNavDirections)
        {
            result.Add(direction, startCell.GetCellsInDirection(direction));
        }

        return result;
    }
    ///<summary>
    /// An extension that converts a Vector3 position to Cell.
    ///</summary>
    ///<param name="vector"></param>
    ///<returns>Cell with the specified position</returns>
    public static Cell ToCell(this Vector3 vector) => Singleton<BaseGameManager>.Instance.Ec.CellFromPosition(vector);
    ///<summary>
    /// An extension that converts a Tile object to Cell.
    ///</summary>
    ///<param name="tile"></param>
    ///<returns>Cell from the tile object</returns>
    public static Cell ToCell(this Tile tile) 
    {
        var v = IntVector2.GetGridPosition(tile.transform.position);
        return ec.cells[v.x, v.z];
    }
    ///<summary>
    /// Navigable Distance from start position to end position, measured in count of cells
    ///</summary>
    ///<param name="startPos"></param>
    ///<param name="endPos"></param>
    ///<returns>Count of cells required to reach the final position</returns>
    public static int NavDistanceTo(this Vector3 startPos, Vector3 endPos) => ec.CellFromPosition(startPos).NavDistanceTo(endPos);
    ///<summary>
    /// Navigable Distance from start cell to end position, measured in count of cells
    ///</summary>
    ///<param name="startCell"></param>
    ///<param name="endPos"></param>
    ///<returns>Count of cells required to reach the final position</returns>
    public static int NavDistanceTo(this Cell startCell, Vector3 endPos) => ec.NavigableDistance(startCell, ec.CellFromPosition(endPos), PathType.Nav);
}
