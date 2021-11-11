using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradingPostBehaviour : MonoBehaviour
{
    GameManager                     gameManager;
    FallenStarBehaviour             fallenStar;
    int                             fallenStarCount;

    public void Init(GameManager p_gameManager)
    {
        gameManager = p_gameManager;
        fallenStar = null;
        fallenStarCount = 0;
    }

    public bool SetFallenStar(FallenStarBehaviour star)
    {
        gameManager.SetNewStarOwner(this, star);
        fallenStar = star;
        fallenStarCount++;

        return false;
    }
}
