using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittable
{
    void Hit(CharacterObject attacker, int projectileIndex, int atkIndex);
}
public interface ISpawnable
{
    void Spawn(int index);
    void DeSpawn();
}
