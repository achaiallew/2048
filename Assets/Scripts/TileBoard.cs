using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class TileBoard : MonoBehaviour
{
    public Tile tilePrefab;
    public TileState[] tileStates;
    public int startNum;

    private TileGrid grid;
    public GameManager gameManager;
    private List<Tile> tiles;

    public float swipeThreshold = 0.15f;
    private bool swipeOnce;

    private bool waiting;

    
    [Header("Input References")]
    public InputActionReference moveUp;
    public InputActionReference moveDown;
    public InputActionReference moveRight;
    public InputActionReference moveLeft;

    [System.Serializable]
    public struct TileSnapshot
    {
        public Tile tile;
        public int index;
        public int number;
    }

    [System.Serializable]
    public class BoardSnapshot
    {
        public List<TileSnapshot> tiles = new();
        public int score;
    }

    private Stack<BoardSnapshot> undoStack = new();

    public bool winState = false;
    private int oneWin = 0;


    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>();
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }
        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }
        tiles.Clear();
    }

    public void SpawnTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetState(tileStates[0], startNum);
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void OnEnable()
    {
        moveUp.action.Enable();
        moveDown.action.Enable();
        moveRight.action.Enable();       
        moveLeft.action.Enable();

    
        moveUp.action.performed += MoveUp;
        moveDown.action.performed += MoveDown;
        moveRight.action.performed += MoveRight;
        moveLeft.action.performed += MoveLeft;
    }

    private void OnDisable()
    {
        moveUp.action.performed -= MoveUp;
        moveDown.action.performed -= MoveDown;
        moveRight.action.performed -= MoveRight;
        moveLeft.action.performed -= MoveLeft;

        moveUp.action.Disable();
        moveDown.action.Disable();
        moveRight.action.Disable();
        moveLeft.action.Disable();
    }

    private void MoveUp(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        if (context.control.device is Mouse)
        {
            if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
                return;

            if (input >= swipeThreshold && !swipeOnce)
            {
                swipeOnce = true;
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }
            swipeOnce = false;
            return;
        }

        if (Mathf.Approximately(input, 1f))
        {
            MoveTiles(Vector2Int.up, 0, 1, 1, 1);
        }

        
    }

    private void MoveDown(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        if (context.control.device is Mouse)
        {
            if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
                return;

            if (input >= swipeThreshold && !swipeOnce)
            {
                swipeOnce = true;
                MoveTiles(Vector2Int.down, 0, 1, grid.height-2, -1);
            }
            swipeOnce = false;  

            return;
        }

        if (Mathf.Approximately(input, 1f))
        {
            MoveTiles(Vector2Int.down, 0, 1, grid.height-2, -1);
        }

        
    }

    private void MoveRight(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        if (context.control.device is Mouse)
        {
            if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
                return;

            if (input >= swipeThreshold && !swipeOnce)
            {
                swipeOnce = true;
                MoveTiles(Vector2Int.right, grid.width-2, -1, 0, 1);
            }
            swipeOnce = false;

            return;
        }

        if (Mathf.Approximately(input, 1f))
        {
            MoveTiles(Vector2Int.right, grid.width-2, -1, 0, 1);
        }

        
    }

    private void MoveLeft(InputAction.CallbackContext context)
    {
        float input = context.ReadValue<float>();

        if (context.control.device is Mouse)
        {
            if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
                return;

            if (input >= swipeThreshold && !swipeOnce)
            {
                swipeOnce = true;
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }           
            swipeOnce = false;
            
        }

        if (Mathf.Approximately(input, 1f))
        {
            MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        }

        
    }

    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {        
        if (waiting)
        {
            return;
        }

        SaveState();

        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }

            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }
        else
        {
            undoStack.Pop(); // discard useless snapshot
        }
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.occupied)
            {
                // Merging
                if (CanMerge(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile);
                    return true;
                }

                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);

        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }
        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.number == b.number;
    }
    
    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);
        int number = b.number * 2;

        b.SetState(tileStates[index], number);

        // Increase the Score 
        gameManager.TrackScore(number);

        if (oneWin == 0)
        {
            if (number == 2048)
            {
                winState = true;
                oneWin ++;
            }
        }
        

    }

    private int IndexOf(TileState state)
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (state == tileStates[i])
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        waiting = true;
        yield return new WaitForSeconds(0.1f);
        waiting = false;

        // Create New Tile
        if (tiles.Count != grid.size)
        {
            SpawnTile();
        }
        //Check for GameOver
        if (CheckForGameOver())
        {
            gameManager.GameOver();
        }
        
    }

    private bool CheckForGameOver()
    {
        if (tiles.Count != grid.size)
        {
            return false;
        }

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            
            if (up != null && CanMerge(tile, up.tile))
            {
                return false;
            } else if (down != null && CanMerge(tile, down.tile))
            {
                return false;
            } else if (right != null && CanMerge(tile, right.tile))
            {
                return false;
            } else if (left != null && CanMerge(tile, left.tile))
            {
                return false;
            }
        }

        return true;
    }

    private void SaveState()
    {
        BoardSnapshot snapshot = new BoardSnapshot
        {
            tiles = new List<TileSnapshot>(),
            score = gameManager.GetScore()
        };

        foreach (Tile tile in tiles)
        {
            int index = System.Array.IndexOf(grid.cells, tile.cell);

            snapshot.tiles.Add(new TileSnapshot
            {
                index = index,
                number = tile.number
            });
        }

        undoStack.Push(snapshot);
    }


    private int IndexOfNumber(int number)
    {
        int index = 0;
        int value = startNum;

        while (value < number && index < tileStates.Length - 1)
        {
            value *= 2;
            index++;
        }

        return index;
    }


    // Undo
    public void Undo()
    {
        if (waiting || undoStack.Count == 0)
            return;

        BoardSnapshot snapshot = undoStack.Pop();

        ClearBoard();

        foreach (var t in snapshot.tiles)
        {
            TileCell cell = grid.cells[t.index];

            Tile tile = Instantiate(tilePrefab, grid.transform);
            tile.SetState(tileStates[IndexOfNumber(t.number)], t.number);
            tile.Spawn(cell);

            tiles.Add(tile);
        }

        gameManager.SetScoreFromUndo(snapshot.score);
    }


}
