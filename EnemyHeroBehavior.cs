using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panda;
using System.Linq;

public class EnemyHeroBehavior : MonoBehaviour
{
    //AI Path Finding
    AIPath aiPath;

    GameObject player;

    // Dashing Particles
    public GameObject dashingParticle;
    bool onFlee;

    // FireArm behavior control variables
    public GameObject[] firearmObjects;
    bool firearmIsActiveNow;

    GameObject firearmFirst;
    GameObject firearmSecond;

    public float firearmActiveTime;
    int firearmFirstBullet;
    int firearmSecondBullet;
    bool bulletIsRecovering;
    bool bulletRecoverOnProcess;

    // Skill behavior control variables
    public GameObject[] skillList;
    bool skillIsReady;
    public int skillNumber;
    public float skillCountDown;

    // Enemy base control variables
    EHBaseLocation basePoint;
    public GameObject[] enemyBases;
    int enemyBaseNum;
    float enemyBaseCountDown;
    bool enemyBaseIsActive;

    // Enemy Spawn Behavior
    public GameObject[] enemyArmies;
    int enemyArmiesNum;
    float enemyArmiesCountDown;
    bool enemyArmiesIsActive;
    int enemyArmySpawnNum;


    // For Health
    EnemyHeroHealth enemyHeroHealth;
    public bool healthIsRecovering;
    public float randomHealthValue;

    // AI Target for House Object -- When EnemyHero flees, change to enemy house Target.
    public GameObject enemyHouse;

    // AI is in Danger movement
    public List<GameObject> column1 = new List<GameObject>();
    public List<GameObject> column2 = new List<GameObject>();
    public List<GameObject> column3 = new List<GameObject>();
    public bool safetyTrigger1IsActive;
    public bool safetyTrigger2IsActive;
    public bool onSafetyColumn1;
    public bool onSafetyColumn2;
    public bool onSafetyColumn3;

    public float hurtValue;
    float tempHurtValue;
    public GameObject enemyAttack;
    public float enemyToObjectDistance;

    public int scoring;

    private Vector2 playerDirection;
    private float xdif;
    private float ydif;
    public float speed;
    public float rotationSpeed;

    // Ally Object -- use these variables to make a smart AI movement
    // Eg. if army is greater than ally more than 3 units, then AI move to the safe place while firing.
    public List<GameObject> allies = new List<GameObject>();

    // Enemy Chase Base
    public List<GameObject> baseObject = new List<GameObject>();
    public Vector3 vectorToTarget;

    // Head 7 SlowGun proporty
    bool slowIsActive;

    // Wing 8 zomebie. Keep track of the last hit, so that it can be used to track whether the character or the escort killed the enemy
    string lastHit;
    public GameObject zombie;

    // Escort 2, keep track the enemy speed, if the enemy is already slow down, then re-enter the slow down area would not slow the enemy down.
    public bool slowDownIsActive;
    float tempSpeed;
    bool slowed;

    // Escort 5, keep track the enemy speed, if the enemy is frozen.
    public bool frozenIsActive;
    bool freezed;

    // Skills No.5 Toxic Skills
    public bool toxicIsActive;

    // Effect of Sprite and particles
    public GameObject frozenSprite;
    public GameObject toxicEffect;

    // Spawn Round Effect
    SpawnController spawnEffect; // increases 5% per round, increasese 45% per decimal round

    // Army9 Spawn army
    public GameObject armyOrigin;

    // Enemy 4 Add HP Effect
    public bool enemy4HPEffectIsActive;
    public GameObject enemy4HPEffect;

    // Enemy Hero Fire
    public Vector3 bulletOffset;
    public GameObject bulletPrefab;

    public float fireDelay;
    float tempFireDelay;
    float coolDownTimer = 1f;

    // Enemy Hero Movement Restriction
    public float heroToObjectDistance;

    // Player skill variable
    public GameObject explodeSkill;
    public bool expldeSkillIsActive;

    // Use this for initialization
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        aiPath = GetComponent<AIPath>();
        enemyHeroHealth = GetComponent<EnemyHeroHealth>();

        slowDownIsActive = false;
        tempSpeed = speed;
        slowed = false;
        freezed = false;
        slowIsActive = false;
        tempFireDelay = fireDelay;
        onFlee = false;
        dashingParticle.SetActive(false);

        firearmActiveTime = Random.Range(25,36);
        firearmFirstBullet = 0;
        firearmSecondBullet = 0;
        firearmIsActiveNow = false;

        // For Skill Initial Variables
        skillCountDown = Random.Range(20, 30);
        skillIsReady = false;

        // For Bases
        basePoint = GameObject.Find("EnemyBases").GetComponent<EHBaseLocation>();
        enemyBaseCountDown = Random.Range(30, 40);

        // For Enemy Army Spawn Control
        enemyArmiesCountDown = Random.Range(5, 10);
        enemyArmiesIsActive = false;

    }

    // Update is called once per frame
    void Update()
    {
        if(player == null)
            player = GameObject.FindGameObjectWithTag("Player");


        firearmActiveTime -= Time.deltaTime;
        if (firearmActiveTime <= 0) {
            firearmActiveTime = Random.Range(180, 241);
            int firearmNum1 = Random.Range(1,4);
            int firearmNum2 = Random.Range(4,8);

            firearmFirst = firearmObjects[firearmNum1];
            firearmSecond = firearmObjects[firearmNum2];

            firearmFirstBullet = 20;
            firearmSecondBullet = 20;
            firearmIsActiveNow = true;
        }

        if (firearmIsActiveNow == true && bulletIsRecovering == false)
        {

            if (firearmFirstBullet <= 0)
            {
                StartCoroutine(RecoverBullet());
                bulletRecoverOnProcess = true;
            }
            else
            {
                bulletRecoverOnProcess = false;
            }
        }

        if (firearmFirstBullet > 20)
            firearmFirstBullet = 20;
        else if (firearmSecondBullet > 20)
            firearmSecondBullet = 20;

        // For Skill Logic
        skillCountDown -= Time.deltaTime;
        if (skillCountDown <= 0)
        {
            skillCountDown = Random.Range(20, 30);
            if (skillIsReady == false)
            {
                skillIsReady = true;
                //skillNumber = Random.Range(0,4);
                skillNumber = 3;
            }
        }

        // If the player health is less than 50%, or if the radar detects army more than 7, trigger explode skill
        if (player.GetComponent<PlayerController>().currentHealth[player.GetComponent<PlayerController>().currentCharacterNumber]
            / player.GetComponent<PlayerController>().startingHealth[player.GetComponent<PlayerController>().currentCharacterNumber] < 0.5f
            && skillNumber == 0 && skillIsReady == true)
        {
            if (baseObject.Count > 0)
            {
                if (baseObject[0].tag == "Player")
                {
                    Instantiate(skillList[0], transform.position, transform.rotation);
                    skillIsReady = false;
                }
            }
        }

        else if(baseObject.Count >= 7 && skillNumber == 0 && skillIsReady == true) {
            Instantiate(skillList[0], transform.position, transform.rotation);
            skillIsReady = false;
        }

        // If the player is in the Radar or if the basecount is greater or equal to 5, trigger frozen skill
        if (baseObject.Count > 0 && skillNumber == 1 && skillIsReady == true)
        {
            if (baseObject[0].tag == "Player")
            {
                Instantiate(skillList[1], transform.position, transform.rotation);
                skillIsReady = false;
            }
        }

        else if (baseObject.Count > 5 && skillNumber == 1 && skillIsReady == true)
        {
            Instantiate(skillList[1], transform.position, transform.rotation);
            skillIsReady = false;
        }

        // If the player current health is less than 80% and if the player is in the radar trigger toxic
        if (player.GetComponent<PlayerController>().currentHealth[player.GetComponent<PlayerController>().currentCharacterNumber]
            / player.GetComponent<PlayerController>().startingHealth[player.GetComponent<PlayerController>().currentCharacterNumber] < 0.8f
            && skillNumber == 2 && skillIsReady == true)
        {
            if (baseObject.Count > 0)
            {
                if (baseObject[0].tag == "Player")
                {
                    Instantiate(skillList[2], transform.position, transform.rotation);
                    skillIsReady = false;
                }
            }
        }

        // Skill is also In Flee Function

        // If the player is in the radar, trigger laser skill
        if (baseObject.Count > 0 && skillNumber == 3 && skillIsReady == true)
        {
            Debug.Log("Inside the trigger laser function");
            skillList[3].SetActive(true);
            skillIsReady = false;
        }

        // Skill Logic Ends

        // Enemy Base Control Logic
        enemyBaseCountDown -= Time.deltaTime;
        if (enemyBaseCountDown <= 0)
        {
            enemyBaseCountDown = Random.Range(30,40);
            enemyBaseNum = Random.Range(0,3);
            
            for (int i = 0; i < basePoint.basePosition.Length; i++)
            {
                if (basePoint.baseIsOccupied[i] == false)
                {
                    basePoint.baseIsOccupied[i] = true;
                    GameObject randomBase = Instantiate(enemyBases[enemyBaseNum], basePoint.basePosition[i].position, basePoint.basePosition[i].rotation);
                    basePoint.baseObjects[i] = randomBase;
                    break;
                }
            }
        }
        /* End Of Enemy Base Control*/

        // Enemy Spawn Logic
        enemyArmiesCountDown -= Time.deltaTime;
        if (enemyArmiesCountDown <= 0)
        {
            enemyArmiesCountDown = Random.Range(5,10);
            enemyArmiesNum = Random.Range(0,15);
            enemyArmySpawnNum = Random.Range(1,5);
            for (int i = 0; i < enemyArmySpawnNum; i++)
            {
                // To do :  Need to change spawn position.
                //Instantiate(enemyArmies[i], enemyHouse.transform.position, enemyHouse.transform.rotation);
            }
        }
    }

    [Task]
    public void MoveToDestination()
    {
        if (frozenIsActive == false)
        {
            if (GameObject.Find("ShadowNinja(Clone)") == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");

                // Enemy Chasing House
                GameObject house = GameObject.FindGameObjectWithTag("House");

                if (PlayerPrefs.GetInt("GameOver") == 0)
                {
                    // Distance Between house - enemy, player - enemy
                    // Remove the object in the list, if the object no longer exist
                    for (int i = 0; i < baseObject.Count; i++)
                    {
                        if (baseObject[i] == null)
                            baseObject.RemoveAt(i);
                    }

                    //if base exist chase base.
                    if (baseObject.Any())
                    {
                        if (baseObject[0] == null)
                        {

                            baseObject.Sort(delegate (GameObject x, GameObject y)
                            {
                                return Vector3.Distance(this.transform.position, x.transform.position)
                                    .CompareTo(Vector3.Distance(this.transform.position, y.transform.position));
                            });
                        }
                        if (baseObject[0].CompareTag("Player"))
                        {
                            if (baseObject[0].GetComponent<PlayerController>().isActiveAndEnabled == false)
                            {
                                baseObject.RemoveAt(0);
                            }
                        }

                        if (Vector3.Distance(transform.position, baseObject[0].transform.position) > heroToObjectDistance)
                        {
                            vectorToTarget = baseObject[0].transform.position - transform.position;
                            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
                            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
                            GetComponent<Rigidbody2D>().AddForce(vectorToTarget.normalized * speed);
                        }

                        else if (Vector3.Distance(transform.position, baseObject[0].transform.position) <= heroToObjectDistance)
                        {
                            vectorToTarget = baseObject[0].transform.position - transform.position;
                            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
                            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
                            // Need to be modified june 9 th 2018
                            //if (angle < 5.0f)
                            //    Task.current.Succeed();
                        }

                    }

                    else

                    {
                        // Facing House
                        xdif = house.transform.position.x - transform.position.x;
                        ydif = house.transform.position.y - transform.position.y;
                        Vector2 houseDirection = new Vector2(xdif, ydif);

                        float angle = Mathf.Atan2(ydif, xdif) * Mathf.Rad2Deg;
                        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

                        transform.rotation = Quaternion.Slerp(transform.rotation, q, rotationSpeed * Time.deltaTime);
                        GetComponent<Rigidbody2D>().AddForce(houseDirection.normalized * speed);
                        Task.current.Succeed();
                    }
                }
            }
            // Shadow Ninja Exist
            else
            {

            }
        }

        // Escort 1 Slow down area
        if (slowDownIsActive == true && slowed == false && frozenIsActive == false)
        {
            speed = speed * 0.5f;
            slowed = true;
        }
        else if (slowDownIsActive == false && slowed == true && frozenIsActive == false)
        {
            speed = tempSpeed;
            slowed = false;
        }

        // Escort 5 + Bullet Frozen
        if (frozenIsActive == true && freezed == false)
        {
            speed = 0f;
            freezed = true;
            gameObject.GetComponentInChildren<Animator>().speed = 0;
            frozenSprite.SetActive(true);
            gameObject.GetComponent<Rigidbody2D>().freezeRotation = true;
        }
        else if (frozenIsActive == false && freezed == true)
        {
            speed = tempSpeed;
            freezed = false;
            gameObject.GetComponentInChildren<Animator>().speed = 1;
            gameObject.GetComponent<Rigidbody2D>().freezeRotation = false;
            frozenSprite.SetActive(false);
        }

    }

    [Task]
    public void StartAttack()
    {
        coolDownTimer -= Time.deltaTime;

        if (coolDownTimer <= 0)
        {

            coolDownTimer = fireDelay;

            Vector3 offset = transform.rotation * bulletOffset;

            GameObject bulletGo = (GameObject)Instantiate(bulletPrefab, transform.position + offset, transform.rotation);
            bulletGo.layer = gameObject.layer;
            bulletGo.name = this.gameObject.name;

        }
        Task.current.Succeed();
    }

    // Object Detection and elimination
    public void ObjectDetected(GameObject heroObject)
    {
        baseObject.Add(heroObject);
    }

    public void ObjectEliminated(GameObject heroObject)
    {
        baseObject.Remove(heroObject);
    }

    // Ally Detection and elimination
    public void AllyDetected(GameObject heroObject)
    {
        allies.Add(heroObject);
    }

    public void AllyEliminated(GameObject heroObject)
    {
        allies.Remove(heroObject);
    }

    // Move to Object if Enemy Hero Found an Army
    [Task]
    public void MoveToObject()
    {
        aiPath.target = baseObject[0].transform;
        aiPath.rotationSpeed = 360;

        if (Vector2.Distance(baseObject[0].transform.position, transform.position) <= 10)
        {
            aiPath.speed = 0;
            
            vectorToTarget = baseObject[0].transform.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
            Task.current.Succeed();
        }
        else if (Vector2.Distance(baseObject[0].transform.position, transform.position) > 10)
        {
            aiPath.speed = speed;
            Task.current.Succeed();
        }
    }


    // Use bool to check if the hero see the army or player
    [Task]
    public bool ObjectsInRange()
    {
        if (baseObject.Count > 0)
        {
            if (baseObject[0] != null)
            {
                return true;
            }
            else {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    // When the hero not see an army, follow Player
    [Task]
    public void LookForPlayer()
    {
        aiPath.target = player.transform;
        aiPath.speed = speed;
        aiPath.rotationSpeed = 360;
        Task.current.Succeed();
    }

    // When Enemy Hero health less than certain range, then trigger Flee function
    [Task]
    public void Flee()
    {
        aiPath.target = enemyHouse.transform;
        if (onFlee == false) {
            onFlee = true;
            StartCoroutine(DashOnFlee());
        };

        // Trigger Shadow Skill on Flee
        if(skillNumber == 1 && skillIsReady == true)
        {
            Instantiate(skillList[1], transform.position, transform.rotation);
            skillIsReady = false;
        }
        

        if (Vector3.Distance(transform.position, enemyHouse.transform.position) <= 6)
        {
            aiPath.speed = 0;
            Task.current.Succeed();
        }
    }


    IEnumerator DashOnFlee()
    {
        aiPath.speed = speed * 2;
        dashingParticle.SetActive(true);
        yield return new WaitForSeconds(2);
        dashingParticle.SetActive(false);
        aiPath.speed = speed;
    }

    // Use bool to check whether the Enemy hero health is less than certain range
    [Task]
    public bool IsHealthLessThan(float health)
    {
        if (enemyHeroHealth.currentHealth <= health)
            return true;
        else
            return false;
    }

    // Use bool Recovering to check whether the hero is in recovering phase
    [Task]
    public bool IsRecovering() {
        if (healthIsRecovering == false)
        {
            randomHealthValue = Random.Range(0.5f,0.8f);
        }

        if (enemyHeroHealth.currentHealth <= enemyHeroHealth.health * randomHealthValue && Vector3.Distance(transform.position, enemyHouse.transform.position) <= 6)
        {
            healthIsRecovering = true;
            return true;
        }
        else
        {
            onFlee = false;
            return false;
        }
    }

    [Task]
    // Dead Function For Enemy Hero
    public void EnemyHeroDie()
    {
        enemyHeroHealth.currentHealth = 0;
    }

    // Skill Functions
    [Task]
    public void ExplodeSkillTrigger()
    {
        Instantiate(explodeSkill, transform.position, transform.rotation);
        Task.current.Succeed();
    }

    // Firearm Behaviors
    [Task]
    public bool FirearmFirstIsActive() {
        if (firearmFirstBullet > 0 && bulletRecoverOnProcess == false)
        {
            bulletPrefab = firearmFirst;
            return true;
        }
        else
        {
            return false;
        }
    }

    [Task]
    public bool FirearmSecondIsActive() {
        if (firearmSecondBullet > 0)
        {
            bulletPrefab = firearmSecond;
            return true;
        }
        else
        {
            return false;
        }
    }

    [Task]
    public void Fire()
    {
        coolDownTimer -= Time.deltaTime;

        if (coolDownTimer <= 0)
        {

            coolDownTimer = fireDelay;

            Vector3 offset = transform.rotation * bulletOffset;

            bool firstFirearmIsActive = FirearmFirstIsActive();
            if (firstFirearmIsActive == true)
            {
                firearmFirstBullet -= 1;
            }
            else if (firstFirearmIsActive == false)
            {
                bool secondFirearmIsActive = FirearmSecondIsActive();
                if (secondFirearmIsActive == true)
                {
                    firearmSecondBullet -= 1;
                }
                else
                {
                    bulletPrefab = firearmObjects[0];
                }
            }
            if (skillList[3].activeSelf == false)
            {
                GameObject bulletGo = (GameObject)Instantiate(bulletPrefab, transform.position + offset, transform.rotation);
                bulletGo.layer = gameObject.layer;
                bulletGo.name = this.gameObject.name;
            }
        }
        Task.current.Succeed();
    }
    // End of Firearm Behavior

    // Hero is in danger operation
    [Task]
    public bool HeroInDanger()
    {
        if(baseObject.Count-allies.Count >= 3)
        {
            if (player.GetComponent<PlayerController>().currentHealth[player.GetComponent<PlayerController>().currentCharacterNumber]
                / player.GetComponent<PlayerController>().startingHealth[player.GetComponent<PlayerController>().currentCharacterNumber] <= 0.15f)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    [Task]
    public void MoveToSafePlace()
    {
        if(safetyTrigger1IsActive == false && safetyTrigger2IsActive == false)
        {
       
            column1.Sort(delegate (GameObject x, GameObject y)
            {
                return Vector3.Distance(this.transform.position, x.transform.position)
                    .CompareTo(Vector3.Distance(this.transform.position, y.transform.position));
            });
            
            GameObject movePoint = column1[0];
            aiPath.target = movePoint.transform;
            aiPath.speed = speed;

            /*
            vectorToTarget = baseObject[0].transform.position - transform.position;
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90;
            Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotationSpeed);
            */
            if (Vector3.Distance(transform.position, column1[0].transform.position) <= 0.1)
            {
                aiPath.speed = 0;

                vectorToTarget = baseObject[0].transform.position - transform.position;
                float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - 90;
                Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotationSpeed);

                Task.current.Succeed();
            }
        }
        
        else if (safetyTrigger1IsActive == true && safetyTrigger2IsActive == false)
        {
            column2.Sort(delegate (GameObject x, GameObject y)
            {
                return Vector3.Distance(this.transform.position, x.transform.position)
                    .CompareTo(Vector3.Distance(this.transform.position, y.transform.position));
            });

            GameObject movePoint = column2[0];
            aiPath.target = movePoint.transform;
            aiPath.speed = speed;

            if (Vector3.Distance(transform.position, column2[0].transform.position) <= 0.1)
            {
                aiPath.speed = 0;
                Task.current.Succeed();
            }
        }

        else if (safetyTrigger1IsActive == false && safetyTrigger2IsActive == true)
        {
            column3.Sort(delegate (GameObject x, GameObject y)
            {
                return Vector3.Distance(this.transform.position, x.transform.position)
                    .CompareTo(Vector3.Distance(this.transform.position, y.transform.position));
            });

            GameObject movePoint = column3[0];
            aiPath.target = movePoint.transform;
            aiPath.speed = speed;

            if (Vector3.Distance(transform.position, column3[0].transform.position) <= 0.1)
            {
                aiPath.speed = 0;
                Task.current.Succeed();
            }
        }        
    }

    IEnumerator RecoverBullet()
    {
        bulletIsRecovering = true;
        yield return new WaitForSeconds(12);
        firearmFirstBullet += 10;
        firearmSecondBullet += 10;
        bulletIsRecovering = false;
    }
}
