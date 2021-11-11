using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarChaserBehaviour : MonoBehaviour
{
    public enum AgentState
    {
        None,
        Collecting,
        Selling,
        ReturningHome,
        Resting
    }

    [SerializeField] AgentState agentState;
    AgentState AgentStateChange{ 
        get { return agentState; } 
        set
        {
            if (agentState == value) return;
            agentState = value;
            if (OnAgentStateChange != null)
                OnAgentStateChange(agentState);
        }
    }

    public delegate void OnAgentStateChangeDelegate(AgentState newState);
    public event OnAgentStateChangeDelegate OnAgentStateChange;

    GameManager                         m_gameManager;
    Pathfinding                         m_pathFinding;
    AStarPath                           m_path;
    public FallenStarBehaviour          m_fallenStar;

    public Vector2                      endTarget;
    Vector2                             currentTarget;
    List<AStarNode>                     path;

    public int                          energy;
    public float                        speed;
    public bool                         hasFallenStar;

    bool                                returningStar;
    public bool                         unitSelected;
    public bool                         RTSMovement;

    public float                        patienceLimit;
    public float                        patienceDT;
    public float                        originalScale;

    public void Init(GameManager gameManager)
    {
        this.m_gameManager              = gameManager;
        m_pathFinding                   = new Pathfinding();
        m_pathFinding.Init(m_gameManager);

        m_path                          = new AStarPath();

        endTarget                       = gameManager.GetTarget();
        if (endTarget.x < 0)
        {
            endTarget = transform.position;
        }

        path                            = new List<AStarNode>();

        currentTarget                   = transform.position;
        speed                           = 5f;
        energy                          = 100;
        hasFallenStar                   = false;
        m_fallenStar                    = null;
        unitSelected                    = false;
        RTSMovement                     = false;

        patienceLimit                   = 10f;
        patienceDT                      = 0f;
        originalScale                   = transform.localScale.x;
    }

    // Start is called before the first frame update
    void Start()
    {
        OnAgentStateChange += CalculateNewPath;
    }

    // Update is called once per frame
    void Update()
    {
        // Change State With Keys
        GetInput();

        Sense();
        Decide();
        Act();

        // Input Controls
        if (RTSMovement)
        {
            if (unitSelected)
            {
                hasFallenStar = CheckForStar();
                if (hasFallenStar)
                {
                    m_fallenStar = GetFallenStar();
                    m_gameManager.SetNewStarOwner(this, m_fallenStar);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.x = Mathf.RoundToInt(mouseWorldPos.x);
                    mouseWorldPos.y = Mathf.RoundToInt(mouseWorldPos.y);
                    mouseWorldPos.z = Mathf.RoundToInt(mouseWorldPos.z);

                    endTarget = mouseWorldPos;

                    if(m_gameManager.CheckIfNodeWalkable(endTarget))
                        path = m_pathFinding.FindPath(transform.position, endTarget, m_path);
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (m_path.m_open != null && m_path.m_open.Count > 0)
                    m_path.m_open.Clear();

                if (m_path.m_closed != null && m_path.m_closed.Count > 0)
                    m_path.m_closed.Clear();

                if (path != null && path.Count > 0)
                    path.Clear();

                endTarget = m_gameManager.GetTarget();
                if (endTarget.x < 0)
                    endTarget = transform.position;

                path = m_pathFinding.FindPath(transform.position, endTarget, m_path);

                if (path != null && path.Count > 0)
                {
                    m_path.m_open.Clear();
                    m_path.m_closed.Clear();
                }
                
                if (path != null && path.Count > 0)
                    currentTarget = path[0].tile.position;
            }
        }
    }

    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AgentStateChange = AgentState.Collecting;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AgentStateChange = AgentState.ReturningHome;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AgentStateChange = AgentState.Selling;
        }
    }

    void Sense()
    {
        if (!RTSMovement && energy <= 0 &&
            endTarget != m_gameManager.GetHomePosition())
        {
            AgentStateChange = AgentState.ReturningHome;
        }
        else if (endTarget == m_gameManager.GetHomePosition() && (Vector2)transform.position == endTarget)
        {
            AgentStateChange = AgentState.Resting;

            if (energy > 0)
            {
                AgentStateChange = AgentState.Collecting;
            }

        }
        else if (energy > 0 && endTarget == m_gameManager.GetFallenStarPosition())
        {
            AgentStateChange = AgentState.Collecting;
        }

        // Recalculate path if the current path is blocked.
        if (path != null && path.Count > 0)
        {
            foreach (AStarNode a in path)
            {
                TileBehaviour tile = m_gameManager.GetTile(a.tile.position);

                if (!tile.isWalkable)
                {
                    CalculateNewPath(agentState);
                }
            }
        }

        // Recalculate path until a path has been found
        if (path == null || path.Count == 0)
        {
            patienceDT += Time.deltaTime;
            if (patienceDT > 0)
                transform.localScale = (patienceDT + 1) * originalScale * Vector3.one;
            CalculateNewPath(agentState);
        }

        if (patienceDT > patienceLimit)
        {
            patienceDT = 0;
            m_gameManager.ResetAllTiles();
            transform.localScale = originalScale * Vector3.one;
            CalculateNewPath(agentState);
        }
    }

    void Decide()
    {
        if (agentState == AgentState.Collecting)
        {
            if ((Vector2)transform.position == endTarget)
            {
                hasFallenStar   = CheckForStar();
                m_fallenStar    = GetFallenStar();

                if (hasFallenStar)
                {
                    m_gameManager.SetNewStarOwner(this, m_fallenStar);

                    AgentStateChange = AgentState.Selling;
                }
            }
        }
        else if (agentState == AgentState.Selling)
        {
            if ((Vector2)transform.position == endTarget)
            {
                m_gameManager.HandOverFallenStar(m_fallenStar);
                hasFallenStar = false;
                m_fallenStar = null;

                AgentStateChange = AgentState.Collecting;
            }
        }
    }

    void Act()
    {
        if (agentState == AgentState.Resting)
        {
            energy = (m_gameManager.grid.GetGridSize() / m_gameManager.grid.GetGridWidth()) + m_gameManager.grid.GetGridHeight();
        }
        else if (path != null && path.Count != 0)
            MoveTowardsTarget(currentTarget);
    }

    void MoveTowardsTarget(Vector2 p_target)
    {
        if ((Vector2)transform.position != currentTarget)
            transform.position = Vector2.MoveTowards(transform.position, p_target /*+ new Vector2(0.5f, 0.5f)*/, speed * Time.deltaTime);

        if ((Vector2)transform.position == currentTarget && 
            path != null && 
            path.Count > 0)
        {
            energy--;
            SetNewTarget();
        }
    }

    void SetNewTarget()
    {
        if (path != null)
        {
            if (path.Count > 0)
                path.RemoveAt(0);

            if (path.Count > 0)
                currentTarget = path[0].tile.position;
        }
    }

    bool CheckForStar()
    {
        return m_gameManager.CheckForFallenStar(transform.position);
    }

    FallenStarBehaviour GetFallenStar()
    {
        return m_gameManager.GetFallenStar(transform.position);
    }

    void CalculateNewPath(AgentState newState)
    {
        if (newState == AgentState.ReturningHome)
        {
            if (hasFallenStar)
            {
                m_gameManager.DropFallenStar(m_fallenStar, transform.position);

                hasFallenStar = false;
                m_fallenStar.starChaserOwner = null;
                m_fallenStar = null;
            }

            endTarget = m_gameManager.GetHomePosition();
        }
        else if (newState == AgentState.Collecting)
        {
            endTarget = m_gameManager.GetFallenStarPosition();
        }
        else if (newState == AgentState.Selling)
        {
            endTarget = m_gameManager.GetTradingPostPosition();
        }

        path = m_pathFinding.FindPath(transform.position, endTarget, m_path);
    }

    public void ResetStarChaser()
    {
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        currentTarget = new Vector2(x,y);

        m_path.m_open.Clear();
        m_path.m_closed.Clear();

        if (path != null)
            path.Clear();
    }

    private void OnMouseDown()
    {
        unitSelected = true;
    }
}
