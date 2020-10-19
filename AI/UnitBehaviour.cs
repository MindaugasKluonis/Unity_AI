using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitBehaviour : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private SpriteRenderer _renderer;

    private bool _isFacingRight = true;

    public void Move(Vector3 velocity)
    {
        transform.Translate(velocity);
    }

    public void Flip(Vector3 velocity)
    {
        if (velocity.x < 0 && _isFacingRight)
        {
            Flip();
        }

        else if (velocity.x > 0 && !_isFacingRight)
        {
            Flip();
        }
    }

    public void Flip()
    {
        _isFacingRight = !_isFacingRight;
        _renderer.flipX = !_renderer.flipX;
    }

    public void SetRunning(bool v)
    {
        _animator.SetBool(GlobalData.GlobalStrings.ANIMATOR_RUNNING, v);
    }
}
