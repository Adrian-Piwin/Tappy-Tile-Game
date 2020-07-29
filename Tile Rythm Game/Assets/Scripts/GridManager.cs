﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    // Unity editor variables
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Sprite[] sprites = new Sprite[4];
    [SerializeField]
    private int rows = 5;
    [SerializeField]
    private int cols = 5;
    [SerializeField]
    private float tileSize = 1.05f;
    [SerializeField]
    private float spawnInterval = 0.8f;
    [SerializeField]
    private float tileUptime = 1.0f;
    [SerializeField]
    private float sliderTileDelay = 0.03f;
    [SerializeField]
    private int tileSlideSizeMin = 2;
    [SerializeField]
    private int tileSlideSizeMax = 3;

    // Arrays and Lists
    private GameObject[,] gridArray;
    // 0 = grey tile, 1 = green tile
    private int[,] gameArray;
    private string[] coords;
    private int[] tempArray;
    private List<GameObject> queuedTileOrder = new List<GameObject>();
    private List<IEnumerator> queuedTileTimer = new List<IEnumerator>();
    private List<int> directionList = new List<int>();
    private List<int> dirList = new List<int>();
    private List<int[]> tempList;

    // Booleans
    public bool tileClicked = false;
    private bool firstTileClicked;
    private bool gamePlaying = false;
    private int tileNumMax = 9;
    private int slideTileProbability = 3;
    private int rowCoord, colCoord, rowAdd, colAdd, tempRow, tempCol, ind, score, tileNum, dir;

    // Ienumerators
    IEnumerator tileSpawnTimer,tileTimer;

    // Other
    private GameObject currentClickedTile;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        GenerateGrid();
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        // Do something if a tile is clicked
        if (tileClicked && gamePlaying)
        {
            // Check if tile matches the current queued tile 
            if (queuedTileOrder.Count != 0){
                if (currentClickedTile == queuedTileOrder[0]){

                    queuedTileOrder.RemoveAt(0);

                    // Play sound effect
                    audioManager.playTileClickedSound();

                    // Update game array
                    coords = (currentClickedTile.name).Split(',');
                    gameArray[int.Parse(coords[0]), int.Parse(coords[1])] = 0;

                    // Update tile sprite and number
                    changeTileSprite(currentClickedTile, 0);
                    currentClickedTile.transform.GetChild (0).gameObject.GetComponent<TextMesh>().text = "";

                    // Update score
                    score += 1;
                    updateScoreText();
                    
                    // Start spawning tiles once the first tile is clicked
                    if (firstTileClicked)
                    {
                        StartCoroutine(tileSpawnTimer);
                        firstTileClicked = false;

                    }else{
                        // Avoid deleting non existent timer for first tile
                        StopCoroutine(queuedTileTimer[0]);
                        queuedTileTimer.RemoveAt(0);
                    }

                }else{
                    // Lose if wrong tile is pressed
                    if(!firstTileClicked)
                    {
                        changeTileSprite(currentClickedTile, 3);
                        gameLost();
                    }
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
        gamePlaying = true;
        queuedTileOrder.Clear();
        queuedTileTimer.Clear();
        tileNum = 0;
        score = 0;
        updateScoreText();
        gameArray = new int[rows, cols];

        tileSpawnTimer = spawnTile(spawnInterval);
        firstTileClicked = true;
        spawnFirstTile();
    }

    // Spawn a tiles using the same constant interval
    IEnumerator spawnTile(float spwnInterval){
        while (true)
        {
            yield return new WaitForSeconds(spwnInterval);

            if(Random.Range(0,10) > slideTileProbability){
                getSpawnCoords(out rowCoord, out colCoord);
                setupNewTile(rowCoord, colCoord);
            }else{
                StartCoroutine(spawnTileSlider(Random.Range(tileSlideSizeMin,tileSlideSizeMax+1)));
            }
            
        }
    }

    // Spawn a tiles using the same constant interval
    IEnumerator spawnTileSlider(int tileSlideSize){
        // 2d array to hold coords that the tiles for the slider will spawn with
        tempList = new List<int[]>();
        tempArray = new int[2];
        
        while (true)
        {
            directionList.Clear();
            // Reset old tiles if error
            for (int i = 0; i < tempList.Count; i++){
                gameArray[tempList[i][0], tempList[i][1]] = 0;
            }
            tempList.Clear();
            // Choose base point
            getSpawnCoords(out rowCoord, out colCoord);
            // Temp have coords as taken up space
            gameArray[rowCoord, colCoord] = 1;

            tempRow = rowCoord;
            tempCol = colCoord;

            // Create list of random and possible directions to spawn the next tile, touching the prev tile
            
            for (int i = 1; i < tileSlideSize; i++)
            {
                dir = chooseRandomDirection(tempRow, tempCol);

                if (dir == -1){
                    break;
                }

                directionList.Add(dir);
                getDirectionModifiers(dir, out rowAdd, out colAdd);

                tempRow += rowAdd;
                tempCol += colAdd;
                
                gameArray[tempRow, tempCol] = 1;
                
                tempArray[0] = tempRow;
                tempArray[1] = tempCol;
                tempList.Add(tempArray);
            }

            // Continue if correct amount of tiles were created
            if (directionList.Count == tileSlideSize-1){
                break;
            }

            // Reset coords if error
            gameArray[rowCoord, colCoord] = 0;

        }

        // Reset tile num to make it easier to see the order
        tileNum = 0;

        rowAdd = 0;
        colAdd = 0;
        // Create tiles in order of direction list
        for (int i = 0; i < tileSlideSize; i++){
            yield return new WaitForSeconds(sliderTileDelay);
            rowCoord += rowAdd;
            colCoord += colAdd;
            setupNewTile(rowCoord, colCoord);

            if (i < tileSlideSize-1)
            {
                getDirectionModifiers(directionList[i], out rowAdd, out colAdd);
            }
            
        }
        
    }

    // Given a row and col, return a random possible direction the next tile could be placed
    private int chooseRandomDirection(int rowBase, int colBase){
        dirList.Clear();
        int tmpRow, tmpCol;
        
        for (int i = 0; i < 4; i++)
        {
            tmpRow = rowBase;
            tmpCol = colBase;
            getDirectionModifiers(i, out rowAdd, out colAdd);
            tmpRow += rowAdd;
            tmpCol += colAdd;

            // Check if out of bounds, or already taken spot
            if (tmpRow < 0 || tmpRow >= rows || tmpCol < 0 || tmpCol >= cols)
            {
                continue;
            }else if (gameArray[tmpRow, tmpCol] == 1){
                continue;
            }

            dirList.Add(i);
            
        }

        // If there is no direction for the next tile to go, return error
        if (dirList.Count == 0){
            return -1;
        }

        // Return one of the avaiable directions randomly
        return dirList[Random.Range(0,dirList.Count)];
    }

    // Spawn the first tile without a time out timer
    private void spawnFirstTile(){
        getSpawnCoords(out rowCoord, out colCoord);
        changeTileSprite(gridArray[rowCoord,colCoord], 1);
        tileNum += 1;
        if (tileNum >= tileNumMax){
            tileNum = 1;
        }
        gridArray[rowCoord,colCoord].transform.GetChild (0).gameObject.GetComponent<TextMesh>().text = ("" + tileNum);

        queuedTileOrder.Add(gridArray[rowCoord,colCoord]);
    }

    // Creation of adding a tile to the grid
    private void setupNewTile(int row, int col)
    {
        // Update sprite
        changeTileSprite(gridArray[row,col], 1);
        // Update game array
        gameArray[row, col] = 1;

        // Get number for the tile and update the tile
        tileNum += 1;
        if (tileNum >= tileNumMax){
            tileNum = 1;
        }
        gridArray[row,col].transform.GetChild (0).gameObject.GetComponent<TextMesh>().text = ("" + tileNum);

        // Set up timers & append to queues
        tileTimer = tileUptimeFinished(gridArray[row,col]);
        StartCoroutine(tileTimer);
        queuedTileTimer.Add(tileTimer);
        queuedTileOrder.Add(gridArray[row,col]);
    }

    // Timer for each tile, lose game if timer ends
    IEnumerator tileUptimeFinished(GameObject tile)
    {
        yield return new WaitForSeconds(tileUptime);
        changeTileSprite(tile, 3);

        gameLost();
    }

    // Loser handler
    private void gameLost()
    {
        gamePlaying = false;
        StopCoroutine(tileSpawnTimer);

        foreach (IEnumerator tileTimer in queuedTileTimer){
            StopCoroutine(tileTimer);
        }
    }

    // Out a random available row and column
    private void getSpawnCoords(out int x, out int y)
    {
        while (true)
        {
            x = Random.Range(0,rows);
            y = Random.Range(0,cols);

            if (gameArray[x, y] == 0){
                break;
            }
        }
    }

    // Out the x and y addition depending on direction
    private void getDirectionModifiers(int dir, out int rowMod, out int colMod){

        switch (dir){
            case 0: // down
                rowMod=1;
                colMod=0;
                break;
            case 1: // up
                rowMod=-1;
                colMod=0;
                break;
            case 2: // right
                rowMod=0;
                colMod=1;
                break;
            case 3: // left
                rowMod=0;
                colMod=-1;
                break;
            default:
                rowMod=0;
                colMod=0;
                Debug.Log("Error: 1");
                break;
            
        }
    }

    // Change a tile's sprite
    private void changeTileSprite(GameObject obj, int chosen)
    {
        obj.GetComponent<SpriteRenderer>().sprite = sprites[chosen];
    }

    // Reset game
    public void resetGame(){
        StopCoroutine(tileSpawnTimer);

        // Reset tiles
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {   
                changeTileSprite(gridArray[row,col], 0);
            }
        }

        // Reset tile timers
        foreach (IEnumerator tileTimer in queuedTileTimer){
            StopCoroutine(tileTimer);
        }

        // Reset text on tiles
        foreach(GameObject tile in queuedTileOrder)
        {
            tile.transform.GetChild (0).gameObject.GetComponent<TextMesh>().text = "";
        }

        StartGame();
    }

    // Update score text on screen
    private void updateScoreText(){
        scoreText.text = ("Score: " + score);
    }

    // Get tile that was clicked
    public void updateTileClicked(GameObject tile)
    {
        tileClicked = true;
        currentClickedTile = tile;
    }
    
}
