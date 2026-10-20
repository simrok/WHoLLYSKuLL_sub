using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using System;
[RequireComponent(typeof(Rigidbody))]

public class Player : MonoBehaviour, IDamageable
{
    [UnityEngine.Serialization.FormerlySerializedAs("context")] public PlayerData data = new PlayerData();
    private StateMachine stateMachine;

    // 하트 시스템
    public event Action<int> OnHeartsChanged;
    
    // 공격 판정
    [SerializeField] LayerMask enemyMask;   // Inspector에서 Enemy 체크

    // Gizmo용: 마지막으로 띄운 공 기억;
    Vector3 lastHitCenter;
    float lastHitRadius;
    float lastHitTime = -999f;

    // 상태 객체는 Awake에서 한 번 만들어두고 계속 재사용
    private PlayerIdleState idleState;
    private PlayerMoveState moveState;
    private PlayerRollState rollState;
    private PlayeyKickState kickState;
    private PlayerSliceState sliceState;
    private PlayerGetHitState getHitState;
    private PlayerDeadState deadState;

    private void Awake()
    {
        data.Init(GetComponent<Rigidbody>(), GetComponentInChildren<Animator>(), GetComponent<PlayerMovement>());

        idleState = new PlayerIdleState(this);
        moveState = new PlayerMoveState(this);
        rollState = new PlayerRollState(this);
        kickState = new PlayeyKickState(this);
        sliceState = new PlayerSliceState(this);
        getHitState = new PlayerGetHitState(this);
        deadState = new PlayerDeadState(this);

        stateMachine = new StateMachine(idleState);     // 시작 상태

        data.currentHearts = data.maxHearts;
        data.kickDamage = 10;
        data.isInvincible = false;
    }

    private void Update()
    {
        // 애니메이션: 방향키 입력이 있으면 1:Move, 없으면 0:Idle 
        float speed = data.PlayerMovement.CurrentSpeed;  // 멈춤 0, 걷기 3, 뛰기 8
        data.Animator.SetFloat("Speed", speed, 0.1f, Time.deltaTime);

        DecideState();  // Player가 State를 판단
        stateMachine.UpdateState(); // 현재 상태를 계속 행동함
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdateState();
    }

    public void DecideState()
    {
        IState cur = stateMachine.CurrentState;

        // 1) 죽음
        if (data.currentHearts <= 0)
        {
            stateMachine.ChangeState(deadState);
            return;
        }
        //2) 피격 (무적이면 무시)
        if (data.gotHit)
        {
            data.gotHit = false; // 피격 신호 끄기
            if (!data.isInvincible)
            {
                stateMachine.ChangeState(getHitState);
                return;
            }
        }
        // 3) 행동 중이면 끝날 때까지 기다림
        if (cur == rollState && !rollState.IsDone) return;
        if (cur == sliceState && !sliceState.IsDone) return;
        if (cur == kickState && !kickState.IsDone) return;
        if (cur == getHitState && !getHitState.IsDone) return;

        // 4) 입력
        if (Input.GetKeyDown(KeyCode.LeftShift)) { }    // 뛰기
        if (Input.GetKeyDown(KeyCode.A)) { stateMachine.ChangeState(rollState); return; }   // 구르기
        if (Input.GetKeyDown(KeyCode.S)) { stateMachine.ChangeState(kickState); return; }     // 발차기
        if (Input.GetKeyDown(KeyCode.D)) { stateMachine.ChangeState(sliceState); return; }     // 칼로 베기

        // 5) 기본: 방향키가 있으면 Move, 없으면 Idle
        stateMachine.ChangeState(data.PlayerMovement.Dir != Vector3.zero ? moveState : idleState);
    }

    public void TakeDamage(int damage)
    {
        // 무적 상태이면 무시
        if (data.isInvincible) return;

        data.gotHit = true;
        data.currentHearts -= damage;

        if (data.currentHearts <= 0) { data.currentHearts = 0; }

        OnHeartsChanged?.Invoke(data.currentHearts);

        // 데미지 오버레이 매니저
        if (DamageOverlayManager.Instance != null)
        {
            DamageOverlayManager.Instance.HitOverlay();
            DamageOverlayManager.Instance.CheckHp(data.currentHearts, data.maxHearts);
        }

    }

    // 몬스터 타입 공격
    public List<Monster> GetHitMonsters(Vector3 _center, float _radius)
    {
        // 중심, 반지름을 받아서 맞은 Monster 목록 반환
        // 1. Gizmos로 그리기 위해 기록
        lastHitCenter = _center;
        lastHitRadius = _radius;
        lastHitTime = Time.time;

        // 2. 공 안의 Enemy 콜라이더 찾기 
        Collider[] colliders = Physics.OverlapSphere(_center, _radius, enemyMask);

        // 3. 콜라이더 -> Monster로 바꿔서 목록에 담기(같은 몬스터 중복 제외)
        List<Monster> result = new List<Monster>();
        foreach (Collider collider in colliders)
        {
            Monster monster = collider.GetComponentInParent<Monster>();
            if (monster != null && !result.Contains(monster))
            {
                result.Add(monster);
            }
        }
        return result;
    }
    //private void OnDrawGizmos()
    //{
    //    // 공격 직후 0.2초 동안만 그리기
    //    if (Time.time - lastHitTime > 0.2f) return;
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(lastHitCenter, lastHitRadius);
    //}
}




