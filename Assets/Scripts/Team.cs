using System;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public List<Unit> Units = new List<Unit>();
    public bool IsActive { set; get; }
    public MyGamePlayer Owner { set; get; }
    public int Id;
    public string Name;

    Queue<Unit> ReadyUnits = new Queue<Unit>();

    /// <summary>
    /// Populates ready units and calls StartTurn for all units.
    /// </summary>
    public void StartTurn()
    {
        IsActive = true;
        foreach (Unit unit in Units)
        {
            unit.StartTurn();
            ReadyUnits.Enqueue(unit);
        }
    }

    public void EndTurn()
    {
        IsActive = false;
        ReadyUnits.Clear();
    }

    public void RotateReadyUnits()
    {
        Unit unit = ReadyUnits.Dequeue();
        ReadyUnits.Enqueue(unit);
    }

    /// <summary>
    /// Returns null if there are no ready units left.
    /// </summary>
    /// <returns></returns>
    public Unit GetFirstReadyUnit()
    {
        //Debug.Log($"ReadyUnits.Count: {ReadyUnits.Count}");
        while (ReadyUnits.Count > 0)
        {
            if (ReadyUnits.Peek().NumActions > 0 && !ReadyUnits.Peek().GetComponent<Health>().IsDead)
            {
                return ReadyUnits.Peek();
            }
            else
            {
                //Debug.Log($"ReadyUnits.Peek().NumActions: {ReadyUnits.Peek().NumActions} ReadyUnits.Peek().GetComponent<Health>().IsDead: {ReadyUnits.Peek().GetComponent<Health>().IsDead}");
                ReadyUnits.Dequeue();
            }
        }
        return null;
    }

    public void RemoveUnit(Unit unit)
    {
        if (Units.Contains(unit))
            Units.Remove(unit);
        // rebuild ReadyUnits queue
        List<Unit> queue = new List<Unit>();
        while (ReadyUnits.Count > 0)
        {
            if (ReadyUnits.Peek() != unit)
            {
                queue.Add(ReadyUnits.Peek());
            }
            ReadyUnits.Dequeue();
        }
        foreach (var c in queue)
        {
            ReadyUnits.Enqueue(c);
        }
    }

    public bool IsDefeated()
    {
        foreach (var unit in Units)
        {
            if (!unit.GetComponent<Health>().IsDead)
            {
                return false;
            }
        }
        return true;
    }
}