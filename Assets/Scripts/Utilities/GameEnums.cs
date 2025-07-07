// Game State enum
public enum GamePhase
{
    Idle,              // No input, waiting for player or system event
    TunnelDrawing,     // Player draws a tunnel path (underground planning)
    TunnelNavigation,  // Player auto-travels along the tunnel
    Combat,            // Player can swipe to attack enemies          
    GameWinState
}

// Player enums
public enum PlayerLocation
{
    Underground,
    AboveGround
}

public enum AnimationState
{
    None,
    Traveling,
    Bursting,
    Hovering,
    Attacking
}


// Enemy enums
public enum EnemyType
{
    Normal,        // Basic enemy with simple behavior
    Advanced,      // Smarter or faster enemy
    Elite,         // Tougher and more dangerous enemy
    Boss           // High HP and unique attacks
}

public enum EnemyState
{
    Idle,          // Waiting or passive
    Patrolling,    // Moving along a set path
    Chasing,       // Following the player
    Attacking,     // Engaged in combat
    Retreating,    // Fleeing or repositioning
    Dead           // No longer active
}

public enum EnemyAttackType
{
    Melee,         // Close-range attack
    Ranged,        // Projectiles or distance-based attack
    AoE,           // Area-of-effect damage
    SuicideBomber  // Explodes on contact
}

public enum EnemyAnimationState
{
    Idle,
    Walk,
    Attack,
    Hit,
    Die
}
