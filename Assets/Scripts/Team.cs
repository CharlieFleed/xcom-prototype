using System;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public List<Unit> Members = new List<Unit>();
    public bool IsActive { set; get; }
    public MyGamePlayer Owner { set; get; }
    public int Id;
    public string Name;

    Queue<Unit> ReadyMembers = new Queue<Unit>();

    public bool IsAI { set; get; }

    /// <summary>
    /// Populates ready members and calls StartTurn for all members.
    /// </summary>
    public void StartTurn()
    {
        IsActive = true;
        foreach (Unit member in Members)
        {
            member.GetComponent<ActionsController>().StartTurn();
            ReadyMembers.Enqueue(member);
        }
    }

    public void EndTurn()
    {
        IsActive = false;
        ReadyMembers.Clear();
    }

    public void RotateReadyMembers()
    {
        Unit member = ReadyMembers.Dequeue();
        ReadyMembers.Enqueue(member);
    }

    /// <summary>
    /// Returns null if there are no ready members left.
    /// </summary>
    /// <returns></returns>
    public Unit GetFirstReadyMember()
    {
        //Debug.Log($"ReadyMembers.Count: {ReadyMembers.Count}");
        while (ReadyMembers.Count > 0)
        {
            if (ReadyMembers.Peek().GetComponent<ActionsController>().NumActions > 0 && !ReadyMembers.Peek().GetComponent<Health>().IsDead)
            {
                return ReadyMembers.Peek();
            }
            else
            {
                //Debug.Log($"ReadyUnits.Peek().NumActions: {ReadyUnits.Peek().NumActions} ReadyUnits.Peek().GetComponent<Health>().IsDead: {ReadyUnits.Peek().GetComponent<Health>().IsDead}");
                ReadyMembers.Dequeue();
            }
        }
        return null;
    }

    public void RemoveMember(Unit member)
    {
        if (Members.Contains(member))
            Members.Remove(member);
        // rebuild ReadyUnits queue
        List<Unit> queue = new List<Unit>();
        while (ReadyMembers.Count > 0)
        {
            if (ReadyMembers.Peek() != member)
            {
                queue.Add(ReadyMembers.Peek());
            }
            ReadyMembers.Dequeue();
        }
        foreach (var c in queue)
        {
            ReadyMembers.Enqueue(c);
        }
    }

    public bool IsDefeated()
    {
        foreach (var member in Members)
        {
            if (!member.GetComponent<Health>().IsDead)
            {
                return false;
            }
        }
        return true;
    }
}