using System;
using System.Collections.Generic;

public class Team
{
    public List<Character> Characters = new List<Character>();
    Queue<Character> ReadyCharacters = new Queue<Character>();
    public bool IsActive { set; get; }

    /// <summary>
    /// Populates ready characters and calls StartTurn for all characters.
    /// </summary>
    public void StartTurn()
    {
        IsActive = true;
        ReadyCharacters.Clear();
        foreach (Character character in Characters)
        {
            character.StartTurn();
            ReadyCharacters.Enqueue(character);
        }
    }

    public void EndTurn()
    {
        IsActive = false;
    }

    public void RotateReadyCharacters()
    {
        Character character = ReadyCharacters.Dequeue();
        ReadyCharacters.Enqueue(character);
    }

    public Character GetFirstReadyCharacter()
    {
        while (ReadyCharacters.Count > 0)
        {
            if (ReadyCharacters.Peek().NumActions > 0 && !ReadyCharacters.Peek().GetComponent<Health>().IsDead)
            {
                return ReadyCharacters.Peek();
            }
            else
            {
                ReadyCharacters.Dequeue();
            }
        }
        return null;
    }
}