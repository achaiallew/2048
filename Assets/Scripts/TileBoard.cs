using UnityEngine;
using System.Collections.Generic;

public class TileBoard : MonoBehaviour
{
    public Tile tilePrefab;
    public TileState[] tileStates;
    public int startNum;

    private TileGrid grid;
    private List<Tile> tiles;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>();
    }

    private void Start()
    {
        SpawnTile();
        SpawnTile();
    }

    private void SpawnTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], startNum);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

}
