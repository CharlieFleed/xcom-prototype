using System;
using System.Collections.Generic;
using UnityEngine;

public class Team
{
    public List<Character> Characters = new List<Character>();
    public bool IsActive { set; get; }
    public Player Owner { set; get; }
    public string Name;

    Queue<Character> ReadyCharacters = new Queue<Character>();

    /// <summary>
    /// Populates ready characters and calls StartTurn for all characters.
    /// </summary>
    public void StartTurn()
    {
        IsActive = true;
        foreach (Character character in Characters)
        {
            character.StartTurn();
            ReadyCharacters.Enqueue(character);
        }
    }

    public void EndTurn()
    {
        IsActive = false;
        ReadyCharacters.Clear();
    }

    public void RotateReadyCharacters()
    {
        Character character = ReadyCharacters.Dequeue();
        ReadyCharacters.Enqueue(character);
    }

    /// <summary>
    /// Returns null if there are no ready characters left.
    /// </summary>
    /// <returns></returns>
    public Character GetFirstReadyCharacter()
    {
        //Debug.Log($"ReadyCharacters.Count: {ReadyCharacters.Count}");
        while (ReadyCharacters.Count > 0)
        {
            if (ReadyCharacters.Peek().NumActions > 0 && !ReadyCharacters.Peek().GetComponent<Health>().IsDead)
            {
                return ReadyCharacters.Peek();
            }
            else
            {
                //Debug.Log($"ReadyCharacters.Peek().NumActions: {ReadyCharacters.Peek().NumActions} ReadyCharacters.Peek().GetComponent<Health>().IsDead: {ReadyCharacters.Peek().GetComponent<Health>().IsDead}");
                ReadyCharacters.Dequeue();
            }
        }
        return null;
    }
}