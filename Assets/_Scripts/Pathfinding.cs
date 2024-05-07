using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinding
{
    private static readonly Color PathColor = new Color(0.65f, 0.35f, 0.35f);
    private static readonly Color OpenColor = new Color(.4f, .6f, .4f);
    private static readonly Color ClosedColor = new Color(0.35f, 0.4f, 0.5f);

    public static List<HexNode> FindPath(HexNode startNode, HexNode targetNode, bool onTarget = true)
    {
        var toSearch = new List<HexNode>() { startNode };
        var processed = new List<HexNode>();

        while (toSearch.Any())
        {
            var current = toSearch[0];
            foreach (var t in toSearch)
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;


            processed.Add(current);
            toSearch.Remove(current);

            //current.SetSelectOutline(SelectOutline.MoveSelect);

            if (current == targetNode)
            {
                var currentPathTile = targetNode;
                var path = new List<HexNode>();
                var count = 100;
                while (currentPathTile != startNode)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                    count--;
                    if (count < 0) throw new Exception();
                }
                if(!onTarget) 
                    path.Remove(targetNode);
                path.Reverse();
                return path;
            }

            foreach (var neighbor in current.Neighbors.Where(t => (t.CanWalk() || t == startNode || t == targetNode) && !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbor);

                var costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch)
                    {
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Add(neighbor);
                        //neighbor.SetSelectOutline(SelectOutline.MoveSelect);
                    }
                }
            }
        }
        return null;
    }

    public static int FindPathDistance(HexNode startNode, HexNode targetNode)
    {
        if (startNode == targetNode)
            return 0;

        var toSearch = new List<HexNode>() { startNode };
        var processed = new List<HexNode>();

        while (toSearch.Any())
        {
            var current = toSearch[0];
            foreach (var t in toSearch)
                if (t.F < current.F || t.F == current.F && t.H < current.H) current = t;

            processed.Add(current);
            toSearch.Remove(current);

            if (current == targetNode)
            {
                var currentPathTile = targetNode;
                var path = new List<HexNode>();
                var count = 100;
                while (currentPathTile != startNode)
                {
                    path.Add(currentPathTile);
                    currentPathTile = currentPathTile.Connection;
                    count--;
                    if (count < 0) throw new Exception();
                }
                return path.Count;
            }

            foreach (var neighbor in current.Neighbors.Where(t => (t.CanWalk() || t == startNode || t == targetNode) && !processed.Contains(t)))
            {
                var inSearch = toSearch.Contains(neighbor);

                var costToNeighbor = current.G + current.GetDistance(neighbor);

                if (!inSearch || costToNeighbor < neighbor.G)
                {
                    neighbor.SetG(costToNeighbor);
                    neighbor.SetConnection(current);

                    if (!inSearch)
                    {
                        neighbor.SetH(neighbor.GetDistance(targetNode));
                        toSearch.Add(neighbor);
                    }
                }
            }
        }
        return 10000;
    }
}