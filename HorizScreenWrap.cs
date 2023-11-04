using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizScreenWrap : MonoBehaviour
{
    private Collider2D ObjectBounds;

    private int spawnMultiplyer;
    private bool enemy;
    private bool readyToWrap;

    void Awake()
    {
        //Make sure to get the correct collider for the wrap to work properly, and check what type of object this is
        if (!this.gameObject.name.StartsWith("Tough Robot"))
        {
            ObjectBounds = this.GetComponent<Collider2D>();
        }
        else
        {
            gameObject.TryGetComponent<ToughEnemyController>(out ToughEnemyController toughScript);
            ObjectBounds = toughScript.hitbox;
        }

        if (this.gameObject.tag == "Enemy")
        {
            enemy = true;
        }
        else
        {
            enemy = false;
        }

        readyToWrap = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //Check to make sure we left the bounds
        if (collision.gameObject.CompareTag("Bounds") & !ObjectBounds.IsTouching(collision) & readyToWrap)
        {
            BoxCollider2D BoundaryBounds = collision.gameObject.GetComponent<BoxCollider2D>();
            float offset;

            //Check to see if we left the bounds on the right (calculation is slightly different left vs. right)
            if (transform.position.x < 0)
            {
                //find the offset from the edge of this object to the edge of the bounds
                offset = transform.position.x + ObjectBounds.bounds.size.x + BoundaryBounds.bounds.size.x;
                spawnMultiplyer = 1;
                
            }
            else
            {
                //find the offset from the edge of this object to the edge of the bounds
                offset = transform.position.x - ObjectBounds.bounds.size.x - BoundaryBounds.size.x;
                spawnMultiplyer = -1;
            }
            
            if (enemy)
            {
                //Find out how many enemies are on the floor in total and how many tough enemies
                int floorCount = 0;
                int toughFloorCount = 0;

                foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
                {
                    if (enemy.gameObject.transform.position.y < 1)
                    {
                        floorCount++;
                        if (enemy.gameObject.name.StartsWith("Tough Robot"))
                            toughFloorCount++;
                    }
                }

                //If the wrap would cause enemies to overlap, spawn them at the top spawn point instead
                if (Physics2D.OverlapBox(new Vector2(offset, ObjectBounds.transform.position.y + 0.1f), new Vector2(ObjectBounds.bounds.size.x/2, ObjectBounds.bounds.size.y), 0.0f) != null)
                {
                    spawnToTop();
                }
                //If there are too many enemies on the floor in general or there is more then one tough enemy, spawn to top
                else if (gameObject.transform.position.y < 1)
                {
                    if (floorCount >= 6 | (toughFloorCount > 1 & gameObject.gameObject.name.StartsWith("Tough Robot")))
                        spawnToTop();
                    else
                        gameObject.transform.position = new Vector3(offset, transform.position.y, transform.position.z);
                }
                else
                {
                    gameObject.transform.position = new Vector3(offset, transform.position.y, transform.position.z);
                }
            }
            else
            {
                gameObject.transform.position = new Vector3(offset, transform.position.y, transform.position.z);
            }
        }
    }

    //Function to spawn enemies to top and set their grounded property fo false, regardless of if it was a weak or tough enemy
    private void spawnToTop()
    {
        gameObject.transform.position = new Vector3(16f * spawnMultiplyer, 18f, 5.5f);
        gameObject.TryGetComponent<WeakEnemyController>(out WeakEnemyController weakScript);
        gameObject.TryGetComponent<ToughEnemyController>(out ToughEnemyController toughScript);
        if (weakScript != null)
        {
            weakScript.grounded = false;
        }
        else
        {
            toughScript.grounded = false;
        }
    }
}
