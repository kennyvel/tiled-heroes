using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayTile : MonoBehaviour
{
    public Vector3Int m_gridLocation;
    public bool m_isBlocked = false;
    
    public Vector2Int m_grid2DLocation { get { return new Vector2Int(m_gridLocation.x, m_gridLocation.y); } }
    
    void Start()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
    }

    // Update is called once per frame
    void Update()
    {
    #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HideTile();
        }
    #endif

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
            {
                HideTile();
            }
        }
    }

    public void ShowTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
    }
    
    public void HideTile()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
    }
}
