using UnityEngine;
using UnityEngine.Playables;

public class PlayerDeadState : PlayerState
{
    public PlayerDeadState(Player player) : base(player) { }

    // 구현 해야함
    public bool IsDone { get; set; }

    public override void OnStateEnter()
    {
    }
    public override void OnStateUpdate()
    {
        //    // 부활
        //    yield return new WaitForSeconds(respawnDelay);
        //    playerHp = maxHp;
        //    transform.position = respawnPoint;
        //    ChangeState(PlayerState.Idle);
    }

    public override void OnStateFixedUpdate()
    {
        base.OnStateFixedUpdate();
    }

    public override void OnStateExit()
    {
    }
}
