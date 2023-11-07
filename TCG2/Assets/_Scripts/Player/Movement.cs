using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public HexCoords hexCoords;
    void Start()
    {
        hexCoords = new HexCoords(1, 0);
        transform.position = hexCoords.Pos;
    }
}
