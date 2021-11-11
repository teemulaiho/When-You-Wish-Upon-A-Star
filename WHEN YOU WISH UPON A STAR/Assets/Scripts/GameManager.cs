using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Tile
{
    public Vector2 position;
}

public class Grid
{
    int width;
    int height;
    int gridSize;

    public List<Tile> tileList = new List<Tile>();
    Tile tile;

    public void Init(int p_width, int p_height)
    {
        width = p_width;
        height = p_height;
        gridSize = width * height;

        for (int i = 0; i < gridSize; i++)
        {
            tile = new Tile();
            tile.position.x = i % width;
            tile.position.y = i / width;

            tileList.Add(tile);
        }
    }

    public int GetGridSize()
    {
        return gridSize;
    }

    public int GetGridWidth()
    {
        return width;
    }

    public int GetGridHeight()
    {
        return height;
    }

    public Vector2 GetTilePosition(int index)
    {
        return tileList[index].position;
    }

    public Tile GetTile(int index)
    {
        return tileList[index];
    }

    public Tile GetTile(Vector2 postion)
    {
        int index = (int)postion.x + (int)postion.y * width;

        if (index >= 0 && index < gridSize)
            return tileList[index];
        else
            return null;
    }

    public Tile GetTile(int x, int y)
    {
        if (x > width - 1&& x < 0)
            return null;

        if (y > height - 1 && y < 0)
            return null;

        int index = x + y * width;

        if (index >= 0 && index < gridSize)
            return tileList[index];
        else
            return null;
    }
}

public class GameManager : MonoBehaviour
{
    public int                          starChaserAmount;
    public int                          tradingPostAmount;

    public TileBehaviour                tilePrefab;
    public StarChaserBehaviour          starChaserPrefab;
    public FallenStarBehaviour          fallenStarPrefab;
    public TradingPostBehaviour         tradingPostPrefab;
    public HomeBehaviour                homePrefab;

    public Grid grid                    = new Grid();

    List<TileBehaviour>                 tileList;
    List<StarChaserBehaviour>           starChaserList;
    List<FallenStarBehaviour>           fallenStarList;
    List<TradingPostBehaviour>          tradingPostList;
    List<HomeBehaviour>                 homeList;

    int                                 gridSize;
    Vector2                             target;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init()
    {
        target                          = new Vector2(9, 9);

        // Initialize grid with x & y.
        grid.Init(25, 25);

        gridSize                        = grid.GetGridSize();
        tileList                        = new List<TileBehaviour>();
        starChaserList                  = new List<StarChaserBehaviour>();
        fallenStarList                  = new List<FallenStarBehaviour>();
        tradingPostList                 = new List<TradingPostBehaviour>();
        homeList                        = new List<HomeBehaviour>();

        tradingPostAmount               = 1;
        starChaserAmount                = 1;

        GameObject gridParent = new GameObject("GRID");
        for (int i = 0; i < gridSize; i++)
        {
            if (tilePrefab != null)
            {
                TileBehaviour tile = Instantiate<TileBehaviour>(tilePrefab, grid.tileList[i].position/* - new Vector2(-0.5f, -0.5f)*/, Quaternion.identity);
                tile.transform.localScale *= 6;
                tile.Init(this);

                if ((int)Random.Range(0, 4) == 1)
                {
                    tile.isWalkable = false;
                }
                tile.name = "Tile: (" + grid.tileList[i].position.x + "," + grid.tileList[i].position.y + ")";
                tileList.Add(tile);
                tile.transform.SetParent(gridParent.transform);
            }
        }

        if (gridSize > 0)
        {
            // Initialize Trading Post
            {
                int tradingPostIndex = (int)Random.Range(0, gridSize);
                TradingPostBehaviour tradingPost = Instantiate<TradingPostBehaviour>(tradingPostPrefab, grid.tileList[tradingPostIndex].position, Quaternion.identity);
                tradingPost.Init(this);
                tradingPostList.Add(tradingPost);
                tileList[tradingPostIndex].isWalkable = true;
            }

            // Initialize Home
            {
                int homeIndex = (int)Random.Range(0, gridSize);
                HomeBehaviour home = Instantiate<HomeBehaviour>(homePrefab, grid.tileList[homeIndex].position, Quaternion.identity);
                home.Init(this);
                homeList.Add(home);
                tileList[homeIndex].isWalkable = true;
            }

            // Initialize Fallen Star
            {
                int fallenStarPos = (int)Random.Range(0, gridSize);
                FallenStarBehaviour fallenStar = Instantiate<FallenStarBehaviour>(fallenStarPrefab, grid.tileList[fallenStarPos].position, Quaternion.identity);
                fallenStarList.Add(fallenStar);
                tileList[fallenStarPos].fallenStar = fallenStar;
                tileList[fallenStarPos].containsFallenStar = true;
                tileList[fallenStarPos].isWalkable = true;
            }

            // Initialize Star Chaser
            {
                for (int i = 0; i < starChaserAmount; i++)
                {
                    int starChaserIndex = i * 2;

                    StarChaserBehaviour starChaser = Instantiate<StarChaserBehaviour>(starChaserPrefab, grid.tileList[starChaserIndex].position, Quaternion.identity);
                    starChaser.Init(this);
                    starChaserList.Add(starChaser);

                    tileList[starChaserIndex].isWalkable = true;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetGame();
        }
    }

    public Vector2 GetTarget()
    {
        if (fallenStarList.Count > 0)
            return fallenStarList[0].transform.position;

        else
            return new Vector2(-1, -1);
    }

    public TileBehaviour GetTile(int x, int y)
    {
        int index = x + y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
            return tileList[index];
        else return null;
    }

    public  TileBehaviour GetTile(Vector2 pos)
    {
        int x = (int)Mathf.Ceil(pos.x);
        int y = (int)Mathf.Ceil(pos.y);

        return GetTile(x, y);
    }

    public AStarNode GetAStarNode(int x, int y)
    {
        int index = x + y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
            return tileList[index].aStarNode;
        else return null;
    }

    public AStarNode GetAStarNode(float x, float y)
    {
        int index = (int)x + (int)y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
            return tileList[index].aStarNode;
        else return null;
    }

    public void SetTileColor(Vector2 position, Color color)
    {
        int index = (int)position.x + (int)position.y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
        {
            tileList[index].sr.color = color;
            tileList[index].originalColor = color;
        }            
    }

    public bool CheckIfNodeWalkable(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        int index = x + y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
            return tileList[index].isWalkable;
        else return false;
    }

    public bool CheckForFallenStar(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        int index = x + y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
            return tileList[index].containsFallenStar;
        else
            return false;
    }

    public FallenStarBehaviour GetFallenStar(Vector2 position)
    {
        int x = (int)position.x;
        int y = (int)position.y;

        int index = x + y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
            return tileList[index].fallenStar;
        else
            return null;
    }

    public void CreateNewFallenStar()
    {
        int fallenStarPos = (int)Random.Range(0, gridSize);
        FallenStarBehaviour fallenStar = Instantiate<FallenStarBehaviour>(fallenStarPrefab, grid.tileList[fallenStarPos].position, Quaternion.identity);
        fallenStarList.Add(fallenStar);
        tileList[fallenStarPos].fallenStar = fallenStar;
        tileList[fallenStarPos].containsFallenStar = true;
        tileList[fallenStarPos].isWalkable = true;
    }

    public Vector2 GetFallenStarPosition()
    {
        for (int i = 0; i < fallenStarList.Count; i++)
        {
            if (fallenStarList[i].starChaserOwner == null && fallenStarList[i].tradingPostOwner == null)
            {
                if (fallenStarList[i].dropped)
                {
                    return fallenStarList[i].GetTargetPos();
                }
                return fallenStarList[i].transform.position;
            }          
        }

        return new Vector2(-1, -1);
    }

    public void SetNewStarOwner(StarChaserBehaviour owner, FallenStarBehaviour star)
    {
        for (int i = 0; i < fallenStarList.Count; i++)
        {
            if (fallenStarList[i] == star)
                fallenStarList[i].SetNewOwner(owner);
        }
    }

    public void SetNewStarOwner(TradingPostBehaviour owner, FallenStarBehaviour star)
    {
        for (int i = 0; i < fallenStarList.Count; i++)
        {
            if (fallenStarList[i] == star)
                fallenStarList[i].SetNewOwner(owner);
        }
    }

    public Vector2 GetTradingPostPosition()
    {
        if (tradingPostList.Count > 0)
            return tradingPostList[0].transform.position;
        else
            return new Vector2(-1, -1);
    }

    public Vector2 GetHomePosition()
    {
        if (homeList.Count > 0)
        {
            return homeList[0].transform.position;
        }
        else
            return new Vector2(-1, -1);
    }

    public void HandOverFallenStar(FallenStarBehaviour star)
    {
        tradingPostList[0].SetFallenStar(star);
        CreateNewFallenStar();
    }

    public bool DropFallenStar(FallenStarBehaviour star, Vector2 position)
    {
        int index = (int)position.x + (int)position.y * grid.GetGridWidth();

        if (index >= 0 && index < gridSize)
        {
            tileList[index].fallenStar = star;
            tileList[index].containsFallenStar = true;
            star.Dropped(position);

            return true;
        }
        else
            return false;
    }
    
    public bool ResetAStarNodes()
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            tileList[i].aStarNode.mParent = null;
            tileList[i].aStarNode.mF = 0;
            tileList[i].aStarNode.mG = 0;
            tileList[i].aStarNode.mH = 0;
        }

        return true;
    }
    
    public void ResetAllTiles()
    {
        foreach (TileBehaviour t in tileList)
        {
            t.isWalkable = true;
            t.sr.color = Color.white;
        }
    }

    void ResetGame()
    {
        for (int i = 0; i < starChaserAmount; i++)
        {
            starChaserList[i].ResetStarChaser();
        }

        for (int i = 0; i < tileList.Count; i++)
        {
            tileList[i].isWalkable = true;
            tileList[i].sr.color = Color.white;
            tileList[i].aStarNode.ResetAStarNode();
        }
    }
}


