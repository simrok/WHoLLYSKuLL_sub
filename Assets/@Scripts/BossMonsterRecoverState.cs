using System.Collections;
using UnityEngine;


public class BossMonsterRecoverState : BossMonsterState
{
    public BossMonsterRecoverState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }
    private BossSkill skill;

    private bool isDone;
    public bool IsDone => isDone;

    private Coroutine waitRoutine;
    
    public override void OnStateEnter()
    {
        isDone = false;
        // ApplyMove로 인한 linearVelocity의 영향 없애기
        owner.StopMove();
        //Debug.Log("recoverState 진입");
        skill = owner.CurrentSkill;
        waitRoutine = owner.StartCoroutine(RecoverRoutine());
    }

    public override void OnStateUpdate()
    {

    }

    public override void OnStateExit()
    {
        //Debug.Log("recoverState 탈출");
        if(waitRoutine != null)
        {
            owner.StopCoroutine(waitRoutine);
            waitRoutine = null;
        }
    }

    private IEnumerator RecoverRoutine()
    {
        yield return new WaitForSeconds(skill.Data.recoverTime);
        isDone = true;
    }
}
