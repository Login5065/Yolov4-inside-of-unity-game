using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour , IObservable<bool>
{
    public bool picked = false;
    private IObserver<bool> _game;
    private void OnTriggerEnter(Collider other)
    {
        if (picked) return;
        if (other.CompareTag("Player"))
        {
            
            picked = true;
            _game.OnNext(true);
            GetComponent<MeshFilter>().mesh = null;

        }
    }
    

    public IDisposable Subscribe(IObserver<bool> observer)
    {
        _game = observer;
        return null;
    }
}
