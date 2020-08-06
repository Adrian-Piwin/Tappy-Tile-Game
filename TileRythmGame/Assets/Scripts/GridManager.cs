using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GridManager : MonoBehaviour
{
    // Unity editor variables
    [SerializeField]
    private TextMeshProUGUI scoreText;
    [SerializeField]
    private Sprite tileSprite;
    [SerializeField]
    private int rows = 5;
    [SerializeField]
    private int cols = 5;
    [SerializeField]
    private float tileSize = 1.05f;

    // Difficulty settings
    private float spawnSingleInterval = 0.5f;
    private float spawnMultipleInterval = 0.8f;
    private float tileUptime = 1.0f;
    private float sliderTileDelay = 0.03f;
    private int tileSingleSizeMin = 3;
    private int tileSingleSizeMax = 6;
    private int tileSlideSizeMin = 2;
    private int tileSlideSizeMax = 3;
    private int slideTileProbability = 3;

    // Arrays and Lists
    private GameObject[,] gridArray;
    // 0 = grey tile, 1 = green tile
    private int[,] gameArray;
    private List<GameObject> queuedTileOrder = new List<GameObject>();
    private List<IEnumerator> queuedTileTimer = new List<IEnumerator>();
    private List<string> colorSetList;

    // Variables
    public bool tileClicked = false;
    private string currentDifficulty = "normal";
    private bool firstTileClicked, isMultipleTile;
    private bool gamePlaying = false;
    private int score, tileNum;
    private float spwnInterval;
    private int currentColorSet;

    // Ienumerators
    IEnumerator tileSpawnTimer;

    // Other
    private GameObject currentClickedTile;
    private MenuScript menuScript;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        menuScript = GameObject.FindObjectOfType<MenuScript>();
        setupColorSets();
        GenerateGrid();
        changeDifficulty(true);
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
                    audioManager.playSound("click");

                    // Update game array
                    string[] coords = (currentClickedTile.name).Split(',');
                    gameArray[int.Parse(coords[0]), int.Parse(coords[1])] = 0;

                    // Stop fading to red
                    currentClickedTile.GetComponent<FadeColorScript>().StopFade();

                    // Update tile sprite and number
                    changeTileColor(currentClickedTile, 0);
                    currentClickedTile.transform.GetChild (0).gameObject.GetComponent<TextMeshPro>().SetText("");

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
                        gameOver(currentClickedTile, false);
                    }
                }
            }
            
            tileClicked = false;
        }else if (tileClicked && !gamePlaying)
        {
            currentClickedTile = null;
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

    // Start Game
    public void startGame()
    {
        tileNum = 0;
        score = 0;
        updateScoreText();
        gameArray = new int[rows, cols];
        gamePlaying = true;
        
        // Reset tiles
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {   
                changeTileColor(gridArray[row,col], 0);
            }
        }

        // Reset text on tiles
        foreach(GameObject tile in queuedTileOrder)
        {
            tile.transform.GetChild (0).gameObject.GetComponent<TextMeshPro>().SetText("");
        }

        queuedTileOrder.Clear();
        queuedTileTimer.Clear();

        // Close Menu
        StartCoroutine(menuScript.toggleMenu(false, 0.1f));
        menuScript.toggleNewBestTime(false);

        // Spawn first tile
        tileSpawnTimer = spawnTiles();
        firstTileClicked = true;
        spawnFirstTile();
    }

    // Loser handler
    private void gameOver(GameObject lostTile, bool isTimedout)
    {
        // Stop spawning tiles
        gamePlaying = false;
        StopCoroutine(tileSpawnTimer);

        // Stop timers for queued tiles
        foreach (IEnumerator tileTimer in queuedTileTimer){
            StopCoroutine(tileTimer);
        }

        // Stop queued tiles from changing colors
        foreach (GameObject tile in queuedTileOrder)
        {
            tile.GetComponent<FadeColorScript>().StopFade();
        }

        // Loss effects
        StartCoroutine(blinkingLossTile(lostTile, isTimedout));
        audioManager.playSound("loss");
        
        // Launch menu
        StartCoroutine(menuScript.toggleMenu(true, 1.5f));

        // Update score
        menuScript.updateScore(score);
        if (checkNewBest())
        {
            if (currentDifficulty == "normal")
            {
                menuScript.updateBestTime(PlayerPrefs.GetInt("normalBestScore", 0));
            }else 
            {
                menuScript.updateBestTime(PlayerPrefs.GetInt("hardBestScore", 0));
            }
            
        }
    }

    // Change difficulty
    public void changeDifficulty(bool normal)
    {
        menuScript.updateScore(score);

        if (normal)
        {
            spawnSingleInterval = 0.25f;
            spawnMultipleInterval = 0.5f;
            tileUptime = 1.3f;
            tileSingleSizeMin = 3;
            tileSingleSizeMax = 6;
            tileSlideSizeMin = 3;
            tileSlideSizeMax = 6;
            sliderTileDelay = 0.08f;
            slideTileProbability = 6;

            menuScript.updateBestTime(PlayerPrefs.GetInt("normalBestScore", 0));
            currentDifficulty = "normal";
        }else{
            spawnSingleInterval = 0.20f;
            spawnMultipleInterval = 0.7f;
            tileUptime = 1f;
            tileSingleSizeMin = 4;
            tileSingleSizeMax = 8;
            tileSlideSizeMin = 6;
            tileSlideSizeMax = 10;
            sliderTileDelay = 0.08f;
            slideTileProbability = 6;

            menuScript.updateBestTime(PlayerPrefs.GetInt("hardBestScore", 0));
            currentDifficulty = "hard";
        }
    }

    // Spawn a tiles using the same constant interval
    IEnumerator spawnTiles(){
        isMultipleTile = false;
        bool isSingleTile = false;
        int rowCoord, colCoord, singleTileMax=0;

        while (true)
        {
            // If nothing is spawning, choose a type of tile to spawn
            if (!isSingleTile && !isMultipleTile && gamePlaying){  
                if(Random.Range(0,11) > slideTileProbability){
                    isMultipleTile = false;
                    isSingleTile = true;
                    spwnInterval = spawnSingleInterval;
                    singleTileMax = Random.Range(tileSingleSizeMax, tileSingleSizeMin);
                    
                }else{
                    isMultipleTile = true;
                    isSingleTile = false;
                    spwnInterval = spawnMultipleInterval;
                }

                if (!firstTileClicked){
                        tileNum = 0;
                    }
            }
            // Change spawn interval depending on type of tile spawning
            yield return new WaitForSeconds(spwnInterval);
            // Spawn single or multiple tiles depending on what is in queue
            if (isSingleTile){
                getSpawnCoords(out rowCoord, out colCoord);
                setupNewTile(rowCoord, colCoord);

                if (tileNum >= singleTileMax){
                    isSingleTile = false;
                }
            }else{
                StartCoroutine(spawnTileSlider(Random.Range(tileSlideSizeMin,tileSlideSizeMax+1)));
                // Wait for tile slide to create it's tiles
                while (isMultipleTile){
                    yield return new WaitForSeconds(0.01f);
                }
            }

        }
    }

    // Spawn a tiles using the same constant interval
    IEnumerator spawnTileSlider(int tileSlideSize){
        // 2d array to hold coords that the tiles for the slider will spawn with
        List<int[]> tempList = new List<int[]>();
        List<int> directionList = new List<int>();
        int[] tempArray = new int[2];
        int rowCoord, colCoord, rowAdd, colAdd, tempRow, tempCol, dir;

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

        rowAdd = 0;
        colAdd = 0;
        // Create tiles in order of direction list
        for (int i = 0; i < tileSlideSize; i++){
            yield return new WaitForSeconds(sliderTileDelay);
            if (!gamePlaying){break;}
            rowCoord += rowAdd;
            colCoord += colAdd;
            setupNewTile(rowCoord, colCoord);

            if (i < tileSlideSize-1)
            {
                getDirectionModifiers(directionList[i], out rowAdd, out colAdd);
            }
            
        }

        isMultipleTile = false;
        
    }

    private IEnumerator blinkingLossTile(GameObject tile, bool isTimedout)
    {
        SpriteRenderer tileSprite = tile.GetComponent<SpriteRenderer>();
        Color currentColor = tileSprite.color;

        if (!isTimedout)
        {
            for (int i = 0; i < 4; i++)
            {
                tileSprite.color = currentColor;
                yield return new WaitForSeconds(0.2f);
                changeTileColor(tile, 1);
                yield return new WaitForSeconds(0.2f);
            }
        }else
        {
            for (int i = 0; i < 4; i++)
            {
                changeTileColor(tile, 0);
                yield return new WaitForSeconds(0.2f);
                tileSprite.color = currentColor;
                yield return new WaitForSeconds(0.2f);
            }
        }

    }

    // Given a row and col, return a random possible direction the next tile could be placed
    private int chooseRandomDirection(int rowBase, int colBase){
        List<int> dirList = new List<int>();
        int tmpRow, tmpCol, rowAdd, colAdd;
        
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
        int rowCoord, colCoord;
        getSpawnCoords(out rowCoord, out colCoord);
        currentColorSet = Random.Range(2,colorSetList.Count);
        changeTileColor(gridArray[rowCoord,colCoord], currentColorSet);
        tileNum += 1;

        gridArray[rowCoord,colCoord].transform.GetChild (0).gameObject.GetComponent<TextMeshPro>().SetText("" + tileNum);

        queuedTileOrder.Add(gridArray[rowCoord,colCoord]);
    }

    // Creation of adding a tile to the grid
    private void setupNewTile(int row, int col)
    {

        // Play sound
        audioManager.playSound("creation");

        // Update game array
        gameArray[row, col] = 1;

        // Get number for the tile and update the tile
        tileNum += 1;

        int oldColorSet = currentColorSet;
        if (tileNum == 1){
            while (currentColorSet == oldColorSet){
                currentColorSet = Random.Range(2,colorSetList.Count);
            }
        }

        // Update sprite
        changeTileColor(gridArray[row,col], currentColorSet);

        gridArray[row,col].transform.GetChild (0).gameObject.GetComponent<TextMeshPro>().SetText("" + tileNum);

        // Set up timers & append to queues
        IEnumerator tileTimer = tileUptimeFinished(gridArray[row,col]);
        StartCoroutine(tileTimer);
        queuedTileTimer.Add(tileTimer);
        queuedTileOrder.Add(gridArray[row,col]);
    }

    // Timer for each tile, lose game if timer ends
    IEnumerator tileUptimeFinished(GameObject tile)
    {
        int colorSet = currentColorSet;
        yield return new WaitForSeconds(tileUptime/2);

        tile.GetComponent<FadeColorScript>().StartFade(tileUptime/2, colorSetList[colorSet], colorSetList[1]);

        yield return new WaitForSeconds(tileUptime/2);

        if (gamePlaying)
        {
            gameOver(tile, true);
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
                break;
            
        }
    }

    // Set up color list
    private void setupColorSets()
    {
        colorSetList = new List<string>();

        // Base color
        colorSetList.Add("#45FFE3");
        // Loss Color
        colorSetList.Add("#202020");

        // Random colors
        colorSetList.Add("#FFB100");
        colorSetList.Add("#FF4C93");
        colorSetList.Add("#F134FF");
        colorSetList.Add("#7467FF");
        colorSetList.Add("#7CFF85");
        colorSetList.Add("#F2FF6C");
        colorSetList.Add("#FF2D3D");
    }

    // Change a tile's sprite
    private void changeTileColor(GameObject obj, int chosen)
    {
        Color newColor;

        ColorUtility.TryParseHtmlString(colorSetList[chosen], out newColor);

        obj.GetComponent<SpriteRenderer>().color = newColor;
    }

    // Check if score is a new best for either normal or hard mode
    private bool checkNewBest()
    {
        if (currentDifficulty == "normal" && score > PlayerPrefs.GetInt("normalBestScore", 0))
        {
            PlayerPrefs.SetInt("normalBestScore", score);
            CloudOnceServices.instance.SubmitScoreToLeaderboard(score, true);
            menuScript.toggleNewBestTime(true);
            return true;
        }else if (currentDifficulty == "hard" && score > PlayerPrefs.GetInt("hardBestScore", 0))
        {
            PlayerPrefs.SetInt("hardBestScore", score);
            CloudOnceServices.instance.SubmitScoreToLeaderboard(score, false);
            menuScript.toggleNewBestTime(true);
            return true;
        }
        
        return false;
    }

    // Update score text on screen
    private void updateScoreText(){
        scoreText.SetText("" + score);
    }

    // Get tile that was clicked
    public void updateTileClicked(GameObject tile)
    {
        tileClicked = true;
        currentClickedTile = tile;
    }
    
}
