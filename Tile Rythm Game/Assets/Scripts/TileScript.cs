using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileScript : MonoBehaviour
{
    private GridManager gridManager;

    void Start()
    {
        gridManager = GameObject.FindObjectOfType<GridManager>();
    }

    void OnMouseDown(){
        passTile();
    }

    void OnMouseEnter(){
        if (Input.GetMouseButton(0)){
            passTile();
        }
    }

    void passTile(){
        gridManager.updateTileClicked(gameObject);
    }
}
