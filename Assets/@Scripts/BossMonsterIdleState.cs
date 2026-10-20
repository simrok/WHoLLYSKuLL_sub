using UnityEngine;

public class BossMonsterIdleState : BossMonsterState
{
    public BossMonsterIdleState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }

    private bool isDone;
    public bool IsDone => isDone;

    public override void OnStateEnter()
    {
        // 보스는 제자리에서 대기 애니메이션만 재생(idle_up_down)
        // 공격하지 않고, 데미지를 받지 않고, 쿨타임도 돌지 않음(BossMonsterController에서 처리)
        // 플레이어가 아레나 안의 트리거에 들어가면 Intro로 넘어감(BossMonsterController에서 처리)
        isDone = false;
        //Debug.Log("현재 idle 상태");
    }

    public override void OnStateUpdate()
    {
        // 트리거를 밟으면 intro로 넘어가는데 이때 입구를 막는다
        // 트리거를 밟았는지 확인 -> isIntroPlayed 체크
    }

    public override void OnStateExit()
    {
        isDone = true;
        //Debug.Log("현재 idle 상태에서 벗어남");
    }
}

