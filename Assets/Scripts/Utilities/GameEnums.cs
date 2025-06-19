// GameEnums.cs

public enum GamePhase
{
    Idle,
    TunnelDrawing,
    PathSelection,
    Combat
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
