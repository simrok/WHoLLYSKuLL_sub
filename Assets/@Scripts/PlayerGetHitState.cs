using UnityEngine;
using UnityEngine.Playables;

public class PlayerGetHitState : PlayerState
{
    public PlayerGetHitState(Player player) : base(player) { }

    private float elapsed;
    private const float stunTime = 0.4f;   // 피격 애니 길이에 맞추기

    public bool IsDone => elapsed >= stunTime;

    public override void OnStateEnter()
    {
        elapsed = 0f;
        context.PlayerMovement.canMove = false;
        context.isInvincible = true;          // 연속 피격 방지
        context.Animator.SetTrigger("GetHit"); // Animator 트리거 이름에 맞게
    }
    public override void OnStateUpdate() { }

    public override void OnStateFixedUpdate()
    {
        context.PlayerMovement.ForceMove(Vector3.zero, 0f);
        elapsed += Time.fixedDeltaTime;
    }

    public override void OnStateExit()
    {
        context.isInvincible = false;
        context.PlayerMovement.canMove = true;
    }
}
