using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexEngine;

public class Movement : MonoBehaviour
{
    public HexCoords hexCoords;
    void Start()
    {

        hexCoords = new HexCoords(1, 0);
        transform.position = hexCoords.Pos;
    }
}
