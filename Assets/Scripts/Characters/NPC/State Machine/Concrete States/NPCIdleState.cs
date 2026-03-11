using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NPCIdleState : NPCState
{
    private Vector3 _targetPos;
    private Vector3 _direction;
    public NPCIdleState(NPC npc, NPCStateMachine nPCStateMachine) : base(npc, nPCStateMachine)
    {
    }

    public override void AnimationTriggerEvent(NPC.AnimationTriggerType triggerType)
    {
        base.AnimationTriggerEvent(triggerType);
    }

    public override void EnterState()
    {
        base.EnterState();

        _targetPos = GetRandomPointInCircle();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();


        _direction = (_targetPos - npc.transform.position).normalized;

        npc.MoveEnemy(_direction * npc.RandomMovementSpeed);

        if ((npc.transform.position - _targetPos).sqrMagnitude < 0.01f)
        {
            _targetPos = GetRandomPointInCircle();
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    private Vector3 GetRandomPointInCircle()
    {
        return npc.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * npc.RandomMovementRange;
    }
}
