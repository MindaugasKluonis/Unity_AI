using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnitManager : UnitManager
{
    private List<AIUnitController> _units;

    public AIUnitManager(GridController gridController, UnitBehaviour unitBehaviourPrefab):base(gridController, unitBehaviourPrefab)
    {
        _units = new List<AIUnitController>();
    }

    public override bool CreateUnitController(Vector3 position)
    {
        if (_gridController.GetGridTile(position).IsLocked)
            return false;

        UnitBehaviour behaviourInstance = UnityEngine.Object.Instantiate(_unitBehaviourPrefab, position + GlobalData.GlobalVectors.POSITION_OFFSET, Quaternion.identity);

        IdleState idleState = new IdleState()
        {
            CurrentTile = _gridController.GetGridTile(position)
        };

        AIUnitController unit = new AIUnitController(null, behaviourInstance, idleState);
        idleState.SetParent(unit);
        idleState.OnStateEnter();

        //idleState.CurrentTile.LockTile();

        unit.UnitIndex = _units.Count;
        _units.Add(unit);



        return true;
    }

    public void ActivateUnit(int index)
    {
        AIUnitController controller = _units[index];

        int count = controller.GetLocationCount();

        if (count == 0)
        {
            
        }

        else if (count > 0)
        {
            MoveUnit(controller, controller.GetCurrentLocation());
        }
    }

    public void MoveUnit(AIUnitController unit, Vector3 location, int checkCount = 0)
    {
        Vector3 startingPosition = unit.GetCurrentTile().LocalLocation;
        List<GridTile> path = _gridController.FindPath(startingPosition, location);

        int count = checkCount + 1;
        int totalLocations = unit.GetLocationCount();

        if (path.Count > 0)
        {

            UnitState nextState = new CallbackUnitState<AIUnitController>(unit, OnUnitPathFinished, unit as AIUnitController);
            WalkingState state = new WalkingState(unit, nextState, path, OnUnitPathFail);

            unit.ChangeState(state);
            return;
        }

        else if(count < totalLocations)
        {
            unit.SetNextLocation();
            MoveUnit(unit, unit.GetCurrentLocation(), count);
        }
    }

    public void OnUnitPathFinished(AIUnitController unit)
    {
        Vector3 location = unit.GetCurrentLocation();
        unit.SetNextLocation();
        if (location != unit.GetCurrentLocation())
        {
            MoveUnit(unit, unit.GetCurrentLocation());
        }

        else
        {
            IdleState state = new IdleState(unit);
            unit.ChangeState(state);
        }
    }

    public override void OnUnitPathFail(GridTile current, GridTile nextTile, UnitState nextState)
    {
        Vector3 startingPosition = current.LocalLocation;
        List<GridTile> path = _gridController.FindPath(startingPosition, nextTile.LocalLocation);

        WalkingState moveState = new WalkingState(nextState.Parent, nextState, path, OnUnitPathFail);

        nextState.Parent.ChangeState(moveState);
    }

    public AIUnitController GetAIUnitController(int index)
    {
        return _units[index];
    }
}

public class AIUnitController : UnitController
{
    private List<Vector3> targetLocations;
    private int currentTargetLocation;

    public AIUnitController(UnitData data, UnitBehaviour behaviour, UnitState startingState) : base(data, behaviour, startingState)
    {
        targetLocations = new List<Vector3>();
        currentTargetLocation = 0;
    }

    public void AddTargetLocation(Vector3 location)
    {
        targetLocations.Add(location);
    }

    public Vector3 GetLocationAt(int index)
    {
        if(index <= targetLocations.Count)
        {
            return targetLocations[index];
        }

        return Vector3.zero;
    }

    public void SetNextLocation()
    {
        currentTargetLocation = (currentTargetLocation + 1 + targetLocations.Count) % targetLocations.Count;
    }

    public void SetPreviousLocation()
    {
        currentTargetLocation = (currentTargetLocation - 1 + targetLocations.Count) % targetLocations.Count;
    }

    public int GetLocationCount()
    {
        return targetLocations.Count;
    }

    public Vector3 GetCurrentLocation()
    {
        if(targetLocations.Count > 0)
            return targetLocations[currentTargetLocation];

        return Vector3.zero;
    }
}
