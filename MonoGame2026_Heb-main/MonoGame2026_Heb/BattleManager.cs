using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class BattleManager : IUpdatable
{
    private readonly List<Unit> units = new();

    public bool IsBattleActive { get; private set; }

    public void Start()
    {
    }

    public void RegisterUnit(Unit unit)
    {
        if (unit == null || units.Contains(unit))
            return;

        units.Add(unit);

        // listens to the unit's death event
        unit.Died += OnUnitDied;
    }

    public void StartBattle()
    {
        IsBattleActive = true;

        foreach (Unit unit in units)
        {
            if (unit.IsAlive)
            {
                unit.SetCombatEnabled(true);
            }
        }
    }

    public void Update(GameTime gameTime)
    {
        if (!IsBattleActive)
            return;

        foreach (Unit unit in units)
        {
            if (!unit.IsAlive)
                continue;

            // replaces dead or missing targets
            if (unit.Target == null ||
                !unit.CanTarget(unit.Target))
            {
                Unit closestEnemy =
                    FindClosestEnemy(unit);

                if (closestEnemy != null)
                {
                    unit.SetTarget(closestEnemy);
                }
                else
                {
                    unit.ClearTarget();
                }
            }
        }
    }

    private Unit FindClosestEnemy(Unit unit)
    {
        Unit closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Unit otherUnit in units)
        {
            if (!unit.CanTarget(otherUnit))
                continue;

            float distance =
                Vector2.DistanceSquared(
                    unit.tm.position,
                    otherUnit.tm.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = otherUnit;
            }
        }

        return closestEnemy;
    }

    private void OnUnitDied(Unit deadUnit)
    {
        Console.WriteLine(
            $"{deadUnit.UnitTeam} unit died");
    }
}