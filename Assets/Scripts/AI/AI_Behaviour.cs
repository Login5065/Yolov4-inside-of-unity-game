using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class AI_Behaviour : MonoBehaviour , IObservable<bool>
{
    [SerializeField] private Camera m_camera;
    [SerializeField] private CinemachineVirtualCamera vircamera;
    [SerializeField] public FieldOfView FOV;
    [SerializeField] private RenderTexture _renderTexture;
    private IObserver<bool> _game;
    public bool restart = false;

    #region BasicBehaviour
    public float rotSpeed;
    private float rot = 0;
    private float lim = 20;
    public int rotAmount = 0;
    public int rotTimes = 3;
    public bool shouldRotate = true;
    public bool e;
    public bool noMoving = false;
    #endregion

    #region Walk Behaviour
    public float WaypointRotLimit;
    public float WalkRotLimit;
    [Range(1,4)]
    public float WalkSpeed = 3.5f;
    
    public bool lookAtWaypoint = false;

    #endregion

    #region Seen Behaviour

    private bool SeenThisFrame = false;
    private bool wasSeen = false;

    public float seen = 0;
    [Range(1,9)]
    public float seenLimit = 5;
    [Range(0.5f,1.5f)]
    public float seenCooldown = 0.3f;
    [Range(0.0f,1.5f)]
    public float seenExtraSpeed = 0;
    #endregion

    #region View Behaviour
    [Range(0,14)]
    public int classIndex = 14;
    [Range(0.001f,1.0f)]
    public float treshold = 0.5f;

    private bool CaughtLastFrame = false;
    private bool CaughtThisFrame = false;
    #endregion

    #region WaypointBehaviour
    public NavMeshAgent navMeshAgent;
    public List<Transform> waypoints;
    private Vector3 pos;
    private int m_CurrentWaypointIndex = 0;
    private bool atWaypoint = false;
    #endregion

    #region Animatons
    private float _animationBlend;
    public Animator _animator;
    private bool _hasAnimator = false;
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;
    public float inputMagnitude = 0;
    #endregion
    

    private Coroutine thinkingRoutine;
    private Coroutine behaviourRoutine;
    
    
    private void Start()
    {
        if (_animator)
        {
            _hasAnimator = true;
        }
        AssignAnimationIDs();
        _animator.SetBool(_animIDGrounded, true);

        navMeshAgent.speed = WalkSpeed;
        _renderTexture = new RenderTexture(416,416,3);
        m_camera.targetTexture = _renderTexture;
        m_camera.enabled = false;
        lim = WalkRotLimit;
        if (waypoints.Count > 0)
        { 
            navMeshAgent.SetDestination (waypoints[0].position);
        }
        else
        {
            atWaypoint = true;
        }

        behaviourRoutine = StartCoroutine(BasicRoutine());
        
        thinkingRoutine = StartCoroutine(Thinking());
    }
    
    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }
    
    IEnumerator Thinking()
    {

        if (shouldRotate)
        {
            gameObject.transform.rotation = transform.parent.rotation;
        }


        while (true)
        {
            
            yield return new WaitUntil(() => seen >= seenLimit);
            if (restart)
            {
                restart = true;

                yield return new WaitForSeconds(10);
                restart = false;

                if (!noMoving)
                {
                    restart = false;
                    navMeshAgent.speed = WalkSpeed;
                    gameObject.transform.rotation = transform.parent.rotation;
                    rot = 0;
                }
                else
                {
                    restart = false;
                }

                seen = 0;
            }
            else
            {
                if (!noMoving)
                {
                    navMeshAgent.SetDestination(pos);
                    navMeshAgent.speed = WalkSpeed + seenExtraSpeed;
                    gameObject.transform.rotation = transform.parent.rotation;
                    rot = 0;
                }
                seen = 0;

            }
        }
        
    }
    
    
    
    private IEnumerator BasicRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        if (waypoints.Count != 0)
        {


            while (true)
            {
                yield return wait;

                lim = WalkRotLimit;
                if (wasSeen == false)
                {
                    if (navMeshAgent.remainingDistance < navMeshAgent.stoppingDistance)
                    {
                        if (lookAtWaypoint)
                        {
                            atWaypoint = true;
                            lim = WaypointRotLimit;
                            rotAmount = rotTimes;
                            navMeshAgent.speed = 0;
                            gameObject.transform.rotation = transform.parent.rotation;
                            rot = 0;
                            yield return new WaitUntil(() => rotAmount == 0);
                        }
                        navMeshAgent.speed = WalkSpeed;
                        PositionCalculation();
                        atWaypoint = false;
                    }
                }
                else
                {

                    SeenDeacrease();
                }
            }

        }
        else
        {
            while (true)
            {
                yield return wait;

                lim = WalkRotLimit;
                if (wasSeen == false)
                {
     
                    if (lookAtWaypoint)
                    {
                        atWaypoint = true;
                        lim = WaypointRotLimit;
                        rotAmount = rotTimes;
                        navMeshAgent.speed = 0;
                        gameObject.transform.rotation = transform.parent.rotation;
                        rot = 0;
                        yield return new WaitUntil(() => rotAmount == 0);
                    }

                    navMeshAgent.speed = WalkSpeed;

                    PositionCalculation();
                    atWaypoint = false;
                    
                }
                else
                {

                    SeenDeacrease();
                }
            }

            
            
        }
    }
    
    
    private void PositionCalculation ()
    {
        
        if (pos!= Vector3.zero)
        {
            pos=Vector3.zero; 
            navMeshAgent.SetDestination (waypoints[m_CurrentWaypointIndex].position); 
            navMeshAgent.speed = WalkSpeed;
            
        }
        else
        {
            m_CurrentWaypointIndex = (m_CurrentWaypointIndex + 1) % waypoints.Count;
            navMeshAgent.SetDestination (waypoints[m_CurrentWaypointIndex].position);
        }
    
    }
    private void AddPath(Transform position)
    {
        pos = position.position;
        seen += seenCooldown;
        SeenThisFrame = true;
        wasSeen = true;
        StopCoroutine(behaviourRoutine);
        behaviourRoutine = StartCoroutine(BasicRoutine());
    }
    public void Look(GameObject Object)
    {
       
        vircamera.transform.position = transform.position;
        m_camera.YoloRender(classIndex);    
        
        foreach (var d in StaticMemory.DetectionScript.MainOutput._cached)
         {
             if (d.classIndex == classIndex)
             {
                 if (d.score < treshold)
                 {
                     Debug.LogWarning(d.score);
                     AddPath(Object.transform);
                 }
                 else
                 {
                     CaughtThisFrame = true;
                     if (CaughtThisFrame && CaughtLastFrame)
                     {
                         seen = seenLimit;
                         restart = true;
                         navMeshAgent.speed = 0;
                         _game.OnNext(false);
                     }
                 }
             }
         }
        CaughtLastFrame = CaughtThisFrame;
        CaughtThisFrame = false;
        
    }
    private void FixedUpdate()
    {
        Rotate();
        
        _animationBlend = Mathf.Lerp(_animationBlend, navMeshAgent.speed, Time.deltaTime * navMeshAgent.acceleration);
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
        
        
    }
    private void Rotate()
    {
        if(!shouldRotate || restart) return;
        if (rot > lim || rot < -lim)
        {
            rotSpeed = -rotSpeed;
        }
        rot += rotSpeed;
        if (rot == 0  && atWaypoint)
            rotAmount -= 1;
        gameObject.transform.Rotate(0, rotSpeed, 0);
    }
    private void SeenDeacrease()
    {
        if (!SeenThisFrame)
        {
            if (seen > 0)
            {
                seen -= seenCooldown;
            }
            else
            {
                
                seen = 0;
                wasSeen = false;
                lim = WalkRotLimit;
    
                if (thinkingRoutine != null)
                {
                    if (!restart)
                    {
                        StopCoroutine(thinkingRoutine);
                        thinkingRoutine = StartCoroutine(Thinking());
                    }
                }
            }
        }
        SeenThisFrame = false;
    }

    public IDisposable Subscribe(IObserver<bool> observer)
    {
        _game = observer;
        return null;
    }
}
