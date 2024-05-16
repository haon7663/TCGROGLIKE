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

    [SerializeField] private GameObject areaPrefab;
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
                case AreaType.Arrange:
                    color = arrangeAreaColor;
                    break;
            }

            GameObject displayArea = null;
            var tileSprite = t.transform.GetChild(0);
            for (int i = 0; i < tileSprite.childCount; i++)
            {
                if (!tileSprite.GetChild(i).gameObject.activeSelf)
                {
                    displayArea = tileSprite.GetChild(i).gameObject;
                    break;
                }
            }
            if (!displayArea)
            {
                displayArea = Instantiate(areaPrefab, t.transform.GetChild(0).transform);
                displayArea.transform.localPosition = Vector2.zero;
            }
            
            displayArea.SetActive(true);
            displayArea.GetComponent<Animator>().enabled = !canSelect;
            
            var displaySpriteRenderer = displayArea.GetComponent<SpriteRenderer>();
            displaySpriteRenderer.color = canSelect ? new Color(color.r, color.g, color.b, 0.2f) : new Color(color.r, color.g, color.b, 0.5f);
            displaySpriteRenderer.sprite = areaSprite;

            foreach (var direction in HexDirectionExtension.Loop(HexDirection.EN))
            {
                var isContain = !tiles.Contains(GetTile(t.Coords + direction.Coords()));
                displayArea.transform.GetChild((int)direction).gameObject.SetActive(isContain);
                if (isContain)
                    displayArea.transform.GetChild((int)direction).GetComponent<SpriteRenderer>().color = color;
            }
            displayArea.GetComponent<RangeDisplayer>().Get(areaType, canSelect, unit);
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

    #region SetTileUnit
    public void SetTileUnit(HexCoords hexCoords, Unit unit)
    {
        GetTile(hexCoords).PutUnit(unit);
    }
    public bool SetTileUnit(HexCoords prevCoords, HexCoords nextCoords, Unit unit)
    {
        if (prevCoords == nextCoords)
            return false;
        if (GetTile(nextCoords)?.CanWalk() != true)
            return false;

        GetTile(nextCoords).PutUnit(unit);
        GetTile(prevCoords).RemoveUnit(unit);
        return true;
    }
    public void SetTileUnitRemove(Unit unit)
    {
        GetTile(unit).RemoveUnit(unit);
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
    public Unit GetUnit(HexNode hexNode) => hexNode.OnUnit ? hexNode.Unit : null;
    public Unit GetUnit(HexCoords hexCoords)
    {
        var hexNode = GetTile(hexCoords);
        return hexNode.OnUnit ? hexNode.Unit : null;
    }
    public Unit GetUnit(Vector2 pos)
    {
        var hexNode = GetTile(pos);
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