using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum DisplayType { Area, Arrow }
public enum AreaType { Select, Move, Attack, Buff }

public class GridManager : MonoBehaviour
{
    public static GridManager Inst;
    void Awake()
    {
        Inst = this;

        Tiles = _scriptableGrid.GenerateGrid();
        foreach (var tile in Tiles.Values) tile.CacheNeighbors();
        OnTileUnits = new Dictionary<Vector2, Unit>();
    }

    [SerializeField] ScriptableGrid _scriptableGrid;
    //[SerializeField] bool _drawConnections;

    public Dictionary<Vector2, HexNode> Tiles { get; private set; }
    public Dictionary<Vector2, Unit> OnTileUnits { get; private set; }
    public HexNode selectedNode;

    [SerializeField] GameObject areaPrefab;
    [SerializeField] Sprite areaSprite;
    [SerializeField] Color selectAreaColor;
    [SerializeField] Color moveAreaColor;
    [SerializeField] Color attackAreaColor;
    [SerializeField] Color buffAreaColor;
    [SerializeField] RuntimeAnimatorController hatch;

    public void AreaDisplay(AreaType areaType, bool canSelect, List<HexNode> tiles, Unit unit)
    {
        foreach (HexNode t in tiles)
        {
            GetArea(areaType, canSelect, t, tiles, unit);
        }
    }
    public void GetArea(AreaType areaType, bool canSelect, HexNode tile, List<HexNode> tiles, Unit unit)
    {
        var color = Color.white;
        switch (areaType)
        {
            case AreaType.Select:
                color = selectAreaColor;
                break;
            case AreaType.Move:
                color = moveAreaColor;
                break;
            case AreaType.Attack:
                color = attackAreaColor;
                break;
            case AreaType.Buff:
                color = buffAreaColor;
                break;
        }

        GameObject displayArea = null;
        var tileSprite = tile.transform.GetChild(0);
        for (int i = 0; i < tileSprite.childCount; i++)
        {
            if(!tileSprite.GetChild(i).gameObject.activeSelf)
            {
                displayArea = tileSprite.GetChild(i).gameObject;
                break;
            }
        }
        if(!displayArea)
        {
            displayArea = Instantiate(areaPrefab, tile.transform.GetChild(0).transform);
            displayArea.transform.localPosition = Vector2.zero;
        }
        displayArea.SetActive(true);
        displayArea.GetComponent<Animator>().enabled = !canSelect;
        var displaySpriteRenderer = displayArea.GetComponent<SpriteRenderer>();
        displaySpriteRenderer.color = canSelect ? new Color(color.r, color.g, color.b, 0.2f) : new Color(color.r, color.g, color.b, 0.5f);
        displaySpriteRenderer.sprite = areaSprite;

        foreach (HexDirection direction in HexDirectionExtension.Loop(HexDirection.EN))
        {
            var isContain = !tiles.Contains(GetTile(tile.coords + direction.Coords()));
            displayArea.transform.GetChild((int)direction).gameObject.SetActive(isContain);
            if (isContain)
                displayArea.transform.GetChild((int)direction).GetComponent<SpriteRenderer>().color = color;
        }
        displayArea.GetComponent<DisplayNode>().Get(areaType, canSelect, unit);
    }

    public void RevertTiles(Unit unit = null)
    {
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

    #region SetTileUnit
    public void SetTileUnit(HexCoords hexCoords, Unit unit)
    {
        GetTile(hexCoords).OnUnit(unit);
        OnTileUnits.Add(hexCoords.Pos, unit);
        SetWalkable();
    }
    public bool SetTileUnit(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
        if (prevCoords == nextCoords)
            return false;
        if (GetTile(nextCoords)?.CanWalk() != true)
            return false;

        GetTile(nextCoords).OnUnit(unit);
        OnTileUnits.Add(nextCoords.Pos, unit);
        GetTile(prevCoords).OnUnit(unit, true);
        OnTileUnits.Remove(prevCoords.Pos);
        SetWalkable();
        return true;
    }
    public void SetTileUnitRemove(Unit unit)
    {
        GetTile(unit).OnUnit(unit, true);
        OnTileUnits.Remove(unit.coords.Pos);
        SetWalkable();
    }
    #endregion    
    #region GetTile
    public HexNode GetTile(Unit unit) => Tiles.ContainsKey(unit.coords.Pos) ? Tiles.TryGetValue(unit.coords.Pos, out var tile) ? tile : null : null;
    public HexNode GetTile(HexCoords coords) => Tiles.ContainsKey(coords.Pos) ? Tiles.TryGetValue(coords.Pos, out var tile) ? tile : null : null;
    public HexNode GetTile(Vector2 pos) => Tiles.ContainsKey(pos) ? Tiles.TryGetValue(pos, out var tile) ? tile : null : null;
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
    public Unit GetUnit(HexNode hexNode) => OnTileUnits.ContainsKey(hexNode.coords.Pos) ? OnTileUnits[hexNode.coords.Pos] : null;
    public Unit GetUnit(HexCoords hexCoords) => OnTileUnits.ContainsKey(hexCoords.Pos) ? OnTileUnits[hexCoords.Pos] : null;
    public Unit GetUnit(Vector2 pos) => OnTileUnits.ContainsKey(pos) ? OnTileUnits[pos] : null;
    #endregion

    public void SetWalkable()
    {
        foreach (KeyValuePair<Vector2, HexNode> tile in Tiles)
            tile.Value.onUnit = OnTileUnits.ContainsKey(tile.Value.coords.Pos);
    }
    public HexNode GetRandomNode() => Tiles.Where(t => t.Value.CanWalk()).OrderBy(t => Random.value).First().Value;
    public void StatusNode()
    {
        foreach (var tile in Tiles.Where(t => t.Value.onUnit && t.Value.statuses.Count != 0))
        {
            print(tile.Value.statuses[0].data.name);
            StatusManager.Inst.AddUnitStatus(tile.Value.statuses, GetUnit(tile.Value));
        }
    }
    /*public void ShowEntire(List<HexNode> tiles = null)
    {
        foreach (var tile in Tiles)
            if(tiles.Contains(tile.Value) == false)
                tile.Value.OnDisplay(DisplayType.Outline);
    }*/

    /*void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        foreach (var t in Tiles)
        {
            if (t.Value.Connection == null) continue;
            Gizmos.DrawLine((Vector3)t.Key + new Vector3(0, 0, -1), (Vector3)t.Value.Connection.coords.Pos + new Vector3(0, 0, -1));
        }
    }*/
}