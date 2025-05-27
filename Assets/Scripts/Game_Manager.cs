using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Game_Manager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject previousTiles;
    public GameObject prefabTiles;
    public GameObject currentTiles;


    private bool start = true;
    private bool gameOver = false;
    
    public int score = 1;
    public float moveSpeed = 2f;
    private bool movingOnX = true;
    public TextMeshProUGUI scoreText;
    public GameObject restartButton;
    public GameObject startButton;
    public GameObject score_obj;


    private float tileStartTime;
    //private Queue<GameObject> stackedTiles = new Queue<GameObject>();
    private int maxTiles = 5;
    public float speed = 50f;
    void Start()
    {
        if (scoreText != null)
        {
            Debug.Log(scoreText.text + " is Available!!");
        }
        else {
            Debug.Log("text Unavailable!");
        }
        scoreText.text = score.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver && currentTiles != null)
        {
            float moveAmount = (speed * Time.deltaTime)*3;

            //Oscillation Scipt 
            if (movingOnX)
            {
                currentTiles.transform.position += new Vector3(Mathf.Sin(Time.time * speed) * Time.deltaTime * 15f, 0, 0);
            }
            else
            {
                currentTiles.transform.position += new Vector3(0, 0, Mathf.Sin(Time.time * speed) * Time.deltaTime * 15f);
            }
        }

        if (Input.GetMouseButtonDown(0) && !gameOver)
        {
            DealWithPlayerClicks();
        }
    }

    public void RestartLevel() {
        SceneManager.LoadScene(0);
    }

    public void game_start() {
        score_obj.SetActive(start);
        start = false;
        startButton.SetActive(start);
    }

    public void DealWithPlayerClicks()
    {
        if (currentTiles == null)
        {
            Vector3 spawnPosition;

            if (movingOnX)
            {
                // Start far from the left (X) and move toward center
                spawnPosition = new Vector3(
                    previousTiles.transform.position.x , // or +2f to come from the right
                    previousTiles.transform.position.y + 1f,
                    previousTiles.transform.position.z - 5f
                );
            }
            else
            {
                // Start far from the back (Z) and move toward center
                spawnPosition = new Vector3(
                    previousTiles.transform.position.x - 5f,
                    previousTiles.transform.position.y + 1f,
                    previousTiles.transform.position.z  // or +2f to come from the front
                );
            }

            currentTiles = Instantiate(prefabTiles, spawnPosition, previousTiles.transform.rotation);
            currentTiles.transform.localScale = previousTiles.transform.localScale;
            //stackedTiles.Enqueue(currentTiles);

            //if (stackedTiles.Count > this.maxTiles)
            //{
            //    Debug.Log(stackedTiles.Count);
            //    GameObject bottomTile = stackedTiles.Dequeue(); // Removes the oldest tile
            //    Destroy(bottomTile);
            //}

            movingOnX = !movingOnX;
            return;
        }

        float delta = 0f;
        float newSize = 0f;
        float fallingBlockSize = 0f;
        Vector3 fallingBlockPosition = Vector3.zero;

        Vector3 currentPos = currentTiles.transform.position;
        Vector3 previousPos = previousTiles.transform.position;
        Vector3 currentScale = currentTiles.transform.localScale;

        if (movingOnX)
        {
            delta = currentPos.x - previousPos.x;
            newSize = previousTiles.transform.localScale.x - Mathf.Abs(delta);

            if (newSize <= 0f)
            {
                restartButton.SetActive(true);
                gameOver = true;
                return;
            }

            fallingBlockSize = currentScale.x - newSize;

            // Adjust current tile size and position
            currentTiles.transform.localScale = new Vector3(newSize, currentScale.y, currentScale.z);
            currentTiles.transform.position = new Vector3(previousPos.x + (delta / 2f), currentPos.y, currentPos.z);

            // Determine position of falling part
            float direction = delta > 0 ? 1 : -1;
            fallingBlockPosition = new Vector3(
                currentTiles.transform.position.x + (newSize / 2f + fallingBlockSize / 2f) * direction,
                currentPos.y,
                currentPos.z
            );

            SpawnFallingBlock(new Vector3(fallingBlockSize, currentScale.y, currentScale.z), fallingBlockPosition);
        }
        else
        {
            delta = currentPos.z - previousPos.z;
            newSize = previousTiles.transform.localScale.z - Mathf.Abs(delta);

            if (newSize <= 0f)
            {
                restartButton.SetActive(true);
                gameOver = true;
                return;
            }

            fallingBlockSize = currentScale.z - newSize;

            currentTiles.transform.localScale = new Vector3(currentScale.x, currentScale.y, newSize);
            currentTiles.transform.position = new Vector3(currentPos.x, currentPos.y, previousPos.z + (delta / 2f));

            float direction = delta > 0 ? 1 : -1;
            fallingBlockPosition = new Vector3(
                currentPos.x,
                currentPos.y,
                currentTiles.transform.position.z + (newSize / 2f + fallingBlockSize / 2f) * direction
            );

            SpawnFallingBlock(new Vector3(currentScale.x, currentScale.y, fallingBlockSize), fallingBlockPosition);
        }

        previousTiles = currentTiles;

        // Instantiate new tile with updated size
        currentTiles = Instantiate(prefabTiles, previousTiles.transform.position + Vector3.up, previousTiles.transform.rotation);
        currentTiles.transform.localScale = previousTiles.transform.localScale;

        movingOnX = !movingOnX;

        Camera.main.transform.position += Vector3.up;
        score++;
        scoreText.text = score.ToString();
    }

    private void SpawnFallingBlock(Vector3 scale, Vector3 position)
    {
        GameObject fallingBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallingBlock.transform.localScale = scale;
        fallingBlock.transform.position = position;
        fallingBlock.AddComponent<Rigidbody>(); // Makes it fall
        fallingBlock.GetComponent<Renderer>().material.color = currentTiles.GetComponent<Renderer>().material.color;
        Destroy(fallingBlock, 3f); // Cleanup after 3 seconds
    }


}
