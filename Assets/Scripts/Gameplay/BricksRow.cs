﻿using UnityEngine;

public class BricksRow : MonoBehaviour
{
    public float m_FloorPosition = -4.25f;

    public Brick[] m_Bricks;
    public ScoreBall[] m_ScoreBalls;

    private void Awake()
    {
        /* fill bricks
        // generate rows of bricks on the scene
        m_Bricks = new List<Bricks>();
        for (int i = 0; i < m_SpawningRows; i++)
        {
            m_Bricks.Add(Instantiate(m_BricksRowPrefab, transform.parent, false));
            m_Bricks[m_Bricks.Count - 1].transform.localPosition = new Vector3(0, m_SpawningTopPosition, 0);
            m_Bricks[m_Bricks.Count - 1].gameObject.SetActive(false);
        } 
         */
        m_Bricks = GetComponentsInChildren<Brick>();
        m_ScoreBalls = GetComponentsInChildren<ScoreBall>();
    }

    private void OnEnable()
    {
        Debug.Log("BricksRow - OnEnable");
        if (transform.localPosition.y < m_FloorPosition)
            GoToTop();

        HideAll();
        GoToTop();

        MoveDown(BrickSpawner.Instance.m_SpawningDistance);

        // make only one score ball available for this row randomly
        m_ScoreBalls[Random.Range(0, m_ScoreBalls.Length)].gameObject.SetActive(true);

        // try to enable bricks randomly except at the score ball's position
        for (int i = 0; i < m_Bricks.Length; i++)
        {
            if(m_ScoreBalls[i].gameObject.activeInHierarchy)
                m_Bricks[i].gameObject.SetActive(false);
            else
                m_Bricks[i].gameObject.SetActive(Random.Range(0, 2) == 1 ? true : false);
        }

        // make at least one brick available if there was not any one before
        bool hasNoBrick = true;
        Debug.Log("OnEnable hasNoBrick " + hasNoBrick);
        for (int i = 0; i < m_Bricks.Length; i++)
            if (m_Bricks[i].gameObject.activeInHierarchy)
            {
                hasNoBrick = false;
                Debug.Log("OnEnable hasNoBrick " + i + hasNoBrick);
                break;
            }

        if (hasNoBrick)
            for (int i = 0; i < m_Bricks.Length; i++)
                if (!m_ScoreBalls[i].gameObject.activeInHierarchy)
                {
                    m_Bricks[i].gameObject.SetActive(true);
                    break;
                }
    }

    private void Update()
    {
        if(transform.localPosition.y <= m_FloorPosition)
        {
            if (HasActiveBricks())
            {
                LevelManager.Instance.m_LevelState = LevelManager.LevelState.GameOver;
                //AttackPlayer();
                if (HasActiveScoreBall())
                {
                    GoToTop();
                    gameObject.SetActive(false);
                }
            }  
            else if (HasActiveScoreBall())
            {
                GoToTop();
                gameObject.SetActive(false);
            }
            else
            {
                GoToTop();
                gameObject.SetActive(false);
            }
        }
    }

    private void HideAll()
    {
        Debug.Log("BricksRow - HideAll");
        Debug.Log("BricksRow - HideAll -> m_Bricks.Length " + m_Bricks.Length);
        for (int i = 0; i < m_Bricks.Length; i++)
        {
            m_Bricks[i].gameObject.SetActive(false);
            m_ScoreBalls[i].gameObject.SetActive(false);
        }
        
    }

    private void GoToTop()
    {
        Debug.Log("GoToTop");
        //HideAll();
        transform.localPosition = new Vector3(0, BrickSpawner.Instance.m_SpawningTopPosition, 0);
    }

    public void MoveDown(float howMuch)
    {
        for (int i = 0; i < m_Bricks.Length; i++)
            if (m_Bricks[i].gameObject.activeInHierarchy)
                m_Bricks[i].ChangeColor();

        iTween.MoveTo(gameObject, new Vector3(transform.position.x, transform.position.y - howMuch, transform.position.z), 0.25f);
    }

    public void CheckBricksActivation()
    {
        int deactiveObjects = 0;

        for (int i = 0; i < m_Bricks.Length; i++)
            if (!m_Bricks[i].gameObject.activeInHierarchy && !m_ScoreBalls[i].gameObject.activeInHierarchy)
                deactiveObjects++;

        if (deactiveObjects == m_Bricks.Length)
        {
            gameObject.SetActive(false);
            GoToTop();
        }
    }

    //TEST METHOD ALEX BOCHKAREV
    public void AttackPlayer ()
    {
        for (int i = 0; i < m_Bricks.Length; i++)
        {
            if (m_Bricks[i].gameObject.activeInHierarchy)
            {
                m_Bricks[i].Attack();
            }
        }
    }

    public bool HasActiveBricks()
    {
        bool hasActiveBrick = false;

        for (int i = 0; i < m_Bricks.Length; i++)
        {
            if (m_Bricks[i].gameObject.activeInHierarchy)
            {
                hasActiveBrick = true;
                break;
            }
        }

        return hasActiveBrick;
    }

    public bool HasActiveScoreBall()
    {
        bool hasActiveScoreBall = false;

        for (int i = 0; i < m_ScoreBalls.Length; i++)
        {
            if (m_ScoreBalls[i].gameObject.activeInHierarchy)
            {
                m_ScoreBalls[i].PlayParticle();
                BallLauncher.Instance.IncreaseBallsAmountFromOutSide(1);

                hasActiveScoreBall = true;
                break;
            }
        }

        return hasActiveScoreBall;
    }
}