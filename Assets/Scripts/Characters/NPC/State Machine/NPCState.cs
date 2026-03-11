using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCState 
{
    protected NPC npc;
    protected NPCStateMachine npcStateMachine;

    public NPCState(NPC npc, NPCStateMachine nPCStateMachine)
    {
        this.npc = npc;
        this.npcStateMachine = nPCStateMachine;
    } 

    public virtual void EnterState() {}
    public virtual void ExitState() {}
    public virtual void FrameUpdate() {}
    public virtual void PhysicsUpdate() {}
    public virtual void AnimationTriggerEvent(NPC.AnimationTriggerType triggerType) {}
}
