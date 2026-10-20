using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonsterAttackState : BossMonsterState
{
    public BossMonsterAttackState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }

    private float turnTime = 0.2f;

    private BossSkill skill;
    private Coroutine attackSequence;

    private bool isDone;
    public bool IsDone => isDone;

    public override void OnStateEnter()
    {
        //Debug.Log("attackState 시작");
        isDone = false;
        skill = owner.CurrentSkill;
        // ApplyMove로 인한 linearVelocity의 영향 없애기
        owner.StopMove();
        // attackSequnce 코루틴
        attackSequence = owner.StartCoroutine(AttackSequence());
    }

    public override void OnStateUpdate() { }

    public override void OnStateExit()
    {
        if (attackSequence != null) owner.StopCoroutine(attackSequence);
        owner.StopSkill();
        //Debug.Log("attackState 끝");
    }

    public IEnumerator AttackSequence()
    {
        // 1. 회전
        // 플레이어 방향으로 회전
        Vector3 toPlayer = owner.playerTransform.position - owner.Rb.position;
        toPlayer.y = 0;
        Vector3 dir = toPlayer.normalized;

        // 보스의 몸을 통과할 수 있으므로 예외처리
        if (dir != Vector3.zero)
        {
            Quaternion startRot = owner.Rb.rotation;
            Quaternion targetRot = Quaternion.LookRotation(toPlayer);

            float t = 0f;
            WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();
            while (t < turnTime)
            {
                t += Time.fixedDeltaTime;
                Quaternion nextRotation = Quaternion.Slerp(startRot, targetRot, t / turnTime);
                owner.Rb.MoveRotation(nextRotation);
                yield return waitFixed;
            }
        }
        // 2. 스킬 시전하기전 대기: skill.Data.castDelay
        yield return new WaitForSeconds(skill.Data.castDelay);

        // 3. 스킬 시전
        owner.StartSkill(skill);        // 공격 스킬을 시전 -> owner.IsSkillRunning == true가 됨
        yield return new WaitUntil(() => !owner.IsSkillRunning);    // owner.RunSkill이 돌고 있을 때 IsSkillRunning이 false가 되기 전까지 대기
        isDone = true;
    }
}
