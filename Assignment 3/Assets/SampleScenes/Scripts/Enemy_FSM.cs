using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_FSM : MonoBehaviour
{
    // enum are nice to keep states

    public enum ENEMY_STATE { PATROL, CHASE, ATTACK };

    [SerializeField]
    private ENEMY_STATE currentState;
    // We need a function to get the current state
    public ENEMY_STATE CurrentState
    {
        get { return currentState; }

        set
        {
            currentState = value;

            //stop all coroutines
            StopAllCoroutines();

            switch (currentState)
            {
                case ENEMY_STATE.PATROL:
                    StartCoroutine(EnemyPatrol());
                    break;
                case ENEMY_STATE.CHASE:
                    StartCoroutine(EnemyChase());
                    break;
                case ENEMY_STATE.ATTACK:
                    StartCoroutine(EnemyAttack());
                    break;
            }
        }

    }

    // What about some references ?
    private CheckMyVision checkMyVision; // This is our previous file
    private NavMeshAgent agent = null;


    private Transform playerTransform = null;

    // reference to patrol Destination
    private Transform patrolDestination = null;

    private Health playerHealth = null;
    public float maxDamage = 10f;

    private void Awake()
    {
        checkMyVision = GetComponent<CheckMyVision>();
        agent = GetComponent<NavMeshAgent>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();

        // Do something about player transform too
        //playerTransform = GameObject.FindGameObjectWithTag("Player").Get
        playerTransform = playerHealth.GetComponent<Transform>();

    }








    // Start is called before the first frame update
    void Start()
    {
        GameObject[] destination = GameObject.FindGameObjectsWithTag("Dest");
        //agent = GetComponent<NavMeshAgent>();
        patrolDestination = destination[Random.Range(0, destination.Length)].GetComponent<Transform>();
        
        currentState = ENEMY_STATE.PATROL;
    }

    public IEnumerator EnemyPatrol()
    {
        while(currentState == ENEMY_STATE.PATROL)
        {
            checkMyVision.sensitivity = CheckMyVision.enmSensitivity.HIGH;
            agent.isStopped = false;
            agent.SetDestination(patrolDestination.position);

            while (agent.pathPending)
            {
                yield return null; // This is to ensure we wait for path to complete

            }

            if (checkMyVision.targetInSight)
            {
                agent.isStopped = true;
                currentState = ENEMY_STATE.CHASE;
                yield break;
            }
            yield break;
        }
        
    }

    public IEnumerator EnemyChase()
    {
        while (currentState == ENEMY_STATE.CHASE)
        {
            // In this case let us keep sensitivity low
            checkMyVision.sensitivity = CheckMyVision.enmSensitivity.LOW;

            // The idea of the chase is to go to the last known position
            agent.isStopped = false;
            agent.SetDestination(checkMyVision.lastKnownSighting);

            // again we need to yield if path is yet incomplete

            while (agent.pathPending)
            {
                yield return null;
            }

            // while chasing we need to keep checking if we reached

            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped = true;

                // what if we reached destination but cannot see the player?

                if (checkMyVision.targetInSight)
                    currentState = ENEMY_STATE.PATROL;
                else
                    currentState = ENEMY_STATE.ATTACK;
                yield break;
            }

            //till next frame
            yield return null;
        }

    }

    public IEnumerator EnemyAttack()
    {
        // like others start with loop
        while (currentState == ENEMY_STATE.ATTACK)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);

            while (agent.pathPending)
                yield return null;

            if(agent.remainingDistance > agent.stoppingDistance)
            {
                CurrentState = ENEMY_STATE.CHASE;
            }
            else
            {
                playerHealth.HealthPoints -= maxDamage * Time.deltaTime;
                //attack
                // Do something here later on about player health

            }
            yield return null;
        }
        yield break;
        
    }

    // Update is called once per frame
    void Update()
    {

    }
}
