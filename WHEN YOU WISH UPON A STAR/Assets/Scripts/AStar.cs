using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AStarNode
{
    public int mH;      // Estimated cost to travel from this node.
    public int mG;      // Calculated cost traveled so far to this node from start.
    public int mF;      // Combined cost of H and G.
    public Tile tile;
    public AStarNode mParent;

    public AStarNode()
    {
        tile = new Tile();
        mH = 0;
        mG = 0;
        mF = 0;
        mParent = null;
    }

    public void ResetAStarNode()
    {
        mH = 0;
        mG = 0;
        mF = 0;
        mParent = null;
    }
}

public class AStarPath
{
    public List<AStarNode> m_open;
    public List<AStarNode> m_closed;

    public AStarPath()
    {
        m_open = new List<AStarNode>();
        m_closed = new List<AStarNode>();
    }
}

public class Pathfinding
{
    bool DiagonalMovementAllowed;

    private const int movementCostStraight = 10;
    private const int movementCostDiagonal = 14;

    GameManager gameManager;

    public void Init(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public List<AStarNode> FindPath(Vector2 p_startPos, Vector2 p_endPos, AStarPath p_path)
    {
        // Reset Tile Colors
        {
            foreach (AStarNode a in p_path.m_closed)
            {
                gameManager.SetTileColor(a.tile.position, Color.white);
            }

            foreach (AStarNode a in p_path.m_open)
            {
                gameManager.SetTileColor(a.tile.position, Color.white);
            }
        }

        p_path.m_closed.Clear();
        p_path.m_open.Clear();

        List<AStarNode> path            = new List<AStarNode>();
        AStarNode startNode             = gameManager.GetAStarNode(p_startPos.x, p_startPos.y);
        AStarNode endNode               = gameManager.GetAStarNode(p_endPos.x, p_endPos.y);
        startNode.tile                  = gameManager.grid.GetTile(p_startPos);
        endNode.tile                    = gameManager.grid.GetTile(p_endPos);

        AStarNode current               = startNode;

        current.mParent                 = null;
        current.mG                      = 0;
        current.mH                      = (int)(Mathf.Abs(current.tile.position.x - p_endPos.x) + Mathf.Abs(current.tile.position.y - p_endPos.y)) * movementCostStraight;
        current.mF                      = current.mG + current.mH;

        p_path.m_open.Add(current);

        List<AStarNode> adjacentNodes   = new List<AStarNode>();

        while (p_path.m_open.Count != 0)
        {
            int min = int.MaxValue;

            for (int i = 0; i < p_path.m_open.Count; i++)
            {
                if (p_path.m_open[i].mF < min)
                {
                    min = p_path.m_open[i].mF;
                    current = p_path.m_open[i];
                }
            }

            p_path.m_open.Remove(current);
            gameManager.SetTileColor(current.tile.position, Color.yellow);

            if (current.tile.position == p_endPos)
            {
                gameManager.SetTileColor(current.tile.position, Color.magenta);
                p_path.m_closed.Add(current);

                break;
            }

            adjacentNodes.Clear();
            adjacentNodes.AddRange(GetAdjacentNodes(current));

            foreach (AStarNode node in adjacentNodes)
            {
                if (gameManager.CheckIfNodeWalkable(node.tile.position))
                {
                    if (!p_path.m_closed.Contains(node))
                    {
                        if (!p_path.m_open.Contains(node))
                        {
                            if (node.mParent == null)
                            {
                                node.mParent = current;
                            }

                            if (node.tile.position == p_endPos)
                            {
                                node.mG = movementCostStraight + node.mParent.mG;
                                node.mH = (int)(Mathf.Abs(node.tile.position.x - p_endPos.x) + Mathf.Abs(node.tile.position.y - p_endPos.y)) * movementCostStraight;
                                node.mF = node.mH + node.mG;
                            }              // If node is in the goal position.
                            else if (node.tile.position != p_startPos)
                            {
                                node.mG = movementCostStraight + node.mParent.mG;
                                node.mH = (int)(Mathf.Abs(node.tile.position.x - p_endPos.x) + Mathf.Abs(node.tile.position.y - p_endPos.y)) * movementCostStraight;
                                node.mF = node.mH + node.mG;
                            }       // If node is in along the path.
                            else
                            {
                                node.mG = 0;
                                node.mH = (int)(Mathf.Abs(node.tile.position.x - p_endPos.x) + Mathf.Abs(node.tile.position.y - p_endPos.y)) * movementCostStraight;
                                node.mF = node.mH + node.mG;
                            }

                            p_path.m_open.Add(node);
                        }
                    }
                }
            }

            p_path.m_closed.Add(current);
            gameManager.SetTileColor(current.tile.position, Color.red);
        }

        if (p_path.m_closed.Count == 0)
        {
            return null;
        }

        // Calculate path back towards the StarChaser
        if (current.tile.position == p_endPos)
        {
            AStarNode temp = new AStarNode();
            temp = p_path.m_closed[p_path.m_closed.IndexOf(current)];
            if (temp == null)
                return null;

            do
            {
                gameManager.SetTileColor(temp.tile.position, Color.yellow);
                path.Add(temp);

                temp = temp.mParent;
            } while (temp != null && temp.tile.position != p_startPos && path.Count < gameManager.grid.GetGridSize());

            path.Reverse();

            gameManager.ResetAStarNodes();

            return path;
        }
        else
        {
            return null;
        }
    }
    
    private List<AStarNode> GetAdjacentNodes(AStarNode node)
    {
        List<AStarNode> tempList = new List<AStarNode>();

        int x = (int)node.tile.position.x;
        int y = (int)node.tile.position.y; ;

        // North
        if (gameManager.grid.GetTile(x, y + 1) != null && y + 1 < gameManager.grid.GetGridHeight())
        {
            tempList.Add(gameManager.GetAStarNode(x, y + 1));
            tempList[tempList.Count - 1].tile = gameManager.grid.GetTile(x, y + 1);
        }

        // East
        if (gameManager.grid.GetTile(x + 1, y) != null && x + 1 < gameManager.grid.GetGridWidth())
        {
            tempList.Add(gameManager.GetAStarNode(x + 1, y));
            tempList[tempList.Count - 1].tile = gameManager.grid.GetTile(x + 1, y);
        }

        // South
        if (gameManager.grid.GetTile(x, y - 1) != null && y - 1 >= 0)
        {
            tempList.Add(gameManager.GetAStarNode(x, y - 1));
            tempList[tempList.Count - 1].tile = gameManager.grid.GetTile(x, y - 1);
        }

        // West
        if (gameManager.grid.GetTile(x - 1, y) != null && x - 1 >= 0)
        {
            tempList.Add(gameManager.GetAStarNode(x - 1, y));
            tempList[tempList.Count - 1].tile = gameManager.grid.GetTile(x - 1, y);
        }

        return tempList;
    }

    public List<AStarNode> JPSFindPath(Vector2 p_startPos, Vector2 p_endPos, AStarPath p_path, Vector2 direction)
    {
        p_path.m_closed.Clear();
        p_path.m_open.Clear();

        return null;
    }

    public AStarNode JPSTravelOrthogonally(AStarNode p_parent, AStarPath p_path, Vector2 direction)
    {
        if (direction.x > 0)            // Travel east
        {
            direction.x++;
            if (direction.x < gameManager.grid.GetGridWidth() && gameManager.CheckIfNodeWalkable(direction))
                JPSTravelOrthogonally(p_parent, p_path, direction);
            else
                return null;
        }
        else if (direction.x < 0)       // Travel west
        {
            direction.x--;
            if (direction.x >= 0 && gameManager.CheckIfNodeWalkable(direction))
                JPSTravelOrthogonally(p_parent, p_path, direction);
            else
                return null;
        }
        else if (direction.y > 0)       // Travel north
        {
            direction.y++;
            if (direction.y < gameManager.grid.GetGridHeight() && gameManager.CheckIfNodeWalkable(direction))
                JPSTravelOrthogonally(p_parent, p_path, direction);
            else
                return null;
        }
        else if (direction.y < 0)       // Travel south
        {
            direction.y--;
            if (direction.y >= 0 && gameManager.CheckIfNodeWalkable(direction))
                JPSTravelOrthogonally(p_parent, p_path, direction);
            else
                return null;
        }

        return null;
    }

    public AStarNode JPSTravelDiagonally(AStarNode p_parent, AStarPath p_path, Vector2 direction)
    {
        return null;
    }
}
