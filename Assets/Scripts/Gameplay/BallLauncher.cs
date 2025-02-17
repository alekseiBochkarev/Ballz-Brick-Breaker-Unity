﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BallLauncher : MonoBehaviour
{
    public static BallLauncher Instance;

    private Vector3 m_StartPosition;
    private Vector3 m_EndPosition;
    private Vector3 m_WorldPosition;

    private Vector3 m_Direction;

    private LineRenderer m_LineRenderer;
    private EdgeCollider2D edgeCollider2D;

    private Vector3 m_DefaultStartPosition;

    public SpriteRenderer m_BallSprite;
    public bool colliderTriggered = false;

    public bool m_CanPlay = true;
    [SerializeField] private GameObject m_DeactivatableChildren;
    [Header("Bricks")]
    //public Brick[] bricks;
    Collider2D[] colliders;

    [Header("Linerenderer Colors")]
    public Color m_CorrectLineColor;    // it will be displayed for correct angles
    public Color m_WrongLineColor;      // it will be displayed for wrong angles

    [Header("Balls")]
    public int m_BallsAmount;
    public int m_TempAmount = 0;  // for score balls
    public Text m_BallsText;
    [SerializeField] private int m_StartingBallsPoolAmount = 10;
    [SerializeField] private Ball m_BallPrefab;
    [SerializeField] private List<Ball> m_Balls;

    [Header("UI Elements")]
    [SerializeField] private GameObject m_ReturnBallsButton;
    public GameObject topBorder;
    public GameObject leftBorder;
    public GameObject rightBorder;
    public GameObject bottomBorder;
    public GameObject ballStartPosition;

    private void Awake()
    {
        Instance = this;
        m_CanPlay = true;
        m_LineRenderer = GetComponent<LineRenderer>();
        edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
        edgeCollider2D.isTrigger = false;

        m_DefaultStartPosition = transform.position;

        m_BallsAmount = PlayerPrefs.GetInt("balls", 1);
    }

    private void Start()
    {
        m_Balls = new List<Ball>(m_StartingBallsPoolAmount);
        m_BallsText.text = "x" + m_BallsAmount.ToString();
        m_ReturnBallsButton.SetActive(false);
        SpawNewBall(m_StartingBallsPoolAmount);
    }

    private void Update()
    {
        if (LevelManager.Instance.m_LevelState == LevelManager.LevelState.GameOver)
            return;

        if (!m_CanPlay)
            return;

        if(Time.timeScale != 0 && LevelManager.Instance.m_LevelState != LevelManager.LevelState.GameOver)
            m_WorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + Vector3.back * -10;

        /*
        if (Input.GetMouseButtonDown(0))
          //  StartDrag(m_WorldPosition);
        else */
        if (Input.GetMouseButton(0)
            && m_WorldPosition.x >= leftBorder.transform.position.x
            && m_WorldPosition.x <= rightBorder.transform.position.x
            && m_WorldPosition.y <= topBorder.transform.position.y
            && m_WorldPosition.y >= bottomBorder.transform.position.y)
            ContinueDrag(m_WorldPosition);
        else if (Input.GetMouseButtonUp(0)
            && m_WorldPosition.x >= leftBorder.transform.position.x
            && m_WorldPosition.x <= rightBorder.transform.position.x
            && m_WorldPosition.y <= topBorder.transform.position.y
            && m_WorldPosition.y >= bottomBorder.transform.position.y)
            EndDrag();
    }

    private void StartDrag(Vector3 worldPosition)
    {
        m_StartPosition = worldPosition;
       // Debug.Log("startPosition " + m_StartPosition);
    }
    
    private void ContinueDrag(Vector3 worldPosition)
    {
        if (worldPosition.x >= leftBorder.transform.position.x
            && worldPosition.x <= rightBorder.transform.position.x
            && worldPosition.y <= topBorder.transform.position.y
            && worldPosition.y >= bottomBorder.transform.position.y)
        {
            //Debug.Log("topBorder.transform.position" + topBorder.transform.position);
            // Debug.Log("endPosition " + worldPosition);
            Vector3 tempEndposition;

            Vector3 topPosition = new Vector3(((topBorder.transform.position.y - ballStartPosition.transform.position.y) * (worldPosition.x - ballStartPosition.transform.position.x)) / (worldPosition.y - ballStartPosition.transform.position.y) + ballStartPosition.transform.position.x, topBorder.transform.position.y, worldPosition.z);
            Vector3 leftPositionPoint = new Vector3(leftBorder.transform.position.x, ((leftBorder.transform.position.x - ballStartPosition.transform.position.x) * (worldPosition.y - ballStartPosition.transform.position.y)) / (worldPosition.x - ballStartPosition.transform.position.x) + ballStartPosition.transform.position.y, worldPosition.z);
            Vector3 rightPositionPoint = new Vector3(rightBorder.transform.position.x, ((rightBorder.transform.position.x - ballStartPosition.transform.position.x) * (worldPosition.y - ballStartPosition.transform.position.y)) / (worldPosition.x - ballStartPosition.transform.position.x) + ballStartPosition.transform.position.y, worldPosition.z);
            // Debug.Log("topPosition " + topPosition);
            if (topPosition.x < leftBorder.transform.position.x)
            {
                tempEndposition = leftPositionPoint;
            }
            else if (topPosition.x > rightBorder.transform.position.x)
            {
                tempEndposition = rightPositionPoint;
            }
            else
            {
                tempEndposition = topPosition;
            }

            Vector3 tempDirection = tempEndposition - ballStartPosition.transform.position;
            tempDirection.Normalize();
            // getting the angle in radians. you can replace 1.35f with any number or without hardcode like this
            if (Mathf.Abs(Mathf.Atan2(tempDirection.x, tempDirection.y)) < 1.35f)
            {
                // Debug.Log("Color is correct");
                m_LineRenderer.startColor = m_CorrectLineColor;
                m_LineRenderer.endColor = m_CorrectLineColor;
            }
            else
            {
                // Debug.Log("Color is incorrect");
                m_LineRenderer.startColor = m_WrongLineColor;
                m_LineRenderer.endColor = m_WrongLineColor;
            }

            m_EndPosition = tempEndposition;
            m_LineRenderer.SetPosition(1, m_EndPosition - ballStartPosition.transform.position);
            ChangeCollider();
        }
        
    }

    private void EndDrag()
    {
        if (m_StartPosition == m_EndPosition)
            return;

       // m_Direction = m_EndPosition - m_StartPosition;
        m_Direction = m_EndPosition - ballStartPosition.transform.position;
        m_Direction.Normalize();

        m_LineRenderer.SetPosition(1, Vector3.zero);
        ChangeCollider();
        if (Mathf.Abs(Mathf.Atan2(m_Direction.x, m_Direction.y)) < 1.35f)   // hardcode for this time. fix it!
        {
            //set RigidbodyType for all bricks
            FindBricksAndSetRigidbodyType(RigidbodyType2D.Static);
            if (m_Balls.Count < m_BallsAmount)
                SpawNewBall(m_BallsAmount - m_Balls.Count);

            m_CanPlay = false;
            StartCoroutine(StartShootingBalls());
        }
    }

    public void FindBricksAndSetRigidbodyType (RigidbodyType2D rigidbodyType)
    {
        colliders = Physics2D.OverlapCircleAll(transform.position, 100);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.GetComponent<Brick>() != null)
            {
                colliders[i].gameObject.GetComponent<Brick>().ChangeRigidbodyType(rigidbodyType);
                colliders[i].gameObject.GetComponent<Brick>().polygonCollider2D.isTrigger = true;
            }
        }
    }

    public void OnMainMenuActions()
    {
        m_CanPlay = false;
        m_BallsAmount = 1;

        m_BallsText.text = "x" + m_BallsAmount.ToString();

        m_BallSprite.enabled = true;
        m_DeactivatableChildren.SetActive(true);

        transform.position = m_DefaultStartPosition;
        m_BallSprite.transform.position = m_DefaultStartPosition;

        ResetPositions();

        m_TempAmount = 0;

        Ball.ResetReturningBallsAmount();

        m_ReturnBallsButton.SetActive(false);

        HideAllBalls();
    }

    public void ResetPositions()
    {
        m_StartPosition = Vector3.zero;
        m_EndPosition = Vector3.zero;
        m_WorldPosition = Vector3.zero;
    }

    private void HideAllBalls()
    {
        for (int i = 0; i < m_Balls.Count; i++)
        {
            m_Balls[i].GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            m_Balls[i].Disable();
        }
    }

    private void SpawNewBall(int Amount)
    {
        for (int i = 0; i < Amount; i++)
        {
            m_Balls.Add(Instantiate(m_BallPrefab, transform.parent, false));
            m_Balls[m_Balls.Count - 1].transform.localPosition = transform.localPosition;
            m_Balls[m_Balls.Count - 1].transform.localScale = transform.localScale;
            m_Balls[m_Balls.Count - 1].Disable();
        }
    }

    IEnumerator StartShootingBalls()
    {
        m_ReturnBallsButton.SetActive(true);
        m_BallSprite.enabled = false;

        int balls = m_BallsAmount;
        
        for (int i = 0; i < m_BallsAmount; i++)
        {
            if (m_CanPlay)
                break;

            m_Balls[i].transform.position = transform.position;
            m_Balls[i].GetReadyAndAddForce(m_Direction);
            
            balls--;
            m_BallsText.text = "x" + balls.ToString();
            
            yield return new WaitForSeconds(0.05f);
        }

        if(balls <= 0)
            m_DeactivatableChildren.SetActive(false);
    }

    public void ActivateHUD()
    {
        m_BallsAmount += m_TempAmount;

        // avoiding more balls than final brick level - I SHOULD AVOID THIS. IF I will use extra balls. Bochkarev Aleksei
        /*
        if (m_BallsAmount > ScoreManager.Instance.m_LevelOfFinalBrick)
            m_BallsAmount = ScoreManager.Instance.m_LevelOfFinalBrick;
        */
        m_TempAmount = 0;

        m_BallsText.text = "x" + m_BallsAmount.ToString();
        m_DeactivatableChildren.SetActive(true);
        m_ReturnBallsButton.SetActive(false);
    }

    public void ReturnAllBallsToNewStartPosition()
    {
        //StopAllCoroutines();
        Debug.Log("ReturnAllBallsToNewStartPosition");
        if(Ball.s_FirstCollisionPoint != Vector3.zero)
        {
            transform.position = Ball.s_FirstCollisionPoint;
            Ball.ResetFirstCollisionPoint();
        }

        m_BallSprite.transform.position = transform.position;
        m_BallSprite.enabled = true;

        for (int i = 0; i < m_Balls.Count; i++)
        {
            m_Balls[i].DisablePhysics();
            m_Balls[i].MoveTo(transform.position, iTween.EaseType.easeInOutQuart, (Vector2.Distance(transform.position, m_Balls[i].transform.position) / 6.0f), "DeactiveSprite");
        }

        ResetPositions();

        Ball.ResetReturningBallsAmount();

        ScoreManager.Instance.UpdateScore();

        BrickSpawner.Instance.MoveDownBricksRows();
        BrickSpawner.Instance.SpawnNewBricks();

        ActivateHUD();
        m_CanPlay = true;
        FindBricksAndSetRigidbodyType(RigidbodyType2D.Dynamic);
    }

    public void ChangeCollider()
    {
        var line = GetComponent<LineRenderer>();

        //get pos
        var pos = new Vector3[line.positionCount];
        line.GetPositions(pos);

        Vector2[] pos2 = ConvertArray(pos);

        //change points of edgeCollider
        edgeCollider2D.points = pos2;
    }

    public void IncreaseBallsAmountFromOutSide(int amout)
    {
        m_BallsAmount += amout;
        m_BallsText.text = "x" + m_BallsAmount.ToString();
    }

    Vector2[] ConvertArray(Vector3[] v3)
    {
        Vector2[] v2 = new Vector2[v3.Length];
        for (int i = 0; i < v3.Length; i++)
        {
            Vector3 tempV3 = v3[i];
            v2[i] = new Vector2(tempV3.x, tempV3.y);
        }
        return v2;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("GameObject2 collided with " + col.name);
        if (col.gameObject.GetComponent<Brick>() != null)
        {
            colliderTriggered = true;
        }   
    }

    void OnTriggerStay2D(Collider2D col)
    {
        //Debug.Log("GameObject2 stay with " + col.name);
        if (col.gameObject.GetComponent<Brick>() != null)
        {
            //m_LineRenderer.SetPosition(1, col.transform.position - ballStartPosition.transform.position);
            //Debug.Log("position " + new Vector3(((col.transform.position.y - ballStartPosition.transform.position.y) * (worldPosition.x - ballStartPosition.transform.position.x)) / (worldPosition.y - ballStartPosition.transform.position.y) + ballStartPosition.transform.position.x, col.transform.position.y, worldPosition.z));
        }
    }

    void OnTriggerExit2D (Collider2D col)
    {
        if (col.gameObject.GetComponent<Brick>() != null)
        {
            colliderTriggered = false;
        }
    }
}