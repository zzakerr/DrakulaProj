using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterInputController))] 
[RequireComponent(typeof(AudioSource))]
public class Character : Player
{/*
    // TO DO:
    [Range(1f,10f)][SerializeField] private float maxSpeedSit = 1f;
    [Range(0f, 50f)][SerializeField] private float jumpForce = 1f;
    */
    [Header("Character Settings")]
    [Range(1f,10f)][SerializeField] private float maxSpeedWalk = 3f;
    [Range(1f,10f)][SerializeField] private float maxSpeedRun = 6f;
    [SerializeField] private OnePersonCamera cameraMain;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private float stepRateWalk = 0.6f; 
    [SerializeField] private float stepRateRun = 0.6f; 
    [SerializeField] private AudioClip[] stepSounds; 
    public OnePersonCamera Camera => cameraMain;
    public Transform CameraPos => cameraPos;
    private AudioSource audioSource;
    private Rigidbody rb;
    private Vector3 moveVector;
    private float stepTime;

    public bool isMove;
    private void Awake()
    {
        Init();
    }
    
    private void Start()
    {
        cameraMain.SetTarget(cameraPos,TypeMoveCamera.WithRotation);
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
    }
    
    private const float Acceleration = 7000f;
    private const float AirMoveLimit = 0.300f;
    
    public void Move(Vector3 direction,MoveType moveType)
    {
        if (direction.magnitude > 1) direction /= 2;
        
        stepTime += Time.deltaTime;
        
        if (direction == Vector3.zero && moveType != MoveType.Air)
        {
            isMove = false;
            rb.velocity = Vector3.zero;
            return;
        }
        
        rb.AddRelativeForce(direction * (Acceleration * Time.deltaTime),ForceMode.Acceleration);
        switch (moveType)
        {
            case MoveType.Sit:
            {  
                /*
                if (rb.velocity.magnitude >= maxSpeedSit)
                {
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeedSit);
                }
                */
                break;
            }
            
            case MoveType.Walk:
            {
                isMove = true;
                if (rb.velocity.magnitude >= maxSpeedWalk)
                {
                    StepsPlay(stepRateWalk);
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeedWalk);
                }

                break;
            }
            
            case MoveType.Run:
            {
                isMove = true;
                if (rb.velocity.magnitude >= maxSpeedRun)
                {
                    StepsPlay(stepRateRun);
                    rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeedRun);
                }

                break;
            }
    
            case MoveType.Air:
                rb.velocity = new Vector3(Mathf.Lerp(rb.velocity.x,0,AirMoveLimit), rb.velocity.y, Mathf.Lerp(rb.velocity.z,0,AirMoveLimit));
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(moveType), moveType, null);
        }
    }

    private void StepsPlay(float stepLenght)
    {
        if (stepTime >= stepLenght)
        {
            audioSource.PlayOneShot(stepSounds[Random.Range(0,stepSounds.Length)]);
            stepTime = 0;
        }
    }
    
    private void CharacterRotate()
    {
        if (cameraMain.IsLocked || cameraMain.enabled)
        {
            return;
        }
      
        transform.rotation = new Quaternion(0, cameraMain.transform.rotation.y,0, cameraMain.transform.rotation.w);
    }
    
    public void CameraMove(float dirX,float dirY)
    {
         cameraMain.Rotate(dirX, dirY);
         CharacterRotate();
    }
    
    /*
    public void Jump()
    {
        rb.AddRelativeForce(transform.up * jumpForce , ForceMode.Impulse);
    }
    */
    public void SetCamera(OnePersonCamera cameraForPlayer)
    {
        cameraMain = cameraForPlayer;
    }
}
