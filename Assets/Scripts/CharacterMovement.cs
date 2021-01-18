using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private const float JUMP_AMOUNT = 100f;

    private static CharacterMovement instance;

    public static CharacterMovement GetInstance()
    {
        return instance;
    }

    public event EventHandler OnDied;
    public event EventHandler OnStart;

    private Rigidbody2D charRigidbody2D;
    private State state;

    private enum State
    { 
        Waiting,
        Playing,
        Dead,

    }

    private void Awake() 
    {
        instance = this; 
        charRigidbody2D = GetComponent<Rigidbody2D>();
        charRigidbody2D.bodyType = RigidbodyType2D.Static;
        state = State.Waiting;
    }
    private void Update()
    {
        switch(state)
        {
            default:
            case State.Waiting:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
             {
                state = State.Playing;
                charRigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                Jump();
                if(OnStart != null) OnStart(this, EventArgs.Empty);
             }
                break;
            case State.Playing:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
             {
                Jump();
             }
                break;
            case State.Dead:
                break;
        }
        
    }

    private void Jump()
    {
        charRigidbody2D.velocity = Vector2.up * JUMP_AMOUNT;
        SoundManager.PlaySound(SoundManager.Sound.CharJump);
    }

   private void OnTriggerEnter2D(Collider2D collider) 
    {
         
         if(OnDied != null) OnDied(this, EventArgs.Empty);
         SoundManager.PlaySound(SoundManager.Sound.Lose);

    }
}
