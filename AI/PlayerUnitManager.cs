using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitManager : UnitManager
{
    protected List<UnitController> _units;

    public PlayerUnitManager(GridController gridController, UnitBehaviour unitBehaviourPrefab) : base(gridController, unitBehaviourPrefab)
    {
        _units = new List<UnitController>();
    }

    public override bool CreateUnitController(Vector3 position)
    {
        if (_gridController.GetGridTile(position).IsLocked)
            return false;

        UnitBehaviour behaviourInstance = Object.Instantiate(_unitBehaviourPrefab, position + GlobalData.GlobalVectors.POSITION_OFFSET, Quaternion.identity);

        IdleState idleState = new IdleState()
        {
            CurrentTile = _gridController.GetGridTile(position)
        };

        UnitController unit = new UnitController(null, behaviourInstance, idleState);
        idleState.SetParent(unit);
        idleState.OnStateEnter();

        //idleState.CurrentTile.LockTile();

        unit.UnitIndex = _units.Count;
        _units.Add(unit);



        return true;
    }

    public override UnitController GetUnitController(int index)
    {
        return base.GetUnitController(index);
    }

    public override void MoveUnit(int unitIndex, Vector3 location)
    {
        Vector3 startingPosition = _units[unitIndex].GetCurrentTile().LocalLocation;
        List<GridTile> path = _gridController.FindPath(startingPosition, location);
        UnitState nextState = new IdleState(_units[unitIndex]);
        WalkingState state = new WalkingState(_units[unitIndex], nextState, path, OnUnitPathFail);

        _units[unitIndex].ChangeState(state);
    }

    public override void OnUnitPathFail(GridTile current, GridTile nextTile, UnitState nextState)
    {
        Vector3 startingPosition = current.LocalLocation;
        List<GridTile> path = _gridController.FindPath(startingPosition, nextTile.LocalLocation);

        WalkingState moveState = new WalkingState(nextState.Parent, nextState, path, OnUnitPathFail);

        nextState.Parent.ChangeState(moveState);
    }
}
