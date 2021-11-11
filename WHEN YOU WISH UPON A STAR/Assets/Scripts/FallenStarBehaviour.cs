using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallenStarBehaviour : MonoBehaviour
{
    GameManager                     gameManager;
    public StarChaserBehaviour      starChaserOwner;
    public TradingPostBehaviour     tradingPostOwner;
    public bool                     dropped;
    public Vector2                  target;

    public void Init(GameManager p_gameManager)
    {
        gameManager                 = p_gameManager;
    }

    // Update is called once per frame
    void Update()
    {
        if (starChaserOwner != null)
        {
            if (!dropped)
                MoveTowardsTarget(starChaserOwner.transform.position);                
        }

        if (dropped)
            MoveTowardsTarget(target);
    }

    void MoveTowardsTarget(Vector2 p_target)
    {
        if (!dropped)
        {
            if (Vector3.Distance(transform.position, starChaserOwner.transform.position) > 0.5)
                transform.position = Vector2.MoveTowards(transform.position, p_target /*+ new Vector2(0.5f, 0.5f)*/, starChaserOwner.speed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, p_target /*+ new Vector2(0.5f, 0.5f)*/, 2f * Time.deltaTime);
            if ((Vector2)transform.position == p_target)
            { 
                dropped = false;
            }
        }
    }

    public void SetNewOwner(StarChaserBehaviour p_owner)
    {
        if (tradingPostOwner != null)
            tradingPostOwner = null;

        starChaserOwner = p_owner;
    }

    public void SetNewOwner(TradingPostBehaviour p_owner)
    {
        if (starChaserOwner != null)
            starChaserOwner = null;
        
        tradingPostOwner = p_owner;
    }

    public void Dropped(Vector2 pos)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;

        target = new Vector2(x, y);
        dropped = true;
    }

    public Vector2 GetTargetPos()
    {
        return target;
    }
}
