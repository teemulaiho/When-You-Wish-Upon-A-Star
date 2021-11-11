using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileState
{
    Empty,
    FallenStar,
    Tree
}
public class TileBehaviour : MonoBehaviour
{
    Tile                            tile;

    public CircleCollider2D         circleCollider;
    public SpriteRenderer           sr;
    GameManager                     gameManager;
    public AStarNode                aStarNode;
    public bool                     isWalkable;
    public bool                     containsFallenStar;
    public FallenStarBehaviour      fallenStar;

    public Color                    originalColor;

    public void Init(GameManager gameManager)
    {
        this.gameManager            = gameManager;
        sr                          = GetComponent<SpriteRenderer>();
        tile                        = this.gameManager.grid.GetTile(transform.position);
        aStarNode                   = new AStarNode();
        isWalkable                  = true;
        circleCollider              = GetComponent<CircleCollider2D>();
        originalColor               = sr.color;
        fallenStar                  = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWalkable)
            sr.color = Color.black;
        else
            sr.color = originalColor;
    }

    private void OnMouseDown()
    {
        if (isWalkable == true)
        {
            isWalkable = false;
        }
        else
        {
            isWalkable = true;
        }
    }
}
