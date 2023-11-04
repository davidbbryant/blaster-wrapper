using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    public GameObject weakEnemy;
    public GameObject strongEnemy;
    public GameObject player;
    public GameObject foreground;
    private GameObject[] spawnPoints;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI pointText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI scoreText;
    private bool spawning;
    private bool waveGap;
    public bool gameOver;
    private int enemyCount;
    private int waveNumber;
    private int waveCap;
    private int strongEnemyCap;
    public int spawnCount;
    public float spawnCooldown, waveTimeGap;
    public float scoreSpeed;
    private float timer;
    public int points;

    void Start()
    {
        spawnPoints = GameObject.FindGameObjectsWithTag("Spawn");
        spawning = false;
        waveGap = true;
        gameOver = false;
        enemyCount = 0;
        waveNumber = 1;
        waveCap = 10;
        strongEnemyCap = 1;
        timer = 0;
        points = 0;
    }

    void Update()
    {
        //If the game is not over, see if either the player is dead or if more enemies need to be spawned
        if (!gameOver)
        {
            if (player.GetComponent<PlayerController>().health <= 0)
            {
                gameOver = true;
                StopAllCoroutines();
                StartCoroutine(DisplayScore("Failure!"));
            }
            else
            {
                //See if the wave is over and determine if we have reached the last wave
                enemyCount = FindObjectsOfType<WeakEnemyController>().Length + FindObjectsOfType<ToughEnemyController>().Length;
                if (enemyCount == 0)
                {
                    if (waveNumber > waveCap & !spawning)
                    {
                        gameOver = true;
                        StartCoroutine(DisplayScore("Victory!"));
                    }
                    else if (!spawning) StartCoroutine(SpawnWave());
                }

                //Update the timer and score, but the only update the timer when a wave is spawning
                if (!waveGap) updateTimer(ref timer);
                pointText.text = "Points\n" + points;
            }
        }
        //Reset the game when it is over and the enter key is pressed
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private IEnumerator SpawnWave()
    {
        spawning = true;
        waveGap = true;

        waveText.text = "Wave " + waveNumber;
        waveText.gameObject.SetActive(true);

        //Destroy all projectiles beforee the next wave starts
        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("Projectile")) Destroy(projectile);

        yield return new WaitForSeconds(waveTimeGap);
        waveText.gameObject.SetActive(false);
        waveGap = false;

        //Actually spawn the wave, with enemy types and positions randomized, making sure we are not spawning on top of another enemy
        for (int i = 0; i < spawnCount; i++)
        {
            spawnShuffle();
            GameObject spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (Physics2D.OverlapBox(new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), new Vector2(3.25f, 1f), 0.0f) == null)
            {
                spawnEnemy(spawnPoint);
                yield return new WaitForSeconds(spawnCooldown);
            }
            //If the spawn point has an enemy in it already, shuffle around until an open spawnpoint is found
            else
            {
                int f = 0;
                bool spawnFound = false;
                while (f < spawnPoints.Length & !spawnFound)
                {
                    spawnPoint = spawnPoints[f];
                    if (Physics2D.OverlapBox(new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), new Vector2(3.25f, 1f), 0.0f) == null)
                    {
                        spawnEnemy(spawnPoint);
                        spawnFound = true;
                        yield return new WaitForSeconds(spawnCooldown);
                    }
                    f++;
                }

                if (!spawnFound)
                {
                    yield return new WaitForSeconds(0.1f);
                }   
            }

            spawnPoint.transform.GetChild(0).gameObject.SetActive(false);
        }

        waveNumber++;
        if (waveNumber == 4 | waveNumber == 7) strongEnemyCap++;
        spawnCount += 4;
        spawnCooldown -= 0.15f;
        spawning = false;
    }

    //Long but fairly simple function to wipe the game and then display the score sequentially
    private IEnumerator DisplayScore(string endText)
    {
        player.GetComponent<PlayerController>().health = 100;

        foreach (GameObject projectile in GameObject.FindGameObjectsWithTag("Projectile")) Destroy(projectile);
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(enemy);
        foreach (GameObject spawnPoint in spawnPoints) spawnPoint.transform.GetChild(0).gameObject.SetActive(false);

        waveNumber--;
        updateTimer(ref timer);
        pointText.text = "Points\n" + points;
        waveText.text = endText;
        waveText.gameObject.SetActive(true);

        yield return new WaitForSeconds(5);

        waveText.gameObject.SetActive(false);
        pointText.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);

        foreground.SetActive(true);
        if (waveNumber <= waveCap)
            scoreText.text += "Got to Wave: " + waveNumber;
        else
            scoreText.text += "Cleared all Waves!";
        scoreText.gameObject.SetActive(true);
        yield return new WaitForSeconds(scoreSpeed);

        scoreText.text += "\nPoints: " + points;
        yield return new WaitForSeconds(scoreSpeed);

        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        scoreText.text += "\nTime: " + minutes + ":" + seconds.ToString("D2");
        yield return new WaitForSeconds(scoreSpeed);

        scoreText.text += "\nFinal Score = \nPoints - Time (in Seconds)";
        yield return new WaitForSeconds(scoreSpeed);

        int finalScore = points - Mathf.FloorToInt(timer);
        scoreText.text += "\n\nFinal Score: " + finalScore;
        yield return new WaitForSeconds(scoreSpeed * 2);

        scoreText.text += "\n\nPress Enter to Continue";
    }

    //Found help online for this function, it shuffles around the order of the spawn points in the spawnPoints array
    //so that when we loop through them sequentially they will be used in a random order
    public void spawnShuffle()
    {
        GameObject tempObj;

        for (int i = 0; i < spawnPoints.Length - 1; i++)
        {
            int random = Random.Range(i, spawnPoints.Length);
            tempObj = spawnPoints[random];
            spawnPoints[random] = spawnPoints[i];
            spawnPoints[i] = tempObj;
        }
    }

    //Code for spawning enemies that tries to prevent trapping the player by not spawning in too many strong enemies in akward places
    public void spawnEnemy(GameObject spawnPoint)
    {
        spawnPoint.transform.GetChild(0).gameObject.SetActive(true);

        int random = Random.Range(0, 2);

        int toughFloorCount = 0;
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy.gameObject.transform.position.y < 1 & enemy.gameObject.name.Contains("Tough Robot"))
                toughFloorCount++;
        }

        //Only spawn another strong enemy if we haven't reached the cap for the current wave.
        //And if we wish to spawn on the floor but there is already a strong enemy on the floor, spawn a weak enemy instead
        if (random == 0 & FindObjectsOfType<ToughEnemyController>().Length < strongEnemyCap)
        {
            if (spawnPoint.transform.position.y > 1)
            {
                Instantiate(strongEnemy, new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), strongEnemy.transform.rotation);
            }
            else if (toughFloorCount == 0)
            {
                Instantiate(strongEnemy, new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), strongEnemy.transform.rotation);
            }
            else
            {
                Instantiate(weakEnemy, new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), weakEnemy.transform.rotation);
            }
            
        }
        else
        {
            if (spawnPoint.transform.position.x >= 0)
            {
                GameObject tempObj = Instantiate(weakEnemy, new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), weakEnemy.transform.rotation);
                tempObj.GetComponent<WeakEnemyController>().speed *= -1;
            }
            else Instantiate(weakEnemy, new Vector2(spawnPoint.transform.position.x, spawnPoint.transform.position.y), weakEnemy.transform.rotation);
        }
    }
    
    //Simple function to update the timer and display it
    private void updateTimer(ref float currentTime)
    {
        currentTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timeText.text = "Time\n" + minutes + ":" + seconds.ToString("D2");
    }
}
