# FPS Utility AI — Zombie FPS with an AI Director

Linear zombie FPS (start safe room → 5 zones → end safe room). Nothing spawns on its own:
an **AI Director** decides when and where enemies, resources and world events appear.

## Setup

1. Open the project in Unity 6.
2. `Tools > Rebuild Game Scene` (generates `Assets/Scenes/Main.unity`, prefabs and materials).
3. Open `Assets/Scenes/Main.unity` and press Play.

Controls: `WASD/ZQSD` move · `Shift` sprint · `Space` jump · `LMB` fire · `R` reload · `1-3` / wheel weapons ·
`G` grenade · `F` flashlight · `F1` debug overlay · `Esc` pause.

## Plugging a Director

Implement `IAIDirector` on a MonoBehaviour, add it to the **AI Director** GameObject and disable/remove `EmptyDirector`.

```csharp
using ZombieFPS;
using UnityEngine;

public class MyDirector : MonoBehaviour, IAIDirector
{
    public void OnGameStart(DirectorContext context, DirectorActions actions)
    {
        context.Events.PlayerDamaged += amount => { /* ... */ };
    }

    public void Tick(DirectorContext context, DirectorActions actions, float deltaTime)
    {
        // read context -> score options -> call actions
    }
}
```

`Tick` is called every `DirectorRunner.TickInterval` seconds (default 0.25 s) while the game is running.

## Context (inputs)

| Member | Content |
|---|---|
| `GameTime`, `State` | Run time (pauses excluded), game state |
| `Player` | Position, Forward, Speed, IsSprinting, Health / MaxHealth / HealthRatio, CurrentWeapon, AmmoInMagazine, ReserveAmmo (-1 = infinite), AmmoRatio, Grenades, HasWeapon(), Progress (0..1), DistanceAlongPath, CurrentZone, IsInSafeRoom |
| `Enemies` | Living enemies: Type, Position, DistanceToPlayer, HealthRatio, IsAlerted, IsVisibleToPlayer, Progress, Age |
| `CountEnemies()`, `CountEnemies(type)`, `CountEnemiesWithin(r)`, `CountAlertedEnemies()` | Enemy counts |
| `MaxAliveEnemies`, `EnemyDamageMultiplier`, `EnemySpeedMultiplier` | Limits and current difficulty modifiers |
| `Pickups`, `CountPickups(type)`, `MaxPickups` | Items lying in the level |
| `EnemySpawnPoints`, `PickupSpawnPoints` | Spawn locations: Zone, Progress, DistanceToPlayer, IsVisibleToPlayer, IsAheadOfPlayer, IsOccupied, TimeSinceLastSpawn |
| `ZoneCount`, `GetZoneName(i)`, `PathLength`, `GetProgress(pos)`, `IsVisibleToPlayer(pos)` | Level helpers |
| `WorldEvents` | IsActive(type), GetRemainingTime(type), TimeSinceLastTrigger(type), TriggerCount(type), PendingHordeEnemies |
| `Stats` | Totals (kills, headshots, shots, accuracy, damage taken/dealt, healing, pickups), sliding windows (`DamageTakenInLast(s)`, `KillsInLast(s)`, `AccuracyInLast(s)`…), `TimeSinceDamageTaken`, `TimeSinceKill`, `TimeSinceLastCombat`… |
| `Events` | PlayerDamaged, PlayerHealed, EnemySpawned, EnemyKilled, PickupCollected, ShotFired, ZoneEntered, WorldEventStarted, WorldEventEnded, GameOver |

## Actions (outputs)

| Method | Effect |
|---|---|
| `SpawnEnemy(type, point, alerted)` / `SpawnEnemies(type, count, point, alerted)` | Spawn on an enemy spawn point |
| `DespawnEnemy(enemy)` | Remove an enemy (no kill counted) |
| `AlertEnemy(enemy)` / `AlertAllEnemies()` | Make enemies chase the player |
| `SetEnemyDamageMultiplier(x)` / `SetEnemySpeedMultiplier(x)` | Global difficulty modifiers |
| `SpawnPickup(type, point)` / `RemovePickup(pickup)` | Resources on pickup spawn points |
| `TriggerHorde(size)` | Wave of alerted zombies from hidden spawn points |
| `SupplyDrop()` | Health + ammo + grenade a few meters ahead |
| `StartFog(duration)` / `StartBlackout(duration)` | Negative timed events |
| `GiveAdrenaline(duration)` | Positive timed event (speed, damage, reload) |
| `ShowMessage(text, duration)` | On-screen message |

Every action returns `false` when refused (wrong point kind, occupied point, limit reached, game not running…).

## Content

| Enemy | Role |
|---|---|
| Common | Average zombie, wanders until it sees/hears the player |
| Runner | Fast, fragile |
| Tank | Huge health, heavy hits |
| Bloater | Slow, explodes on contact or death (hurts zombies too) |

| Pickup | Effect |
|---|---|
| Health | +50 HP (ignored at full health) |
| Ammo | +35 % reserve on owned shotgun / rifle |
| Grenade | +1 grenade (max 3) |
| Shotgun / Rifle | New weapon, or ammo if already owned |

Zombies hear gunshots (28 m) and explosions. Stats of enemies, weapons and events are editable on prefabs
(`Assets/Prefabs`) and on the `Game` GameObject (`Spawner`, `WorldEvents`).

## Debug

`F1` shows the full context (left) and the Director panel (right). From your Director:

```csharp
DirectorDebug.Watch("Intensity", intensity);       // text value
DirectorDebug.Plot("Intensity", intensity, 0, 1);  // scrolling graph, one sample per call
DirectorDebug.Log("Relax phase");                   // log line
```

Every successful action is logged automatically. Spawn points, zones and the path are drawn as gizmos in the Scene view.
