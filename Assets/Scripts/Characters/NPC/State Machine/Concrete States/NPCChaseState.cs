using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCChaseState : NPCState
{
    public NPCChaseState(NPC npc, NPCStateMachine nPCStateMachine) : base(npc, nPCStateMachine)
    {
    }

    public override void AnimationTriggerEvent(NPC.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }
}
