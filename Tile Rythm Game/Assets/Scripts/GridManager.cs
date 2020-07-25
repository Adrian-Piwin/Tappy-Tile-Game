﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int rows = 5;
    [SerializeField]
    private int cols = 5;
    [SerializeField]
    private float tileSize = 1;
    [SerializeField]
    private Sprite[] sprites = new Sprite[4];
    [SerializeField]
    private bool gamePlaying = false;

    private GameObject[,] gridArray;
    // 0 = grey tile, 1 = green tile
    private int[,] gameArray;
    private int rowCoord, colCoord;
    private List<GameObject> queuedTileOrder = new List<GameObject>();
    private List<IEnumerator> queuedTileTimer = new List<IEnumerator>();
    private GameObject currentClickedTile;

    public bool tileClicked = false;
    static float spawnInterval = 1.0f;
    private float tileUptime = 3.0f;
    private int score = 0;

    IEnumerator tileSpawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        // Do something if a tile is clicked
        if (tileClicked)
        {
            // Check if tile matches the current queued tile 
            if (queuedTileOrder.Count != 0){
                if (currentClickedTile == queuedTileOrder[0]){
                    queuedTileOrder.RemoveAt(0);
                    StopCoroutine(queuedTileTimer[0]);
                    queuedTileTimer.RemoveAt(0);

                    changeTileSprite(currentClickedTile, 0);
                    score += 1;
                }else{
                    gameLost();
                }
            }
            
            tileClicked = false;
        }

        
    }

    // Generate grid of tiles, using tile reference in resources
    private void GenerateGrid()
    {
        GameObject referenceTile = (GameObject)Instantiate(Resources.Load("tile-0"));
        gridArray = new GameObject[rows, cols];

        for (int row = 0; row < rows; row++)
        {

            for (int col = 0; col < cols; col++)
            {
                GameObject tile = (GameObject)Instantiate(referenceTile, transform);
                tile.name = row + "," + col;

                float posX = col * tileSize;
                float posY = row * -tileSize;

                tile.transform.position = new Vector2(posX, posY);

                gridArray[row, col] = tile;
            }
        }

        Destroy(referenceTile);

        float gridW = cols * tileSize;
        float gridH = rows * tileSize;
        transform.position = new Vector2(-gridW / 2 + tileSize / 2, gridH / 2 - tileSize / 2);
    }

    // Start game
    private void StartGame()
    {
        //queuedTileOrder.Add(gridArray[0,1]);
        //changeTileSprite(gridArray[0,1], 1);
        gamePlaying = true;
        queuedTileOrder.Clear();
        queuedTileTimer.Clear();
        score = 0;
        gameArray = new int[rows, cols];
        tileSpawnTimer = spawnTile();
        StartCoroutine(tileSpawnTimer);
    }

    IEnumerator spawnTile(){
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            getSpawnCoords(out rowCoord, out colCoord);
            changeTileSprite(gridArray[rowCoord,colCoord], 1);

            IEnumerator tileTimer = tileUptimeFinished(gridArray[rowCoord,colCoord]);
            StartCoroutine(tileTimer);
            queuedTileOrder.Add(gridArray[rowCoord,colCoord]);
            queuedTileTimer.Add(tileTimer);
        }
    }

    IEnumerator tileUptimeFinished(GameObject tile)
    {
        yield return new WaitForSeconds(tileUptime);
        changeTileSprite(tile, 3);

        gameLost();
    }

    private void gameLost()
    {
        StopCoroutine(tileSpawnTimer);

        foreach (IEnumerator tileTimer in queuedTileTimer){
            StopCoroutine(tileTimer);
        }

        Debug.Log("Score: " + score);
    }

    private void getSpawnCoords(out int x, out int y)
    {
        while (true)
        {
            x = Random.Range(0,rows);
            y = Random.Range(0,cols);

            if (gameArray[x, y] == 0){
                gameArray[x, y] = 1;
                break;
            }
        }
    }

    // Change a tile's sprite
    private void changeTileSprite(GameObject obj, int chosen)
    {
        obj.GetComponent<SpriteRenderer>().sprite = sprites[chosen];
    }

    // Reset game
    public void resetGame(){
        gamePlaying = false;
        StopCoroutine(tileSpawnTimer);

        // Reset tiles
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {   
                changeTileSprite(gridArray[row,col], 0);
            }
        }

        StartGame();
    }

    // Get tile that was clicked
    public void updateTileClicked(GameObject tile)
    {
        tileClicked = true;
        currentClickedTile = tile;
    }
}
