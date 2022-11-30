using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;

public class Game : MonoBehaviour , IObserver<bool> 
{
    [SerializeField] private List<Key> _keys;
    [SerializeField] private List<AI_Behaviour> _enemies;
    [SerializeField] private int _keysPicked=0;
    [SerializeField] private GameEnd _gameEnd;
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private StarterAssetsInputs _input;

    
        // Start is called before the first frame update
    void Start()
    {
        
        foreach (var VARIABLE in _keys)
        {
            
            VARIABLE.Subscribe(this);

        }

        foreach (var VARIABLE in _enemies)
        {
            VARIABLE.Subscribe(this);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
       
        if (_input.fov)
        {


            foreach (var VARIABLE in _enemies)
        {
            if (IsInView(PlayerCamera.gameObject,VARIABLE.gameObject))
            {
                VARIABLE.FOV.ShowField = true;
            }
            else
            {
                VARIABLE.FOV.ShowField = false;
            }
        }
        
        }

        else
        {
            foreach (var VARIABLE in _enemies)
            {
                VARIABLE.FOV.ShowField = false;
            }
        }

    }

    public void OnCompleted()
    {
        if (_keysPicked >= _keys.Count)
        {
            Debug.LogWarning("Picked All Keys. Leave through exit");
            _gameEnd.m_IsPlayerAtExit = true;

        }
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext(bool value)
    {
        if (value)
        {
            _keysPicked++;
            if (_keysPicked >= _keys.Count)
            {
                Debug.LogWarning("Picked All Keys. Leave through exit");
                _gameEnd.m_IsPlayerAtExit = true;

            }
        }
        else
        {
            _gameEnd.m_IsPlayerCaught = true;

        }
    }

    private bool IsInView(GameObject origin, GameObject toCheck)
    {
        Vector3 pointOnScreen = PlayerCamera.WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);
 
        //Is in front
        if (pointOnScreen.z < 0)
        {
            //Debug.Log("Behind: " + toCheck.name);
            return false;
        }
 
        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
            (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            //Debug.Log("OutOfBounds: " + toCheck.name);
            return false;
        }
 
        RaycastHit hit;
        if (Physics.Linecast(PlayerCamera.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        {
            if (hit.transform.name != toCheck.name)
            {
                //Debug.Log(toCheck.name + " occluded by " + hit.transform.name);
                return false;
            }
        }
        return true;
    }

}
