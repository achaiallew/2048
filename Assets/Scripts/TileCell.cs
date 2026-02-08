using UnityEngine;

public class TileCell : MonoBehaviour
{
    public Vector2Int coords {get; set;}
    public Tile tile;
    public bool empty => tile == null;
    public bool occupied => tile != null;

}
