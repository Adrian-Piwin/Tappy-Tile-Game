using System.Collections;
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

    private GameObject[,] gridArray;
    private List<GameObject> queuedTileOrder = new List<GameObject>();
    private GameObject currentClickedTile;

    public bool tileClicked = false;

    // Start is called before the first frame update
    void Start()
    {
        GenerateGrid();
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if (tileClicked)
        {
            if (currentClickedTile == queuedTileOrder[0]){
                Debug.Log("Yes");
            }else{
                Debug.Log("No");
            }

            tileClicked = false;
        }
    }

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

    private void StartGame()
    {
        queuedTileOrder.Add(gridArray[2,3]);
    }

    public void updateTileClicked(GameObject tile)
    {
        tileClicked = true;
        currentClickedTile = tile;
    }
}
