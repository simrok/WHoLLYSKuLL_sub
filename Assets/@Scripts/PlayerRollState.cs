using System.Collections;
using UnityEngine;

public class PlayerRollState : PlayerState
{
    private float elapsed;
    private Vector3 rollDir;

    // 구르기 + 후딜레이가 끝났는지. 상태 전환은 Player가 이 값을 보고 결정
    // 구르기 종료 후 아주 짧은 재입력 불가 구간 0.1~0.15초
    public bool IsDone => elapsed >= context.rollDuration + context.rollRecoveryTime;

    public PlayerRollState(Player player) : base(player) { }

    public override void OnStateEnter()
    {
        elapsed = 0f;
        rollDir = owner.transform.forward;
        context.PlayerMovement.canMove = false;

        //Vector3 startPos = player.transform.position;

        // 현재 바라보고 있는 방향으로 전진하면서 구르는 애니메이션
        context.Animator.SetTrigger("Roll");
    }

    public override void OnStateUpdate() 
    { 
        context.playerShield.SetActive(context.isInvincible);
    }

    public override void OnStateFixedUpdate()
    {
       if (elapsed < context.rollDuration)
        {
            // 무적 프레임은 앞쪽 60~70%. 
            context.isInvincible = elapsed < context.rollDuration * context.invincibleRatio;

            // 지속시간 0.35~0.45초.
            float speed = context.rollMoveSpeed * context.rollSpeedCurve.Evaluate(elapsed / context.rollDuration);
            context.PlayerMovement.ForceMove(rollDir, speed);
        }
        else
        {
            // 구르기 이동이 끝나면 제자리 (수평 0, 위로 가는 속력 제거, 중력 제거)
            context.PlayerMovement.ForceMove(Vector3.zero, 0f);
        }
        elapsed += Time.fixedDeltaTime;

    }
    public override void OnStateExit()
    {
        context.isInvincible = false;   // 무적 상태 끄기
        context.playerShield.SetActive(false);  // 구르기 도중 상태가 바껴도 남지 않도록
        context.PlayerMovement.canMove = true;  // 움직일 수 있음
    }
}
