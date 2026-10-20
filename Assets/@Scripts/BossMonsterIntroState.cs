using System.Collections;
using UnityEngine;

public class BossMonsterIntroState : BossMonsterState
{
    public BossMonsterIntroState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }

    private Coroutine waitSeconds;
    private bool isDone;
    public bool IsDone => isDone;

    public override void OnStateEnter()
    {
        isDone = false;
        waitSeconds = owner.StartCoroutine(WaitSeconds());
        // 애니메이션 재생
        // vfx 효과 재생
        //Debug.Log("인트로 시작");
    }

    public override void OnStateUpdate()
    {
        
    }

    public override void OnStateExit()
    {
        //Debug.Log("인트로 끝");
        owner.StopCoroutine(waitSeconds);
    }

    public IEnumerator WaitSeconds()
    {
        yield return new WaitForSeconds(0.3f);
        isDone = true;
    }
}
