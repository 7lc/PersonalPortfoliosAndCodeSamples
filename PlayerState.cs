using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    // GameOver
    public GameOver go;
    public GameObject hearts;

    // Item State Property
    [Header("Item Property")]
    public bool isUnstoppable;
    public bool isInvisible;
    public bool isUntouchable;
    public float itemCDTime;

    [Header("Item with Character Porperty")]
    public GameObject untouchableHat;
    public GameObject untouchableStick;
    public GameObject invisibleHat;
    public GameObject unstoppableHat;

    // Player Expression Animation
    public Renderer faceRend;
    public Material[] deadMat;
    public Material[] metMat;

    // For Player Attacking Animation
    public bool isAttacking; 
    
    // For Player dead animation
    public bool isFacingVertical;

    // Attacking Sound
    public GameSceneAudioController attackSound;

    private void Start()
    {
        hearts.SetActive(false);

        // Item states
        isUnstoppable = false;
        isInvisible = false;
        isUntouchable = false;

        //itemCDTime = 7;
    }

    private void FixedUpdate()
    {
        if (isUntouchable || isInvisible || isUnstoppable)
        {
            itemCDTime -= Time.deltaTime;
            if (itemCDTime <= 0)
            {
                isUnstoppable = false;
                isInvisible = false;
                isUntouchable = false;
                
                //itemCDTime = 7;
            }

            else
            {
                if (isInvisible)
                {
                    // TO do : add animations, ignore enemy and obstacles
                    if(gameObject.name == "Player-Choi")
                    {
                        Physics.IgnoreLayerCollision(9, 11, true);
                        Physics.IgnoreLayerCollision(9, 12, true);
                    }
                    else
                    {
                        Physics.IgnoreLayerCollision(10, 11, true);
                        Physics.IgnoreLayerCollision(10, 12, true);
                    }
                }
                else
                {
                    if (gameObject.name == "Player-Choi")
                    {
                        Physics.IgnoreLayerCollision(9, 11, false);
                        Physics.IgnoreLayerCollision(9, 12, false);
                    }
                    else
                    {
                        Physics.IgnoreLayerCollision(10, 11, false);
                        Physics.IgnoreLayerCollision(10, 12, false);
                    }
                }
            }
        }

        else
        {
            if (gameObject.name == "Player-Choi")
            {
                Physics.IgnoreLayerCollision(9, 11, false);
                Physics.IgnoreLayerCollision(9, 12, false);
            }
            else
            {
                Physics.IgnoreLayerCollision(10, 11, false);
                Physics.IgnoreLayerCollision(10, 12, false);
            }

            // Turn off the item on character
            unstoppableHat.SetActive(false);
            invisibleHat.SetActive(false);
            untouchableHat.SetActive(false);
            untouchableStick.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            hearts.SetActive(true);
            go.playerIsMet = true;
            this.gameObject.GetComponent<Rigidbody>().isKinematic = true;

            // Animation 3: Victory animation
            gameObject.GetComponentInChildren<Animator>().SetInteger("animation", 3);
            faceRend.materials = metMat;
        }

        else if (collision.gameObject.tag == "Enemy")
        {
            if (isUntouchable)
            { 
                // Set Attacking Animation
                isAttacking = true;
                gameObject.GetComponentInChildren<Animator>().SetInteger("animation", 11);
                gameObject.GetComponentInChildren<Animator>().Play("animation",9,0);

                // Play Enemy Dead animation
                //collision.gameObject.GetComponent<EnemyController>().isHurt = true;
                collision.gameObject.GetComponent<Animator>().SetInteger("animation",6);
                // Turn off enemy's collider and rigid body
                collision.gameObject.GetComponent<Collider>().enabled = false;
                collision.gameObject.GetComponent<Rigidbody>().isKinematic = true;

                // Play Attacking Sound
                attackSound.sound.clip = attackSound.soundClips[3];
                attackSound.sound.Play();

                Destroy(collision.gameObject,2f);
                StartCoroutine(TurnOffAttackingAnimation());
            }
            else
            {
                go.playerIsDead = true;
                this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }

            // The character Dead animation is invisible when the character facing vertically.

            if (go.playerIsDead)
            {
                // Animation 6: Die animation
                gameObject.GetComponentInChildren<Animator>().SetInteger("animation", 7);
                faceRend.materials = deadMat;
            }

        }

    }

    IEnumerator TurnOffAttackingAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        
    }
    
}
