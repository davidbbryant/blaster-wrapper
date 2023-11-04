using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject projectile;
    public GameObject explosion;

    public Animator animator;
    private Rigidbody2D PlayerRb;
    private Collider2D PlayerCollider;
    private SpriteRenderer PlayerSprite;

    public float groundSpeed;
    public float airAcceleration;
    public float jumpForce;
    public float horizontalKnockback;
    public float verticalKnockback;
    public float invunerability;
    public float cooldown;
    public float offset, offsetHeight;
    public bool jump, shoot, grounded, readyToFire, airStun, facingRight;
    public int health;
    
    void Start()
    {
        PlayerRb = GetComponent<Rigidbody2D>();
        PlayerCollider = GetComponent<Collider2D>();
        PlayerSprite = GetComponent<SpriteRenderer>();

        jumpForce *= 100;
        jump = false;
        grounded = false;
        airStun = false;
        readyToFire = true;
        facingRight = true;
        health = 3;
    }

    // Update is called once per frame
    private void Update()
    {
        //Check if the spacebar was pressed
        if (Input.GetKeyDown(KeyCode.Space) & grounded) jump = true;

        //Check if the left mouse button was pressed
        if (Input.GetKeyDown(KeyCode.Mouse0) & readyToFire & !airStun) shoot = true;
    }

    // Update is called once per frame (fixed)
    void FixedUpdate()
    {
        HorizontalMovement();

        //If the spacebar was pressed, make the player jump
        if (jump)
        {
            PlayerRb.AddForce(Vector2.up * jumpForce);
            jump = false;
        }

        //Player shooting
        if (shoot)
        {
            StartCoroutine(Fire());
        }

        //Set the animator variables correctly
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Air Stun", airStun);
        animator.SetFloat("Vertical Speed", PlayerRb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Check to see if the player collided with ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            //Make sure the player collided with the top of the ground, and if so set grounded to true and airStun to false
            if (transform.position.y - gameObject.GetComponent<Collider2D>().bounds.extents.y + 0.1f >= collision.gameObject.transform.position.y + collision.gameObject.GetComponent<Collider2D>().bounds.extents.y)
            {
                grounded = true;
                airStun = false;
            }
        }
        //If player collided with an enemy, knock them back, give them stun/invicability, and have them take damage
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.transform.position.x < PlayerRb.position.x)
            {
                PlayerRb.velocity = new Vector2(horizontalKnockback, verticalKnockback);
            }
            else
            {
                PlayerRb.velocity = new Vector2(-horizontalKnockback, verticalKnockback);
            }

            grounded = false;
            airStun = true;
            StartCoroutine(HitStun());
        }
    }

    //Unground the player if necessary
    private void OnCollisionExit2D(Collision2D collision)
    {
        //Set grounded to false if the player is no longer touching the ground
        if (collision.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }

    //If the player is hit by a projectile (a trigger) then destroy the projectile and take appropriate knockback/damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            //Take knockback and damage
            if (collision.gameObject.transform.position.x < PlayerRb.position.x)
            {
                PlayerRb.velocity = new Vector2(horizontalKnockback, verticalKnockback);
            }
            else
            {
                PlayerRb.velocity = new Vector2(-horizontalKnockback, verticalKnockback);
            }

            Destroy(collision.gameObject);
            grounded = false;
            airStun = true;
            StartCoroutine(HitStun());
        }
    }

    //Function that controls the horizontal movement, which changes depending on whether grounded or in the air
    private void HorizontalMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0) facingRight = true;
        else if (horizontalInput < 0) facingRight = false;

        //Make sure that the sprite is facing the right direction
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

        //If the player is on the ground directly change velocity (twitchy movement)
        if (grounded) PlayerRb.velocity = new Vector2(horizontalInput * groundSpeed, PlayerRb.velocity.y);
        //If the player is not on the ground and not sttunned, accelerate them in the correct direction
        else if (!airStun)
        {
            float airVelocity = PlayerRb.velocity.x + airAcceleration * horizontalInput;

            //Cap the air movement speed to ground speed
            if (airVelocity > groundSpeed) airVelocity = groundSpeed;
            if (airVelocity < -groundSpeed) airVelocity = -groundSpeed;

            //Set the new capped velocity
            PlayerRb.velocity = new Vector2(airVelocity, PlayerRb.velocity.y);
        }

        //Update the animator is informed of the new speed
        animator.SetFloat("Speed", Mathf.Abs(PlayerRb.velocity.x));
    }

    //Routine to take damage and maintain hitstun
    private IEnumerator HitStun()
    {
        health--;
        //If the player is dead, destroy them
        if (health <= 0)
        {
            Instantiate(explosion, new Vector2(gameObject.transform.position.x, gameObject.transform.position.y), explosion.transform.rotation);
            this.gameObject.SetActive(false);
        }
        //If not, give them invincability for a bit
        else
        {
            PlayerSprite.color = new Color(PlayerSprite.color.r, PlayerSprite.color.g, PlayerSprite.color.b, 0.5f);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemies"), this.gameObject.layer, true);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Projectiles"), this.gameObject.layer, true);
            yield return new WaitForSeconds(invunerability);
            PlayerSprite.color = new Color(PlayerSprite.color.r, PlayerSprite.color.g, PlayerSprite.color.b, 1f);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Enemies"), this.gameObject.layer, false);
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Projectiles"), this.gameObject.layer, false);
        }
    }

    //Function to fire the projectile in the right direction and prevent the player from spamming them
    private IEnumerator Fire()
    {
        shoot = false;
        readyToFire = false;
        if (facingRight)
        {
            GameObject tempObj = Instantiate(projectile, new Vector2(PlayerRb.position.x + offset, PlayerRb.position.y + offsetHeight), projectile.transform.rotation);
            tempObj.GetComponent<ProjectileController>().goingRight = 1;
        }
        else
        {
            GameObject tempObj = Instantiate(projectile, new Vector2(PlayerRb.position.x - offset, PlayerRb.position.y + offsetHeight), projectile.transform.rotation);
            tempObj.GetComponent<ProjectileController>().goingRight = -1;
        }
        yield return new WaitForSeconds(cooldown);
        readyToFire = true;
    }
}
