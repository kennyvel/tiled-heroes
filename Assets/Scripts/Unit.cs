using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public float m_hitPoints;
    private float m_startingHitPoints;
    public int m_attackDamage;
    public int m_defense;
    public int m_attackRange;
    public int m_movementRange;
    public bool m_canMove = true;
    public bool m_canAttack = true;

    public OverlayTile m_activeTile;
    public OverlayTile m_previousTile;

    private Image m_healthBar;
    
    // Start is called before the first frame update
    void Start()
    {
        m_startingHitPoints = m_hitPoints;
        m_healthBar = transform.GetChild(0).GetChild(2).gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void TakeDamage(int damage)
    {
        m_hitPoints -= (damage - m_defense);
        StartCoroutine(DamageAnimation());
    }

    public bool CanCounterAttack(OverlayTile enemyTile)
    {
        Vector2Int enemyPos = enemyTile.m_grid2DLocation;
        Vector2Int pos = m_activeTile.m_grid2DLocation;

        int diff = Mathf.Abs(enemyPos.x - pos.x) + Mathf.Abs(enemyPos.y - pos.y);
        // unit can counter attack if it isn't dead and if the distance between the attacking unit and itself is less than this unit's attack range
        return diff <= m_attackRange && m_hitPoints > 0;
    }

    public IEnumerator DamageAnimation()
    {
        // play a sound here too!
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSecondsRealtime(0.1f);
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSecondsRealtime(0.1f);
        gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSecondsRealtime(0.1f);
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        m_healthBar.fillAmount = m_hitPoints / m_startingHitPoints;

        if (m_hitPoints <= 0 && this.gameObject != null)
        {
            // Destroy unit
            m_activeTile.m_isBlocked = false;
            GameManager.Instance.PlayDeathSound();
            yield return new WaitForSecondsRealtime(0.1f);
            Destroy(this.gameObject);
        }
    }

    public void EndUnitTurn()
    {
        m_canAttack = false;
        m_canMove = false;
    }
}
