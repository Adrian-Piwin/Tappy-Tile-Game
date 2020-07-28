using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private int rows = 5;
    [SerializeField]
    private int cols = 5;
    [SerializeField]
    private float tileSize = 1;
    [SerializeField]
    private Text scoreText;
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
    private bool firstTileClicked;
    static float spawnInterval = 2.0f;
    private float tileUptime = 3.0f;
    private int score = 0;
    private int tileNum;
    private int tileNumMax = 9;
    private int slideTileProbability = 3;
    private string[] coords;

    IEnumerator tileSpawnTimer;
    IEnumerator tileTimer;

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
        if (tileClicked && gamePlaying)
        {
            // Check if tile matches the current queued tile 
            if (queuedTileOrder.Count != 0){
                if (currentClickedTile == queuedTileOrder[0]){
                    queuedTileOrder.RemoveAt(0);
                    
                    // Update game array
                    coords = (currentClickedTile.name).Split(',');
                    gameArray[int.Parse(coords[0]), int.Parse(coords[1])] = 0;

                    changeTileSprite(currentClickedTile, 0);
                    currentClickedTile.transform.GetChild (0).gameObject.GetComponent<TextMesh>().text = "";
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
        //queuedTileOrder.Add(gridArray[0,1]);
        //changeTileSprite(gridArray[0,1], 1);
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
                setupNewTile(rowCoord, colCoord, 1);
            }else{
                spawnTileSlider(Random.Range(2,4));
            }
            
        }
    }

    // Spawn a tiles using the same constant interval
    private void spawnTileSlider(int tileSlideSize){
        // 2d array to hold coords that the tiles for the slider will spawn with
        List<int> directionList = new List<int>();
        int rowAdd, colAdd, tempRow, tempCol;

        while (true)
        {
            getSpawnCoords(out rowCoord, out colCoord);
            
            for(int i = 0; i < 4; i++)
            {
                getDirectionModifiers(i, out rowAdd, out colAdd);

                // Check if spots avaiable in all directions
                for (int ind = 1; ind < tileSlideSize; ind++)
                {
                    tempRow = rowCoord+(rowAdd*ind);
                    tempCol = colCoord+(colAdd*ind);

                    // Check if out of range, or if spot already occupied
                    if (tempRow < 0 || tempRow >= rows || tempCol < 0 || tempCol >= cols)
                    {
                        break;
                    }else if (gameArray[tempRow, tempCol] == 1)
                    {
                        break;
                    }
                    

                    // If spot is available, add direction to list
                    if (ind == tileSlideSize-1)
                    {
                        directionList.Add(i);
                    }
                }
            }

            // Continue if at least 1 direction is available
            if (directionList.Count > 0){
                break;
            }

        }
        
        getDirectionModifiers(directionList[Random.Range(0,directionList.Count)], out rowAdd, out colAdd);

        for (int i = 0; i < tileSlideSize; i++){
            setupNewTile(rowCoord+(rowAdd*i), colCoord+(colAdd*i), 1);
        }
        
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
    private void setupNewTile(int row, int col, int spriteIndex)
    {
        changeTileSprite(gridArray[row,col], spriteIndex);
        tileNum += 1;
        if (tileNum >= tileNumMax){
            tileNum = 1;
        }
        gridArray[row,col].transform.GetChild (0).gameObject.GetComponent<TextMesh>().text = ("" + tileNum);

        tileTimer = tileUptimeFinished(gridArray[row,col]);
        StartCoroutine(tileTimer);
        queuedTileOrder.Add(gridArray[row,col]);
        queuedTileTimer.Add(tileTimer);
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
                gameArray[x, y] = 1;
                break;
            }
        }
    }

    // Out the x and y addition depending on direction
    private void getDirectionModifiers(int dir, out int rowMod, out int colMod){
        if (dir==0){ // right
            rowMod=1;
            colMod=0;
        }else if (dir==1){ // left
            rowMod=-1;
            colMod=0;
        }else if (dir==2){ // up
            rowMod=0;
            colMod=1;
        }else if (dir==3){ // down
            rowMod=0;
            colMod=-1;
        }else{
            rowMod=0;
            colMod=0;
            Debug.Log("Error: 1");
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
