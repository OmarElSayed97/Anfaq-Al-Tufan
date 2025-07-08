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