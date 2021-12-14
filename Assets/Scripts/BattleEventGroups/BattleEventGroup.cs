using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleEventGroup
{
    int _priority;
    public int Priority { get { return _priority; } }

    public BattleEventGroup(int priority)
    {
        _priority = priority;
    }

    List<BattleEvent> _battleEvents = new List<BattleEvent>();
    List<BattleEvent> _newBattleEvents = new List<BattleEvent>();
    List<BattleEvent> _endedBattleEvents = new List<BattleEvent>();

    public void Run()
    {
        foreach (var battleEvent in _battleEvents)
        {
            battleEvent.Run();
            if (battleEvent._ended)
            {
                _endedBattleEvents.Add(battleEvent);
            }
        }
        foreach (var battleEvent in _endedBattleEvents)
        {
            _battleEvents.Remove(battleEvent);
        }
        _endedBattleEvents.Clear();
        _battleEvents.AddRange(_newBattleEvents);
        _newBattleEvents.Clear();
    }

    public void AddBattleEvent(BattleEvent battleEvent)
    {
        _newBattleEvents.Add(battleEvent);
    }

    public bool IsComplete()
    {
        return _battleEvents.Count == 0 && _newBattleEvents.Count == 0;
    }

    override public string ToString()
    {
        string s = "";
        for (int i = 0; i < _battleEvents.Count; i++)
        {
            s += _battleEvents[i].GetType() +"\n";
        }
        return s;
    }
}
