using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool m_playerTurn = true;
    
    private static GameManager m_instance;

    public AudioSource m_attackSound;
    public AudioSource m_deathSound;

    public Text m_levelStatusText;

    public Text m_turnText;
    private bool m_levelEnd = false;
    
    public static GameManager Instance
    {
        get { return m_instance; }
    }
    
    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            m_instance = this;
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        m_turnText.color = Color.cyan;
    }

    // Update is called once per frame
    void Update()
    {
        MapManager.Instance.m_enemyUnits.RemoveAll(unit => unit == null);
        MapManager.Instance.m_playerUnits.RemoveAll(unit => unit == null);

        if (MapManager.Instance.m_playerUnits.Count == 0 && !m_levelEnd)
        {
            // player loses
            m_levelEnd = true;
            StartCoroutine(PlayerLoss());
        }

        if (MapManager.Instance.m_enemyUnits.Count == 0 && !m_levelEnd)
        {
            // player wins
            m_levelEnd = true;
            StartCoroutine(PlayerWin());
        }
    }

    private void ResetEnemyUnitActions()
    {
        //Debug.Log("RESET ENEMY");
        foreach (var unit in MapManager.Instance.m_enemyUnits)
        {
            unit.m_canAttack = true;
            unit.m_canMove = true;
        }
    }
    
    private void ResetPlayerUnitActions()
    {
        //Debug.Log("RESET PLAYER");
        foreach (var unit in MapManager.Instance.m_playerUnits)
        {
            unit.m_canAttack = true;
            unit.m_canMove = true;
        }
    }

    public void SwapTurns()
    {
        if (m_playerTurn)
        {
            m_playerTurn = false;
            ResetEnemyUnitActions();
            // Do ui things?
            if (m_turnText != null)
            {
                m_turnText.text = "Enemy Turn";
                m_turnText.color = Color.red;
            }
        }
        else
        {
            m_playerTurn = true;
            ResetPlayerUnitActions();
            // Do ui things?
            if (m_turnText != null)
            {
                m_turnText.text = "Player Turn";
                m_turnText.color = Color.cyan;
            }
        }
    }

    public void PlayDeathSound()
    {
        if (m_deathSound != null && !m_deathSound.isPlaying)
        {
            m_deathSound.Play();
        }
    }

    public void PlayAttackSound()
    {
        if (m_attackSound != null && !m_attackSound.isPlaying)
        {
            m_attackSound.Play();
        }
    }

    private IEnumerator PlayerWin()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        m_levelStatusText.text = "Victory!";
        m_levelStatusText.color = Color.cyan;
        yield return new WaitForSecondsRealtime(2.0f);
        // Launch the next level (add 1)
        if (SceneManager.GetActiveScene().buildIndex < 8)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            m_levelStatusText.text = "Thanks for playing!";
        }
    }

    private IEnumerator PlayerLoss()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        m_levelStatusText.text = "Defeat...";
        m_levelStatusText.color = Color.red;
        yield return new WaitForSecondsRealtime(2.0f);
        // Relaunch the current level
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
