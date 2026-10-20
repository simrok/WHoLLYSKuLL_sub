using UnityEngine;

public class BossMonsterPhaseChangeState : BossMonsterState
{
    public BossMonsterPhaseChangeState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }

    private bool isDone;
    public bool IsDone => isDone;

    public override void OnStateEnter()
    {
        isDone = false;    
    }

    public override void OnStateUpdate()
    {
        isDone = true;
    }
}
