using UnityEngine;

public class BossMonsterDeadState : BossMonsterState
{
    public BossMonsterDeadState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }

    public override void OnStateEnter()
    {
        owner.UnderVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public override void OnStateUpdate()
    {
    
    }
}
