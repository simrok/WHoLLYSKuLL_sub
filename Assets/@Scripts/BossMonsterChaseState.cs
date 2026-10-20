using UnityEngine;

public class BossMonsterChaseState : BossMonsterState
{
    // 플레이어 추격 상태
    // 지금 거리에서 쓸 수 있는 기술이 있으면 Attack 상태로 넘어감

    public BossMonsterChaseState(BossMonsterController _bossMonsterController) : base(_bossMonsterController) { }

    private float rotationSmooth = 15f;
    private float stopDistance = 6f;    // 보스 반지름 2.5 + 여유
    private float enterTime;    // chaseState에 들어온 시간
    private float minChaseTime = 2f;    // 최소 추격 시간 
    public bool CanAttack => Time.time - enterTime >= minChaseTime;

    public override void OnStateEnter() 
    { 
        //Debug.Log("ChaseState 시작");
        enterTime = Time.time;
    }
    public override void OnStateUpdate() { }

    public override void OnStateFixedUpdate()
    {
        owner.Rb.angularVelocity = Vector3.zero;    // 물리 충돌로 생긴 회전을 매번 지움

        Vector3 toPlayer = owner.playerTransform.position - owner.Rb.position;               // 플레이어 방향
        toPlayer.y = 0;
        float dist = toPlayer.magnitude;    // 거리 길이
        Vector3 dir = toPlayer.normalized;

        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);   // 회전
            Quaternion nextRotation = Quaternion.Slerp(owner.Rb.rotation, targetRotation, rotationSmooth * Time.fixedDeltaTime);
            owner.Rb.MoveRotation(nextRotation);
        }

        // 이동. 가까우면 멈춤
        if (dist > stopDistance) owner.ApplyMove(dir, data.moveSpeed);
        else owner.ApplyMove(Vector3.zero, 0f);
    }

    public override void OnStateExit()
    {
        // [이동 속도 off] 공격 상태로 넘어갈 때 미끄러지지 않게 수평 속도 제거
        Vector3 v = owner.Rb.linearVelocity;
        owner.Rb.linearVelocity = new Vector3(0f, v.y, 0f);

        // 디버깅용
        //Debug.Log("chaseState에서 머문 시간: " + (Time.time - enterTime));
    }
}
