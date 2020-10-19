using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController
{
    private UnitData _unitData;

    public UnitBehaviour Behaviour { private set; get; }

    private UnitState _currentState;

    public int UnitIndex { set; get; }
    public bool IsBusy { set; get; } = false;
    public UnitState NextLoggedState { set; get; }

    public UnitController(UnitData data, UnitBehaviour behaviour, UnitState startingState)
    {
        _unitData = data;
        Behaviour = behaviour;
        _currentState = startingState;
    }

    public void ChangeState(UnitState nextState)
    {
        if (!IsBusy)
        {
            GridTile current = _currentState.CurrentTile;

            _currentState.OnStateExit();

            _currentState = nextState;
            NextLoggedState = null;

            _currentState.CurrentTile = current;
            _currentState.OnStateEnter();
            
        }

        else
        {
            NextLoggedState = nextState;
        }
    }

    public GridTile GetCurrentTile()
    {
        return _currentState.CurrentTile;
    }
}

public class UnitState
{
    public UnitState NextState { get; set; }
    public UnitController Parent { get; set; }
    public GridTile CurrentTile { set; get; }

    public UnitState(UnitController controller)
    {
        Parent = controller;
    }

    public virtual void OnStateEnter()
    {

    }

    public virtual void OnStateExit()
    {

    }

    public void SetNextState(UnitState nextState)
    {
        NextState = nextState;
    }
}

public class IdleState : UnitState
{
    public IdleState() : base(null) { }

    public IdleState(UnitController parent) : base(parent) { }

    public void SetParent(UnitController parent)
    {
        Parent = parent;
    }

    public override void OnStateEnter()
    {
        Parent.IsBusy = false;

        if(NextState != null)
        {
            Parent.ChangeState(NextState);
        }
    }

    public override void OnStateExit()
    {

    }
}

public class CallbackUnitState<T> : UnitState
{
    public delegate void Callback(T item);
    private Callback _callback;
    private T _item;

    public CallbackUnitState(UnitController controller, Callback callback, T item) : base(controller)
    {
        _callback += callback;
        _item = item;
    }

    public override void OnStateEnter()
    {
        _callback.Invoke(_item);
    }

    public override void OnStateExit()
    {

    }
}

public class WalkingState : UnitState
{
    private List<GridTile> _path;
    private OnUnitPathFail _callback;

    public WalkingState(UnitController parent, UnitState nextState, List<GridTile> path, OnUnitPathFail callback) : base(parent)
    {

        NextState = nextState;
        _path = path;
        _callback = callback;
    }

    public WalkingState(UnitController parent, List<GridTile> path) : base(parent)
    {
        _path = path;
    }

    public GridTile GetLastPathPosition()
    {
        return _path[_path.Count - 1];
    }

    public override void OnStateEnter()
    {
        Parent.IsBusy = true;
        Parent.Behaviour.SetRunning(true);
        Parent.Behaviour.StartCoroutine(Move());
    }

    public override void OnStateExit()
    {
        Parent.Behaviour.SetRunning(false);
        Parent.Behaviour.StopAllCoroutines();
    }

    public IEnumerator Move()
    {
        Vector3 pos;
        GridTile targetTile;

        for (int i = 0; i < _path.Count; i++)
        {
            //CurrentTile.UnlockTile();

            if (Parent.NextLoggedState != null)
            {
                Parent.IsBusy = false;
                Parent.ChangeState(Parent.NextLoggedState);
                
                yield break;
            }

            pos = _path[i].LocalLocation + new Vector3(0.5f, 0.5f, 0f);

            targetTile = _path[i];

            if (targetTile.IsLocked)
            {
                if(targetTile == GetLastPathPosition())
                {
                    Parent.IsBusy = false;
                    Parent.ChangeState(NextState);
                    yield break;
                }

                //targetTile.LockTile(Color.red);

                //path is blocked, need to recalculate path
                Parent.IsBusy = false;
                _callback.Invoke(CurrentTile, GetLastPathPosition(), NextState);
                yield break;
            }

            //targetTile.LockTile();
            CurrentTile = targetTile;

            while (Parent.Behaviour.transform.position != pos)
            {
                Vector3 nextPos = Vector2.MoveTowards(Parent.Behaviour.transform.position, pos, 6f * Time.deltaTime);
                Vector3 velocity = nextPos - Parent.Behaviour.transform.position;
                Parent.Behaviour.transform.position = nextPos;

                Parent.Behaviour.Flip(velocity);

                yield return null;
            }
        }

        Parent.IsBusy = false;
        Parent.ChangeState(NextState);
    }


}