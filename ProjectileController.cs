using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private GameObject player;
    public GameObject explosion;

    private Rigidbody2D rb;
    Collider2D hitbox;

    public float speed;
    public float goingRight;

    private void Awake()
    {
        player = GameObject.Find("Player").gameObject;
        hitbox = GetComponent<Collider2D>();
        StartCoroutine(PlayerInvincability());
    }

    void Start()
    {
        //Make sure the projectile is going the right direction relative to the way the player is facing
        speed *= goingRight;
        if (goingRight < 0)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x *= -1;
            transform.localScale = currentScale;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector2.right * speed;

        //If the projectile is spawned out of bounds, wrap it around to the other side
        Collider2D outOfBounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<Collider2D>();
        if (rb.position.x > 0 & rb.position.x - hitbox.bounds.extents.x > outOfBounds.bounds.extents.x)
            rb.transform.position = new Vector2(-outOfBounds.bounds.extents.x, rb.position.y);
        else if (rb.position.x < 0 & rb.position.x + hitbox.bounds.extents.x < -outOfBounds.bounds.extents.x)
            rb.transform.position = new Vector2(outOfBounds.bounds.extents.x, rb.position.y);
    }

    //Check when the projectile collides with something and take the appropriate action (which usually includes spawning in an explosion), eventually destroying the projectile
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.name.StartsWith("Tough Robot"))
            {
                collision.TryGetComponent<ToughEnemyController>(out ToughEnemyController toughScript);
                if (collision == toughScript.weakPoint)
                {
                    Instantiate(explosion, new Vector2(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y), explosion.transform.rotation);
                    Destroy(collision.gameObject);
                }
            }
            else
            {
                Instantiate(explosion, new Vector2(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y), explosion.transform.rotation);
                Destroy(collision.gameObject);
            }
        }
        else if (collision.gameObject.CompareTag("Projectile"))
        {
            Instantiate(explosion, new Vector2(collision.gameObject.transform.position.x, collision.gameObject.transform.position.y), explosion.transform.rotation);
            Destroy(collision.gameObject);
        }

        //Final catch all for if the projectile doesn't hit any known object, just assume it is terrain and destroy the projectile
        if (!collision.gameObject.CompareTag("Player") & !collision.gameObject.CompareTag("Bounds"))
        {
            Destroy(gameObject);
        }
    }

    //Ignore collision with the player when initially spawning to avoid hitting the player on spawn
    private IEnumerator PlayerInvincability()
    {
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), hitbox, true);
        yield return new WaitForSeconds(0.5f);
        Physics2D.IgnoreCollision(player.GetComponent<Collider2D>(), hitbox, false);
    }
}
