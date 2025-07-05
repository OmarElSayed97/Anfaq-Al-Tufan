using UnityEngine;

public abstract class GamePhaseState
{
    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();
}

public class IdleState : GamePhaseState
{
    public override void OnEnter() { Debug.Log("[GamePhaseState] Enter Idle"); }
    public override void OnUpdate() { }
    public override void OnExit() { Debug.Log("[GamePhaseState] Exit Idle"); }
}

public class TunnelNavigationState : GamePhaseState
{
    private TunnelNavigator tunnelNavigator;

    public TunnelNavigationState(TunnelNavigator navigator) => this.tunnelNavigator = navigator;

    public override void OnEnter()
    {
        Debug.Log("[GamePhaseState] Enter TunnelNavigation");
        tunnelNavigator.Initialize();
        // Optionally, you can reset or prepare the navigator here
    }

    public override void OnUpdate() => tunnelNavigator.OnUpdate();
    
    public override void OnExit()
    {
        Debug.Log("[GamePhaseState] Exit TunnelNavigation");
        tunnelNavigator.FinalizeNavigation();
        // Optionally, clean up or reset navigator state here
    }
}

public class TunnelDrawingState : GamePhaseState
{
    private TunnelDrawer tunnelDrawer;

    public TunnelDrawingState(TunnelDrawer drawer) => this.tunnelDrawer = drawer;

    public override void OnEnter()
    {
        Debug.Log("[GamePhaseState] Enter TunnelDrawing");
        tunnelDrawer.OnEnter();
    }

    public override void OnUpdate() => tunnelDrawer.OnUpdate();

    public override void OnExit()
    {
        Debug.Log("[GamePhaseState] Exit TunnelDrawing");
        tunnelDrawer.OnExit();
    }
}

public class CombatState : GamePhaseState
{
    private CombatHandler combatHandler;

    public CombatState(CombatHandler handler) => this.combatHandler = handler;

    public override void OnEnter() {
        Debug.Log("[GamePhaseState] Enter Combat");
        combatHandler.OnEnter();
    }

    public override void OnUpdate() => combatHandler.OnUpdate();

    public override void OnExit() {
        Debug.Log("[GamePhaseState] Exit Combat");
        combatHandler.OnExit();
    }
}
