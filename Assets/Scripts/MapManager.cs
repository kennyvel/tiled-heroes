using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = System.Numerics.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MapManager : MonoBehaviour
{
    public List<Unit> m_playerUnitPrefabs;
    public List<Unit> m_enemyUnitPrefabs;

    public List<Unit> m_playerUnits;
    public List<Unit> m_enemyUnits;
    
    public List<Sprite> m_tileSprites;

    public List<Vector2Int> m_playerSpawnLocations;
    public List<Vector2Int> m_enemySpawnLocations;

    private static MapManager m_instance;
    public static MapManager Instance
    {
        get { return m_instance; }
    }

    public Dictionary<Vector2Int, OverlayTile> m_map;
    public OverlayTile m_movementTilePrefab;
    public OverlayTile m_attackTilePrefab;
    public GameObject m_overlayContainer;
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
        m_map = new Dictionary<Vector2Int, OverlayTile>();
        var tileMap = gameObject.GetComponentInChildren<Tilemap>();

        BoundsInt bounds = tileMap.cellBounds;

        // Looping through all our tiles in the tilemap
        for (int z = bounds.min.z; z < bounds.max.z; z++)
        {
            for (int y = bounds.min.y; y <= bounds.max.y; y++)
            {
                for (int x = bounds.min.x; x <= bounds.max.x; x++)
                {
                    var tileLocation = new Vector3Int(x, y, z);
                    var tileKey = new Vector2Int(x, y);
                    bool hasTile = tileMap.HasTile(tileLocation);

                    if (tileMap.HasTile(tileLocation))
                    {
                        var overlayTile = Instantiate(m_movementTilePrefab, m_overlayContainer.transform);
                        // x-direction is off by 1 for anything on top of the tile
                        var offset = tileLocation + Vector3Int.right;

                        var cellWorldPosition = tileMap.GetCellCenterWorld(offset);

                        overlayTile.transform.position = new Vector3(cellWorldPosition.x, cellWorldPosition.y, cellWorldPosition.z - 1);
                        overlayTile.GetComponent<SpriteRenderer>().sortingOrder =
                            tileMap.GetComponent<TilemapRenderer>().sortingOrder;
                        overlayTile.m_gridLocation = tileLocation;
                        m_map.Add(tileKey, overlayTile);
                    }
                }
            }
        }

        SpawnUnits();
    }

    void SpawnUnits()
    {
        var tileMap = gameObject.GetComponentInChildren<Tilemap>();
        BoundsInt bounds = tileMap.cellBounds;

        m_playerUnits = new List<Unit>();
        m_enemyUnits = new List<Unit>();

        for(int i = 0; i < m_playerUnitPrefabs.Count; i++)
        {
            OverlayTile overlayTile = m_map[new Vector2Int(m_playerSpawnLocations[i].x, m_playerSpawnLocations[i].y)];
            Vector3 pos = overlayTile.transform.position;
            overlayTile.m_isBlocked = true;
            // Each tile's position is the bottom of the cube, bring the sprite up so its on top of the cube
            pos.y += .7f;
            // Make sure unit goes above everything else
            pos.z -= 3.0f;
            Unit unit = Instantiate(m_playerUnitPrefabs[i], pos, Quaternion.identity);
            unit.m_activeTile = overlayTile;
            m_playerUnits.Add(unit);
        }
        for(int i = 0; i < m_enemyUnitPrefabs.Count; i++)
        {
            OverlayTile overlayTile = m_map[new Vector2Int(m_enemySpawnLocations[i].x, m_enemySpawnLocations[i].y)];
            Vector3 pos = overlayTile.transform.position;
            overlayTile.m_isBlocked = true;
            // Each tile's position is the bottom of the cube, bring the sprite up so its on top of the cube
            pos.y += .7f;
            // Make sure unit goes above everything else
            pos.z -= 3.0f;
            Unit unit = Instantiate(m_enemyUnitPrefabs[i], pos, Quaternion.identity);
            unit.m_activeTile = overlayTile;
            m_enemyUnits.Add(unit);
        }
    }

    public List<OverlayTile> GetNeighborTiles(OverlayTile currentOverlayTile, List<OverlayTile> searchableTiles)
    {
        Dictionary<Vector2Int, OverlayTile> tileToSearch = new Dictionary<Vector2Int, OverlayTile>();

        if (searchableTiles.Count > 0)
        {
            foreach (var item in searchableTiles)
            {
                tileToSearch.Add(item.m_grid2DLocation, item);
            }
        }
        else
        {
            tileToSearch = m_map;
        }
        
        List<OverlayTile> neighbors = new List<OverlayTile>();

        // Top
        Vector2Int locationToCheck = new Vector2Int(
            currentOverlayTile.m_gridLocation.x,
            currentOverlayTile.m_gridLocation.y + 1
            );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }
        
        // Bottom
        locationToCheck = new Vector2Int(
            currentOverlayTile.m_gridLocation.x,
            currentOverlayTile.m_gridLocation.y - 1
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }
        
        // Right
        locationToCheck = new Vector2Int(
            currentOverlayTile.m_gridLocation.x + 1,
            currentOverlayTile.m_gridLocation.y
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }
        
        // Left
        locationToCheck = new Vector2Int(
            currentOverlayTile.m_gridLocation.x - 1,
            currentOverlayTile.m_gridLocation.y
        );
        if (tileToSearch.ContainsKey(locationToCheck))
        {
            neighbors.Add(tileToSearch[locationToCheck]);
        }

        return neighbors;
    }
}
