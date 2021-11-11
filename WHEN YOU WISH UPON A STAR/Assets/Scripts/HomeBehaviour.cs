using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeBehaviour : MonoBehaviour
{
    GameManager                 gameManager;

    public void Init(GameManager p_gameManager)
    {
        gameManager = p_gameManager;
    }
}
