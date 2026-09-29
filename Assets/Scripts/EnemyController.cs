using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private RangeFinder m_rangeFinder;
    private List<OverlayTile> m_inMovementRangeTiles;
    private List<OverlayTile> m_inAttackRangeTiles;

    private Unit m_selectedUnit;
    private bool m_routineStarted = false;

    public AudioSource m_moveSound;
    
    // Start is called before the first frame update
    void Start()
    {
        m_inAttackRangeTiles = new List<OverlayTile>();
        m_inMovementRangeTiles = new List<OverlayTile>();
        m_rangeFinder = new RangeFinder();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.Instance.m_playerTurn && !m_routineStarted)
        {
            StartCoroutine(EnemyTurn());
            m_routineStarted = true;
            //Debug.Log("END ENEMY TURN");
        }
    }

    private IEnumerator EnemyTurn()
    {
        //Debug.Log("ENEMY TURN");
        yield return new WaitForSecondsRealtime(0.1f);
        foreach (var unit in MapManager.Instance.m_enemyUnits.ToArray())
        {
            Unit attackedUnit = null;
            m_selectedUnit = unit;
            if (unit.m_canAttack)
            {
                GetInAttackRangeTiles(unit.m_attackRange);
                OverlayTile bestAttack = null;
                float bestDist = 10000.0f;
                foreach (var item in m_inAttackRangeTiles)
                {
                    foreach (var playerUnit in MapManager.Instance.m_playerUnits)
                    {
                        if (playerUnit.m_activeTile == item)
                        {
                            float dist = Vector2Int.Distance(playerUnit.m_activeTile.m_grid2DLocation,
                                item.m_grid2DLocation);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestAttack = item;
                                attackedUnit = playerUnit;
                            }
                        }
                    }     
                }

                // Attack bestAttack!
                if (bestAttack != null)
                {
                    AttackTile(bestAttack);
                    if (attackedUnit != null)
                    {
                        yield return attackedUnit.DamageAnimation();
                    }
                }
                
                if (unit == null)
                {
                    continue;
                }
            }
            
            if (unit.m_canMove)
            {
                GetInMovementRangeTiles();
                OverlayTile bestMove = null;
                float bestDist = 10000.0f;
                foreach (var item in m_inMovementRangeTiles)
                {
                    foreach (var playerUnit in MapManager.Instance.m_playerUnits)
                    {
                        if (item.m_isBlocked)
                        {
                            continue;
                        }
                        //float currentDist = Vector3.Distance(playerUnit.gameObject.transform.position, unit.gameObject.transform.position);
                        float dist = Vector2Int.Distance(playerUnit.m_activeTile.m_grid2DLocation,
                            item.m_grid2DLocation);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestMove = item;
                            attackedUnit = playerUnit;
                        }
                    }
                }

                // Move to bestMove!
                if (bestMove != null)
                {
                    PositionUnitOnTile(bestMove);
                }
            }
            
            if (unit.m_canAttack)
            {
                GetInAttackRangeTiles(unit.m_attackRange);
                OverlayTile bestAttack = null;
                float bestDist = 10000.0f;
                foreach (var item in m_inAttackRangeTiles)
                {
                    foreach (var playerUnit in MapManager.Instance.m_playerUnits)
                    {
                        if (playerUnit.m_activeTile == item)
                        {
                            float dist = Vector2Int.Distance(playerUnit.m_activeTile.m_grid2DLocation,
                                item.m_grid2DLocation);
                            if (dist < bestDist)
                            {
                                bestDist = dist;
                                bestAttack = item;
                            }
                        }
                    }     
                }
                
                // Attack bestAttack!
                if (bestAttack != null)
                {
                    AttackTile(bestAttack);
                    if (attackedUnit != null)
                    {
                        yield return attackedUnit.DamageAnimation();
                    }
                }
                if (unit == null)
                {
                    continue;
                }
            }

            yield return new WaitForSecondsRealtime(0.6f);
        }
        GameManager.Instance.SwapTurns();
        m_routineStarted = false;
        StopCoroutine(EnemyTurn());
    }

    private void GetInMovementRangeTiles()
    {
        m_inMovementRangeTiles = m_rangeFinder.GetTilesInRange(m_selectedUnit.m_activeTile, m_selectedUnit.m_movementRange);
    }
    
    private void GetInAttackRangeTiles(int range)
    {
        m_inAttackRangeTiles = m_rangeFinder.GetTilesInRange(m_selectedUnit.m_activeTile, range);
    }

    private void PositionUnitOnTile(OverlayTile tile)
    {
        Vector3 pos = tile.transform.position;
        pos.y += .7f;
        pos.z -= 3.0f;
        m_selectedUnit.gameObject.transform.position = pos;
        m_selectedUnit.m_activeTile.m_isBlocked = false;
        m_selectedUnit.m_activeTile = tile;
        tile.m_isBlocked = true;
        m_selectedUnit.m_canMove = false;

        if (m_moveSound != null)
        {
            m_moveSound.Play();
        }
    }

    private void AttackTile(OverlayTile tile)
    {
        if (!m_selectedUnit.m_canAttack)
        {
            return;
        }
        if (tile.m_isBlocked)
        {
            foreach (var unit in MapManager.Instance.m_playerUnits)
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
                    // If the attacked unit can counterattack, make the unit counterattack
                    if (unit.CanCounterAttack(m_selectedUnit.m_activeTile))
                    {
                        m_selectedUnit.TakeDamage(unit.m_attackDamage);
                    }
                    break;
                }
            }
        }
    }

    // private void ShowAllEnemyAttackTiles()
    // {
    //     foreach (var item in m_AllEnemyAttackTiles)
    //     {
    //         item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[0];
    //         item.HideTile();
    //     }
    //     m_AllEnemyAttackTiles.Clear();
    //
    //     foreach (var unit in MapManager.Instance.m_enemyUnits)
    //     {
    //         m_AllEnemyAttackTiles.AddRange(m_rangeFinder.GetTilesInRange(unit.m_activeTile,
    //             unit.m_attackRange + unit.m_movementRange));
    //     }
    //
    //     foreach (var unit in MapManager.Instance.m_enemyUnits)
    //     {
    //         m_AllEnemyAttackTiles.Remove(unit.m_activeTile);
    //     }
    //     
    //     foreach (var item in m_AllEnemyAttackTiles)
    //     {
    //         item.GetComponent<SpriteRenderer>().sprite = MapManager.Instance.m_tileSprites[1];
    //         item.ShowTile();
    //     }
    // }
}
