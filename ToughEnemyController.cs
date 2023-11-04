using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToughEnemyController : MonoBehaviour
{
    private GameObject player;
    private SpawnManager spawnManager;

    private Rigidbody2D rb;
    public Collider2D weakPoint;
    public Collider2D hitbox;

    private bool facingRight;
    private bool spottedPlayer;
    private float previousPosition;
    public float deltaPosition;
    public bool turning, grounded;
    public float speed;
    public float turnTime;
    
    void Start()
    {
        player = GameObject.Find("Player").gameObject;
        spawnManager = GameObject.FindObjectOfType<SpawnManager>();

        rb = GetComponent<Rigidbody2D>();

        facingRight = true;
        spottedPlayer = true;

        StartCoroutine(SpawnInvincability());
    }

    void FixedUpdate()
    {
        //If the tough enemy is not already turning and has spotted the player, face them
        if (!turning & spottedPlayer) FacePlayer();

        //If grounded move, otherwise drift
        if (grounded)
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }
        else rb.velocity = new Vector2(0, rb.velocity.y);

        //If player is in line of sight (detected using function), they've been spotted
        spottedPlayer = LineOfSight();

        //Make sure the tough enemy sprite is facing the right way
        if (facingRight & transform.localScale.x < 0)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
        else if (!facingRight & transform.localScale.x > 0)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }
    }

    //Start the coroutine to face the player if necessary
    private void FacePlayer()
    {
        if (facingRight)
        {
            if (player.transform.position.x < this.transform.position.x)
            {
                facingRight = false;
                StartCoroutine(Turn());
            }
        }
        else
        {
            if (player.transform.position.x > this.transform.position.x)
            {
                facingRight = true;
                StartCoroutine(Turn());
            }   
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //If the enemy collided with the top of a ground object, declare them as grounded
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (transform.position.y - hitbox.bounds.extents.y > collision.gameObject.transform.position.y)
            {
                grounded = true;
            }
        }
        //If collided with an enemy while not tracking the player, see if the tough enemy can turn around
        else if (collision.gameObject.CompareTag("Enemy") & !spottedPlayer)
        {
            if (collision.gameObject.transform.position.x < this.gameObject.transform.position.x & !facingRight)
            {
                AttemptTurn();
            }
            else if (collision.gameObject.transform.position.x > this.gameObject.transform.position.x & facingRight)
            {
                AttemptTurn();
            }
        }
    }

    //Same as OnCollisionEnter2D, just making absolutely sure that these trigger if for some reason the first function didn't catch it
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (transform.position.y - hitbox.bounds.extents.y > collision.gameObject.transform.position.y)
            {
                grounded = true;
            }
        }
        else if (collision.gameObject.CompareTag("Enemy") & !spottedPlayer)
        {
            if (collision.gameObject.transform.position.x < this.gameObject.transform.position.x & !facingRight)
            {
                AttemptTurn();
            }
            else if (collision.gameObject.transform.position.x > this.gameObject.transform.position.x & facingRight)
            {
                AttemptTurn();
            }
        }
    }

    //Unground the enemy if necessary
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }

    //Assign the appropriate number of points upon death
    private void OnDestroy()
    {
        if (!spawnManager.gameOver)
            spawnManager.points += 500;
    }

    //Check to make sure the tough enemy is not already turning before turning
    public void AttemptTurn()
    {
        if (!turning)
        {
            facingRight = !facingRight;
            StartCoroutine(Turn());
        }
    }

    //Turn around, waiting a bit before being able to move and turn again
    private IEnumerator Turn()
    {
        turning = true;
        float tempSpeed = -speed;
        speed = 0;
        yield return new WaitForSeconds(turnTime);
        speed = tempSpeed;
        yield return new WaitForSeconds(0.1f);
        turning = false;
    }

    //Check if the player is within the lign of sight of the enemy (if they are approximately on the same plane horizontally
    private bool LineOfSight()
    {
        if (player.transform.position.y - 2 < this.transform.position.y & player.transform.position.y + 2 > this.transform.position.y)
            return true;
        else
            return false;
    }

    //Make the tough enemy not hurt the player briefly on spawn
    private IEnumerator SpawnInvincability()
    {
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), hitbox, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), hitbox, false);
    }
}
