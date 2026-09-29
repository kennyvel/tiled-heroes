using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputController : MonoBehaviour
{
    // Start is called before the first frame update
    private RangeFinder m_rangeFinder;
    private List<OverlayTile> m_inMovementRangeTiles;
    private List<OverlayTile> m_inAttackRangeTiles;

    public Joystick m_joystick;

    private Unit m_selectedUnit;
    private Unit m_selectedEnemyUnit;
    private float m_cameraPanSpeed = 4.0f;

    private bool m_undo = false;
    private GraphicRaycaster m_graphicRaycaster;
    private EventSystem m_eventSystem;

    public AudioSource m_moveSound;

    void Start()
    {
        // Hidden at first
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        m_inMovementRangeTiles = new List<OverlayTile>();
        m_inAttackRangeTiles = new List<OverlayTile>();
        m_rangeFinder = new RangeFinder();
        
        m_graphicRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
        m_eventSystem = GameObject.Find("EventSystem").GetComponent<EventSystem>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var focusedTileHit = GetFocusedOnTile();
    #if UNITY_EDITOR
        if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.position += Vector3.up * m_cameraPanSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.position += Vector3.left * m_cameraPanSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.position += Vector3.down * m_cameraPanSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.position += Vector3.right * m_cameraPanSpeed * Time.deltaTime;
        }
    #endif

        if (m_joystick.Vertical > 0.0f)
        {
            Camera.main.transform.position += Vector3.up * m_cameraPanSpeed * m_joystick.Vertical * Time.deltaTime;
        }
        if (m_joystick.Horizontal < 0.0f)
        {
            Camera.main.transform.position += Vector3.right * m_cameraPanSpeed * m_joystick.Horizontal * Time.deltaTime;
        }
        if (m_joystick.Vertical < 0.0f)
        {
            Camera.main.transform.position += Vector3.up * m_cameraPanSpeed * m_joystick.Vertical * Time.deltaTime;
        }
        if (m_joystick.Horizontal > 0.0f)
        {
            Camera.main.transform.position += Vector3.right * m_cameraPanSpeed * m_joystick.Horizontal * Time.deltaTime;
        }

        if (focusedTileHit.HasValue)
        {
            GameObject overlayTile = focusedTileHit.Value.collider.gameObject;
            transform.position = overlayTile.transform.position;
            transform.position += Vector3.back;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder =
                overlayTile.GetComponent<SpriteRenderer>().sortingOrder;
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            
            if (!GameManager.Instance.m_playerTurn)
            {
                return;
            }

        #if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                // Prevent selecting a tile when the UI is over it
                PointerEventData data = new PointerEventData(m_eventSystem);
                data.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                m_graphicRaycaster.Raycast(data, results);
                
                if (results.Count > 0)
                {
                    return;
                }
                
                InputLogic(overlayTile.GetComponent<OverlayTile>());
            }
        #endif

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    // Prevent selecting a tile when the UI is over it
                    PointerEventData data = new PointerEventData(m_eventSystem);
                    data.position = touch.position;
                    List<RaycastResult> results = new List<RaycastResult>();
                    m_graphicRaycaster.Raycast(data, results);
                
                    if (results.Count > 0)
                    {
                        return;
                    }
                    
                    InputLogic(overlayTile.GetComponent<OverlayTile>());
                }
            }
        }
    }

    private void InputLogic(OverlayTile overlayTile)
    {
        OverlayTile overlay = overlayTile.GetComponent<OverlayTile>();
        if (m_selectedUnit != null)
        {
            foreach (var item in m_inMovementRangeTiles)
            {
                if (item == overlay)
                {
                    PositionUnitOnTile(overlay);
                    break;
                }
            }

            foreach (var item in m_inAttackRangeTiles)
            {
                if (item == overlay)
                {
                    AttackTile(overlay);
                    break;
                }
            }
        }
        ResetTileSprites();
        SelectedUnitSwitch(overlay);
    }

    public RaycastHit2D? GetFocusedOnTile()
    {
        // Mouse cursor
        RaycastHit2D[] hits;
    #if UNITY_EDITOR
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        hits = Physics2D.RaycastAll(mousePos2D, Vector2.zero);

        if (hits.Length > 0)
        {
            return hits.OrderByDescending(i => i.collider.transform.position.z).First();
        }
    #endif
        
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
        
            // Mobile touch cursor
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                Vector2 touchPos2D = new Vector2(touchPos.x, touchPos.y);
            
                hits = Physics2D.RaycastAll(touchPos2D, Vector2.zero);
            
                if (hits.Length > 0)
                {
                    return hits.OrderByDescending(i => i.collider.transform.position.z).First();
                }
            }
        }

        return null;
    }

    private void GetInMovementRangeTiles()
    {
        foreach (var item in m_inMovementRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
            item.HideTile();
        }
        
        m_inMovementRangeTiles = m_rangeFinder.GetTilesInRange(m_selectedUnit.m_activeTile, m_selectedUnit.m_movementRange);
        m_inMovementRangeTiles.Remove(m_selectedUnit.m_activeTile);
        
        foreach (var item in m_inMovementRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
            item.ShowTile();
        }
    }
    
    private void GetInAttackRangeTiles(int range)
    {
        foreach (var item in m_inAttackRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
            item.HideTile();
        }
        
        m_inAttackRangeTiles = m_rangeFinder.GetTilesInRange(m_selectedUnit.m_activeTile, range);
        m_inAttackRangeTiles.Remove(m_selectedUnit.m_activeTile);

        foreach (var item in m_inAttackRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[1];
            item.ShowTile();
        }
    }
    
    private void GetEnemyAttackRangeTiles(int range)
    {
        foreach (var item in m_inAttackRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
            item.HideTile();
        }
        
        m_inAttackRangeTiles = m_rangeFinder.GetTilesInRange(m_selectedEnemyUnit.m_activeTile, range);
        m_inAttackRangeTiles.Remove(m_selectedEnemyUnit.m_activeTile);

        foreach (var item in m_inAttackRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[1];
            item.ShowTile();
        }
    }

    private void PositionUnitOnTile(OverlayTile tile)
    {
        if (tile.m_isBlocked || !m_selectedUnit.m_canMove)
        {
            return;
        }
        
        Vector3 pos = tile.transform.position;
        pos.y += .7f;
        pos.z -= 3.0f;
        m_selectedUnit.m_previousTile = m_selectedUnit.m_activeTile;
        m_selectedUnit.gameObject.transform.position = pos;
        m_selectedUnit.m_activeTile.m_isBlocked = false;
        m_selectedUnit.m_activeTile = tile;
        tile.m_isBlocked = true;
        m_selectedUnit.m_canMove = false;
        
        if (m_moveSound != null)
        {
            m_moveSound.Play();
        }

        if (m_undo)
        {
            GetInAttackRangeTiles(m_selectedUnit.m_attackRange + m_selectedUnit.m_movementRange);
            GetInMovementRangeTiles();
            m_undo = false;
        }
    }

    private void AttackTile(OverlayTile tile)
    {
        if (!m_selectedUnit.m_canAttack)
        {
            return;
        }

        if (m_selectedUnit.m_canMove)
        {
            int x = m_selectedUnit.m_activeTile.m_grid2DLocation.x;
            int y = m_selectedUnit.m_activeTile.m_grid2DLocation.y;
            int x2 = tile.m_grid2DLocation.x;
            int y2 = tile.m_grid2DLocation.y;

            int diffx = Mathf.Abs(x2 - x);
            int diffy = Mathf.Abs(y2 - y);

            if (diffx + diffy > m_selectedUnit.m_attackRange)
            {
                return;
            }
        }
        if (tile.m_isBlocked)
        {
            foreach (var unit in MapManager.Instance.m_enemyUnits)
            {
                if (unit.m_activeTile == tile)
                {
                    m_selectedUnit.m_canAttack = false;
                    m_selectedUnit.m_canMove = false;
                    unit.TakeDamage(m_selectedUnit.m_attackDamage);
                    GameManager.Instance.PlayAttackSound();
                    if (unit == null)
                    {
                        return;
                    }
                    // If the attacked unit can counterattack and is still alive, make the unit counterattack
                    if (unit.CanCounterAttack(m_selectedUnit.m_activeTile))
                    {
                        m_selectedUnit.TakeDamage(unit.m_attackDamage);
                    }
                    break;
                }
            }
        }
    }

    void ResetTileSprites()
    {
        foreach (var item in m_inAttackRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
            item.HideTile();
        }
        foreach (var item in m_inMovementRangeTiles)
        {
            item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
            item.HideTile();
        }

        m_inAttackRangeTiles.Clear();
        m_inMovementRangeTiles.Clear();
    }

    private void SelectedUnitSwitch(OverlayTile tile)
    {
        m_selectedUnit = null;
        m_selectedEnemyUnit = null;
        foreach (var unit in MapManager.Instance.m_playerUnits)
        {
            if (unit.m_activeTile == tile)
            {
                m_selectedUnit = unit;
                break;
            }
        }

        if (m_selectedUnit != null)
        {
            if (m_selectedUnit.m_canAttack && m_selectedUnit.m_canMove)
            {
                //Debug.Log("CAN ATTACK AND MOVE SHOW ATTACK AND MOVE TILES");
                GetInAttackRangeTiles(m_selectedUnit.m_attackRange + m_selectedUnit.m_movementRange);
                GetInMovementRangeTiles();
            }
            else if (m_selectedUnit.m_canAttack && !m_selectedUnit.m_canMove)
            {
                //Debug.Log("CAN ATTACK SHOW ATTACK TILES");
                GetInAttackRangeTiles(m_selectedUnit.m_attackRange);
            }
        }
        
        foreach (var unit in MapManager.Instance.m_enemyUnits)
        {
            if (unit.m_activeTile == tile)
            {
                m_selectedEnemyUnit = unit;
                ShowEnemyUnitAttackRange();
            }
        }

        if (m_selectedEnemyUnit != null && m_selectedEnemyUnit.m_hitPoints <= 0)
        {
            ResetTileSprites();
        }
    }
    public void EndTurn()
    {
        GameManager.Instance.SwapTurns();
    }

    public void UndoMove()
    {
        // Look for a selected unit
        if (m_selectedUnit != null && !m_selectedUnit.m_previousTile.m_isBlocked)
        {
            // Only allow undoing when the unit has only moved
            if (!m_selectedUnit.m_canMove && m_selectedUnit.m_canAttack)
            {
                // Undo this turn's move for the selected unit
                if (m_selectedUnit.m_previousTile != null)
                {
                    m_undo = true;
                    m_selectedUnit.m_canMove = true;
                    PositionUnitOnTile(m_selectedUnit.m_previousTile);
                    m_selectedUnit.m_canMove = true;
                    m_selectedUnit.m_previousTile = null;
                }
            }
        }
    }

    public void ShowEnemyUnitAttackRange()
    {
        GetEnemyAttackRangeTiles(m_selectedEnemyUnit.m_attackRange + m_selectedEnemyUnit.m_movementRange);
    }
}
