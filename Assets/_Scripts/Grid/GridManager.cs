using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public enum DisplayType { Area, Arrow }
public enum AreaType { Select, Move, Attack, Buff, Arrange }

public class GridManager : MonoBehaviour
{
    public static GridManager inst;
    public Dictionary<Vector2, HexNode> Tiles { get; private set; }
    private void Awake()
    {
        inst = this;

        Tiles = scriptableGrid.GenerateGrid();
        foreach (var tile in Tiles.Values) tile.CacheNeighbors();
    }

    [SerializeField] private ScriptableGrid scriptableGrid;
    public HexNode selectedNode;

    [SerializeField] private RangeDisplay rangeDisplayPrefab;
    [SerializeField] private Sprite areaSprite;
    [SerializeField] private Color selectAreaColor;
    [SerializeField] private Color moveAreaColor;
    [SerializeField] private Color attackAreaColor;
    [SerializeField] private Color buffAreaColor;
    [SerializeField] private Color arrangeAreaColor;
    [SerializeField] private RuntimeAnimatorController hatch;

    public void AreaDisplay(AreaType areaType, bool canSelect, List<HexNode> tiles, Unit unit)
    {
        foreach (var t in tiles)
        {
            var color = areaType switch
            {
                AreaType.Select => selectAreaColor,
                AreaType.Move => moveAreaColor,
                AreaType.Attack => attackAreaColor,
                AreaType.Buff => buffAreaColor,
                AreaType.Arrange => arrangeAreaColor,
                _ => Color.white
            };

            RangeDisplay rangeDisplay = null;
            if (t.RangeDisplays.Count > 0)
            {
                rangeDisplay = t.RangeDisplays.Find(display => !display.Active);
            }
            else
            {
                rangeDisplay = Instantiate(rangeDisplayPrefab, t.transform.GetChild(0));
                rangeDisplay.transform.localPosition = Vector2.zero;
            }
            
            var displaySpriteRenderer = rangeDisplay.GetComponent<SpriteRenderer>();
            displaySpriteRenderer.color = canSelect ? new Color(color.r, color.g, color.b, 0.2f) : new Color(color.r, color.g, color.b, 0.5f);
            foreach (var direction in HexDirectionExtension.Loop())
            {
                var isContain = !tiles.Contains(GetNode(t.Coords + direction.Coords()));
                rangeDisplay.transform.GetChild((int)direction).gameObject.SetActive(isContain);
                if (isContain)
                    rangeDisplay.transform.GetChild((int)direction).GetComponent<SpriteRenderer>().color = color;
            }
            rangeDisplay.Setup(areaType, canSelect, unit);
        }
    }

    public void RevertTiles(Unit unit = null)
    {
        //print("RevertTiles / Unit: " + unit.data.name);
        foreach (var t in Tiles.Values) t.RevertTile(unit);
        RevertAbles();
    }
    public void RevertAbles()
    {
        foreach (var t in Tiles.Values) t.RevertAble();
    }
    /*public List<GameObject> InstantiateSelectNodes(List<HexNode> tiles)
    {
        var outlines = new List<GameObject>();
        foreach (HexNode t in tiles)
        {
            var outline = Instantiate(this.outline, t.transform);
            outline.transform.localPosition = Vector2.zero;
            t.SetOutline(outline, tiles);
            outlines.Add(outline);
            outline.SetActive(false);
        }
        return outlines;
    }*/
    public void SelectNode(HexNode node, bool isSelect = true)
    {
        if (!isSelect)
            selectedNode = null;
        else if (node != selectedNode)
            selectedNode = node;
    }

    public bool ContainNode(Vector2 pos) => Tiles.ContainsKey(pos);
    #region SetTileUnit
    public void SetTileUnit(HexCoords hexCoords, Unit unit)
    {
        GetNode(hexCoords).PutUnit(unit);
    }
    public bool SetTileUnit(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
        if (prevCoords == nextCoords)
            return false;
        if (GetNode(nextCoords)?.CanWalk() != true)
            return false;

        GetNode(nextCoords).PutUnit(unit);
        GetNode(prevCoords).RemoveUnit(unit);
        return true;
    }
    public void SetTileUnitRemove(Unit unit)
    {
        GetNode(unit).RemoveUnit(unit);
    }
    #endregion    
    #region GetTile
    public HexNode GetNode(Unit unit) => Tiles.ContainsKey(unit.coords.Pos) ? Tiles.TryGetValue(unit.coords.Pos, out var tile) ? tile : null : null;
    public HexNode GetNode(HexCoords coords) => Tiles.ContainsKey(coords.Pos) ? Tiles.TryGetValue(coords.Pos, out var tile) ? tile : null : null;
    public HexNode GetNode(Vector2 pos) => Tiles.ContainsKey(pos) ? Tiles.TryGetValue(pos, out var tile) ? tile : null : null;
    public List<HexNode> GetTiles(List<HexCoords> coordses)
    {
        List<HexNode> tiles = new();
        foreach (HexCoords c in coordses)
        {
            if (Tiles.ContainsKey(c.Pos))
                tiles.Add(Tiles[c.Pos]);
        }
        return tiles;
    }
    #endregion
    #region GetUnit
    public Unit GetUnit(HexNode hexNode) => hexNode.OnUnit ? hexNode.Unit : null;
    public Unit GetUnit(HexCoords hexCoords)
    {
        var hexNode = GetNode(hexCoords);
        return hexNode.OnUnit ? hexNode.Unit : null;
    }
    public Unit GetUnit(Vector2 pos)
    {
        var hexNode = GetNode(pos);
        return hexNode.OnUnit ? hexNode.Unit : null;
    }
    #endregion
    
    public HexNode GetRandomNode() => Tiles.Where(t => t.Value.CanWalk()).OrderBy(t => Random.value).First().Value;
    public void StatusNode()
    {
        foreach (var tile in Tiles.Where(t => t.Value.OnUnit && t.Value.statuses.Count != 0))
        {
            print(tile.Value.statuses[0].data.name);
            StartCoroutine(StatusManager.Inst.AddUnitStatus(tile.Value.statuses, GetUnit(tile.Value)));
        }
    }
}