using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[RequireComponent(typeof(InteractiveObject))]
[RequireComponent(typeof(AudioSource))]
public class Dracula : SingletonBase<Dracula>
{
    [SerializeField] private bool playOnAwake = true;
    [Space][Header("Dracula Prefabs")]
    [SerializeField] private GameObject draculaPrefabsNone;
    [SerializeField] private GameObject draculaPrefabsSexy;
    [SerializeField] private GameObject draculaPrefabsCross;
    [SerializeField] private GameObject draculaPrefabsStand;
    [SerializeField] private GameObject draculaPrefabsFly;
    [SerializeField] private GameObject draculaPrefabsHand;
    
    [Space][Header("Visual Prefabs")]
    [SerializeField] private DraculaSpawnEffect draculaSpawnEffectPrefab;
    [SerializeField] private ImpactEffect visionEffectPrefab;

    [Space] [Header("Dracula Settings")] 
    [SerializeField] [Range(1f, 15f)]private int spawnSpeed = 7;
    [SerializeField] [Range(0f, 10f)]private int speedChange = 2;
    [SerializeField] [Range(0f, 50f)] private float minDistanceToNextPp = 6;
    [SerializeField] [Range(0f, 50f)] private float minDistanceToPlayer = 3;
    [SerializeField] private AudioClip[] spawnClips;
    [SerializeField] private PatrolPoint[] spawnPositions;
    
    private Transform character;
    private GameObject draculaPrefab;
    private List<PatrolPoint> patrolPoints;
    private List<PatrolPoint> nearestPatrolPoints;
    private AudioSource source;
    private MeshRenderer draculaMeshRenderer;
    private DraculaSpawnEffect draculaSpawnEffect;
    private float timer;
    
    
    private bool isHeart = false;
    private bool isVisible = false;

    [HideInInspector] public UnityEvent<int> draculaInPlayer;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        CharacterInputController.Instance.heartOn.AddListener(TogleHeartOn);
        CharacterInputController.Instance.heartOff.AddListener(TogleHeartOff);
        GetComponent<InteractiveObject>().onVision.AddListener(TogleVisionOn);
        GetComponent<InteractiveObject>().onHide.AddListener(TogleVisionOff);
        NoiseLevel.Instance.OnChange += SpeedChange;
        nearestPatrolPoints = new List<PatrolPoint>();
        patrolPoints = new List<PatrolPoint>();
        
        FillPatrolPointsInScene();
        
        character = Character.Instance.transform;
        source = GetComponent<AudioSource>();
        
        if (!playOnAwake)enabled = false;
    }

    private void OnDestroy()
    {
        CharacterInputController.Instance.draculaAnim.RemoveListener(TogleHeartOn);
        CharacterInputController.Instance.draculaAnim.RemoveListener(TogleHeartOff);
        GetComponent<InteractiveObject>().onVision.RemoveListener(TogleVisionOn);
        GetComponent<InteractiveObject>().onHide.RemoveListener(TogleVisionOff);
        NoiseLevel.Instance.OnChange -= SpeedChange;
    }

    private int lastValue = 0;
    private void SpeedChange(int value)
    {
        if (lastValue > value) spawnSpeed += speedChange;
        else if (spawnSpeed - speedChange >= 0 )spawnSpeed -= speedChange;
        lastValue = value;
    }

    private void FixedUpdate()
    {
        if (draculaSpawnEffect != null && draculaSpawnEffect.IsPlaying())
        {
            DraculaState();
            return;
        }
        
        timer += Time.deltaTime;
        
        if (timer >= spawnSpeed)
        {
            DraculaMove();
            timer = 0;
        }
        DraculaState();
    }

    private void DraculaState()
    {
        VisibleMeshDracula();
        DraculaRotateToPlayer();
        DraculaEffect();
    }
    
    private void TogleVisionOn() => isVisible = true;
    private void TogleVisionOff() => isVisible = false;
    private void TogleHeartOn() => isHeart = true;
    private void TogleHeartOff() => isHeart = false;

    private bool isActiveMesh;
    private void VisibleMeshDracula()
    {
        if (draculaMeshRenderer != null)
        {
            if (isVisible && isHeart || isHeart && isActiveMesh)
            {
                if (draculaMeshRenderer.enabled == false)
                {
                    isActiveMesh = true;
                    draculaMeshRenderer.enabled = true;
                    Instantiate(visionEffectPrefab,transform.position,Quaternion.identity);
                }
            }
            else
            {
                if (draculaMeshRenderer.enabled == true)
                {
                    isActiveMesh = false;
                    draculaMeshRenderer.enabled = false;
                    Instantiate(visionEffectPrefab,transform.position,Quaternion.identity);
                }
            }
        }
    } 
    private void DraculaEffect()
    {
        if (isVisible && isHeart)
        {
            if (draculaSpawnEffect == null)
            {
                draculaSpawnEffect = Instantiate(draculaSpawnEffectPrefab,new Vector3(transform.position.x,transform.position.y,transform.position.z), Quaternion.identity);
            }
            else if (!draculaSpawnEffect.IsPlaying())
            {
                draculaSpawnEffect = Instantiate(draculaSpawnEffectPrefab,new Vector3(transform.position.x,transform.position.y,transform.position.z), Quaternion.identity);
            }
        }
    }
    private void DraculaRotateToPlayer() 
    {
        if (draculaPrefab != null)
        {
            draculaPrefab.transform.LookAt(new Vector3(character.position.x,transform.position.y,character.position.z));
        }
    }
    public void DraculaSpawn()
    {
        PatrolPoint rand = spawnPositions[Random.Range(0, spawnPositions.Length)];
        transform.position = rand.transform.position;
        Spawn(rand);
    }
    public void DraculaSpawn(PatrolPoint spawnPoint)
    {
        transform.position = spawnPoint.transform.position;
        Spawn(spawnPoint);
    }
    
    public void DraculaSpawns(PatrolPoint[] spawnPoints)
    {
        spawnPositions = spawnPoints;
        PatrolPoint rand = spawnPositions[Random.Range(0, spawnPositions.Length)];
        Spawn(rand);
    }
    
    private void Spawn(PatrolPoint spawnPoint)
    {
        source.PlayOneShot(spawnClips[Random.Range(0,spawnClips.Length)]);
        draculaPrefab = Instantiate(GetDraculaPrefab(spawnPoint), transform.position, Quaternion.identity, transform);
        draculaMeshRenderer = draculaPrefab.GetComponent<MeshRenderer>();
        draculaMeshRenderer.enabled = false;
        enabled = true;
    }

    public void DraculaEnable()
    {
        transform.position = lastPosition;
        enabled = true;
    }
    
    private Vector3 lastPosition;
    public void DraculaDisable()
    {
        lastPosition = transform.position;
        transform.position = Vector3.zero;
        timer = 0;
        enabled = false;
    }

    private void DraculaMove()
    {
        FindNearestPatrolPoint();

        if (nearestPatrolPoints.Count == 0) return;
        
        Destroy(draculaPrefab);
        
        var patrolPoint = FindPatrolPointsToPlayer();
        
        /*
        if (patrolPoint.DraculaPos == DraculaPosType.Player)
        {
            KillPlayer();
            return;
        }*/
        
        draculaPrefab = Instantiate(GetDraculaPrefab(patrolPoint), patrolPoint.transform.position, Quaternion.identity, transform);
        draculaMeshRenderer = draculaPrefab.GetComponent<MeshRenderer>();
        draculaMeshRenderer.enabled = false;
        CleatNearestPatrolPoint();
    
    }

    private GameObject GetDraculaPrefab(PatrolPoint patrolPoint)
    {
        var currentDraculaPrefab = draculaPrefabsNone;
        
        if (patrolPoint.DraculaPos == DraculaPosType.None && draculaPrefabsNone != null)
        {
            Random.Range(2, 5);
        }
        
        if (patrolPoint.DraculaPos == DraculaPosType.Sexy 
            && draculaPrefabsSexy != null) currentDraculaPrefab = draculaPrefabsSexy;
        if (patrolPoint.DraculaPos == DraculaPosType.Stand 
            && draculaPrefabsStand != null) currentDraculaPrefab = draculaPrefabsStand;
        if (patrolPoint.DraculaPos == DraculaPosType.Cross 
            && draculaPrefabsCross != null) currentDraculaPrefab = draculaPrefabsCross;
        if (patrolPoint.DraculaPos == DraculaPosType.Hand 
            && draculaPrefabsHand != null) currentDraculaPrefab = draculaPrefabsHand;
        if (patrolPoint.DraculaPos == DraculaPosType.Fly 
            && draculaPrefabsFly != null) currentDraculaPrefab = draculaPrefabsFly;
        transform.position = patrolPoint.transform.position;

        return currentDraculaPrefab;
    }

    private void KillPlayer()
    {
        draculaInPlayer.Invoke(1);
        enabled = false;
    }

    private readonly Vector3 OffsetY = new Vector3(0, 0.5f, 0);
    private void FindNearestPatrolPoint()
    {
        var draculaPos = transform.position;
        
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            PatrolPoint patrolPoint = patrolPoints[i];
            var distance = Vector3.Distance(draculaPos, patrolPoint.transform.position);
            
            if (distance < minDistanceToNextPp)
            {   
                RaycastHit hitInfo;
                Ray ray = new Ray(transform.position + OffsetY, patrolPoint.transform.position - transform.position + OffsetY);
                
                Debug.DrawLine(transform.position + OffsetY, patrolPoint.transform.position + OffsetY, Color.blue,3f);
                
                if (Physics.Raycast(ray, out hitInfo,minDistanceToNextPp))
                {
                    if (hitInfo.collider.transform.parent?.GetComponent<Character>())
                    {
                        if (Vector3.Distance(hitInfo.transform.position,transform.position) <= minDistanceToPlayer)
                        {
                            KillPlayer();
                            enabled = false;
                            return;
                        }
                    }
                    Debug.DrawLine(transform.position, patrolPoint.transform.position, Color.red, 3f);
                    continue;
                }
 
                nearestPatrolPoints.Add(patrolPoint);
            }
        }
    }
    
    private PatrolPoint FindPatrolPointsToPlayer()
    {
        var playerPos = character.transform.position;
        float minDist =  Mathf.Infinity;
        PatrolPoint spawnPoints = null;
        for (int i = 0; i < nearestPatrolPoints.Count; i++)
        {
            PatrolPoint patrolPoint = nearestPatrolPoints[i];
            var distance = Vector3.Distance(playerPos, patrolPoint.transform.position);
            if (distance < minDist)
            {
                spawnPoints = patrolPoint;
                minDist = distance;
            }
        }

        return spawnPoints;
    }

    private void CleatNearestPatrolPoint()
    {
        nearestPatrolPoints.Clear();
    }

    private void FillPatrolPointsInScene()
    {
        patrolPoints.AddRange(FindObjectsOfType<PatrolPoint>());
    }
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minDistanceToNextPp);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistanceToPlayer);
    }

}