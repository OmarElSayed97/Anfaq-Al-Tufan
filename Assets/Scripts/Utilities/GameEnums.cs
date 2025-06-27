public enum GamePhase
{
    Idle,              // No input, waiting for player or system event
    TunnelDrawing,     // Player draws a tunnel path (underground planning)
    TunnelNavigation,  // Player auto-travels along the tunnel
    Hovering,          // Player hovers above ground, awaiting input
    Combat             // Player can swipe to attack enemies          
}

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
    Hovering
}
