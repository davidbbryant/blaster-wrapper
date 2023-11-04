using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeakEnemyController : MonoBehaviour
{
    private GameObject player;
    private SpawnManager spawnManager;

    private Rigidbody2D rb;
    private Collider2D hitbox;

    public float speed;
    public bool grounded;
    public bool facingRight;
    public bool readyToTurn;


    void Start()
    {
        player = GameObject.Find("Player").gameObject;
        spawnManager = GameObject.FindObjectOfType<SpawnManager>();

        rb = GetComponent<Rigidbody2D>();
        hitbox = gameObject.GetComponent<Collider2D>();

        StartCoroutine(SpawnInvincability());
    }

    void FixedUpdate()
    {
        //If the enemy is grounded then keep speeding forwards, otherwise simply drift until grounded again
        if (grounded) rb.velocity = new Vector2(speed, rb.velocity.y);
        else rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y);

        if (speed > 0) facingRight = true;
        else facingRight = false;

        //Make sure the sprite is facing the correct direction
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
        //If collided with either a wall or another enemy turn around
        else if ((collision.gameObject.CompareTag("Enemy") | collision.gameObject.CompareTag("Wall")) & readyToTurn)
        {
            if (collision.gameObject.transform.position.x > this.gameObject.transform.position.x)
            {
                if (speed > 0) speed *= -1;
            }
            else
            {
                if (speed < 0) speed *= -1;
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
        else if (collision.gameObject.CompareTag("Enemy") | collision.gameObject.CompareTag("Wall"))
        {
            if (collision.gameObject.transform.position.x > this.gameObject.transform.position.x)
            {
                if (speed > 0) speed *= -1;
            }
            else
            {
                if (speed < 0) speed *= -1;
            }
        }
    }

    //Unground the enemy if necessary
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            grounded = false;
        }
    }

    //Assign the appropriate number of points upon death
    private void OnDestroy()
    {
        if (!spawnManager.gameOver)
            spawnManager.points += 100;
    }

    //Make the weak enemy not hurt the player briefly on spawn
    private IEnumerator SpawnInvincability()
    {
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), hitbox, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), hitbox, false);
    }
}
