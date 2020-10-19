using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager
{
    protected GridController _gridController;

    //temp
    protected UnitBehaviour _unitBehaviourPrefab;

    public UnitManager(GridController gridController, UnitBehaviour unitBehaviourPrefab)
    {
        _unitBehaviourPrefab = unitBehaviourPrefab;
        _gridController = gridController;
    }

    public virtual UnitController GetUnitController(int index)
    {
        return null;
    }

    public virtual void MoveUnit(int unitIndex, Vector3 location)
    {
        
    }

    public virtual void OnUnitPathFail(GridTile current, GridTile nextTile, UnitState nextState)
    {
        
    }

    public virtual bool CreateUnitController(Vector3 position)
    {
        return default;
    }
}
