using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMonsterController : Monster
{
    private StateMachine stateMachine;
    public Transform playerTransform;
    
    // 보스 몬스터의 공격 필드 범위 확인 용 콜라이더
    [SerializeField] private Collider arenaCollider;
    public Collider ArenaCollider => arenaCollider;

    // 보스 몬스터 피격 효과
    private HitFlash hitFlash;

    // 보스 고유 필드
    [SerializeField] int phaseCount;   // 해당 보스의 페이즈 개수
    [SerializeField] public int phaseChangeHp;       // 페이즈가 바뀌는 기준 HP

    private int currentPhase = 1;    // 현재 보스전 페이즈
    public int CurrentPhase => currentPhase;
    private bool isPhaseChanged;    // 페이즈가 바꼈는지
    private bool isInvincible;  // 무적 상태인지

    // 움직임
    private float maxSlopeAngle = 40f;     // 이 각도보다 가파르면 바닥이 아님
    private float fallMultiplier = 2.5f;

    // 각 페이즈 별 스킬
    [SerializeField] private List<BossSkill> bossSkills;
    public List<BossSkill> BossSkills => bossSkills;
    private BossSkill currentSkill;     // 현재 사용할 스킬
    public BossSkill CurrentSkill => currentSkill;

    // States
    private BossMonsterIntroState introState;
    private BossMonsterIdleState idleState;
    private BossMonsterChaseState chaseState;
    private BossMonsterAttackState attackState;
    private BossMonsterRecoverState recoverState;
    private BossMonsterPhaseChangeState phaseChangeState;
    private BossMonsterDeadState deadState;
    private BossMonsterDropState dropState;

    // State: idle
    [SerializeField] BossMapGate bossMapGate;
    private bool isGateClosed;

    // State: introState
    private bool isIntroPlayed; // 인트로를 이미 봤는지

    // State: attackState
    private Coroutine skillRoutine;
    public bool IsSkillRunning { get; private set; }

    // State: chaseState
    [SerializeField] private float chaseStartRange = 35f; // 추격 기준 거리

    // State: deadState
    [SerializeField] private ParticleSystem underVFX;
    public ParticleSystem UnderVFX => underVFX;

    private void OnEnable() 
    {
        isGateClosed = false;
        bossMapGate.OnGateClosed += CheckGateClosed; 
    }
    private void OnDisable() { bossMapGate.OnGateClosed -= CheckGateClosed; }

    private void Start()
    {
        //onDie += DropBone;
        underVFX.Play(true);
    }

    protected override void Awake()
    {
        base.Awake();

        // 보스만의 초기화
        introState = new BossMonsterIntroState(this); 
        idleState = new BossMonsterIdleState(this); 
        chaseState = new BossMonsterChaseState(this); 
        attackState = new BossMonsterAttackState(this);
        recoverState = new BossMonsterRecoverState(this);
        phaseChangeState = new BossMonsterPhaseChangeState(this);
        deadState = new BossMonsterDeadState(this);
        dropState = new BossMonsterDropState(this);

        stateMachine = new StateMachine(idleState);    // idle로 시작

        hitFlash = GetComponentInParent<HitFlash>();
    }

    private void Update()
    {
        if (isDead) return;
        DecideState();
        stateMachine.UpdateState();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        stateMachine.FixedUpdateState();
    }

    public void DecideState()
    {
        IState cur = stateMachine.CurrentState;

        // 1. 인트로 중에는 다른 전환 금지
        if (cur == introState)
        {
            if (introState.IsDone)
            {
                isIntroPlayed = true;
                stateMachine.ChangeState(idleState);
            }
            return;
        }

        // 2.  전투 시작 전: 문이 닫힐 때까지 대기만
        if (!isIntroPlayed)
        {
            if (isGateClosed) stateMachine.ChangeState(introState);
            return;
        }

        // 4. 전투 중 규칙(죽음, 추격, 공격...)
        // 1) 플레이어 죽음
        // 1-1) deadState 전환
        if (currentHealth <= 0)
        {
            stateMachine.ChangeState(deadState);
            return;
        }
        // 1-2) deadState에서 처리 끝나면 dropState
        if (isDead)
        {
            stateMachine.ChangeState(dropState);
            return;
        }

        // 2) 플레이어가 아레나 안에 있으면
        if (PlayerInArena())        
        {
            // 2-1) 다른 행동 중이라면 상태 변환 안함
            if (cur == attackState && !attackState.IsDone) return;
            if (cur == phaseChangeState && !phaseChangeState.IsDone) return;

            // 2-2) 행동 결정
            if (cur == idleState)
            {
                //Debug.Log("플레이어가 chase range 안에 있는가?: " + PlayerInChaseRange());
                if (PlayerInChaseRange()) stateMachine.ChangeState(chaseState);    // idleState -> chaseState
            }
            else if (cur == chaseState && chaseState.CanAttack)
            {
                currentSkill = ProcessSkill();
                if (currentSkill != null)
                {
                    stateMachine.ChangeState(attackState);      // chaseState -> attackState
                }
            }
            else if (cur == attackState && attackState.IsDone)
            {
                stateMachine.ChangeState(recoverState);         // attackState -> recoverState
            }
            else if (cur == recoverState && recoverState.IsDone)
            {
                stateMachine.ChangeState(chaseState);           // recoverState -> chaseState
            }
        }
    }
    // 이동
    public void ApplyMove(Vector3 moveDir, float speed)
    {
        // 1) 발밑 바닥 확인
        // Trigger 콜라이더(몬스터 감지 범위 등)는 무시
        bool hitGround = Physics.Raycast(this.transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, 0.4f, ~0, QueryTriggerInteraction.Ignore);

        // 2) 바닥 면의 기울기(각도)가 maxSlopeAngle 이하일 때만 걸을 수 있는 바닥임
        // 바닥 면이 수평에서 몇 도 기울었는지 구함. 따라서 레이가 맞고 동시에 완만할 때 true
        bool isGrounded = hitGround && Vector3.Angle(hit.normal, Vector3.up) <= maxSlopeAngle;

        Vector3 velocity;
        if (isGrounded)
        {
            // 2) 바닥에 있으면: 이동 방향을 바닥 면에 맞게 눕혀서 그 방향으로 이동
            // -> 오르막은 위로, 내리막은 아래로, 경사로 끝에서는 바닥 normal이 (0,1,0)으로 바뀌면서 y 속도가 바로 0이 됨
            velocity = Vector3.ProjectOnPlane(moveDir, hit.normal).normalized * speed;
        }
        else
        {
            // 3) 공중이면: 수평은 입력대로, y는 중력에 맡기되 더 세게 끌어내림
            // 떨어지는 속도는 살리고 올라가는 속도를 없앰
            velocity = moveDir * speed;
            float y = Mathf.Min(rb.linearVelocity.y, 0f);
            // 떨어질 때 중력 추가
            velocity.y = y + Physics.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        }
        rb.linearVelocity = velocity;   // 물리 엔진이 이동 + 충돌 처리
    }
    public void StopMove()
    {
        Vector3 v = rb.linearVelocity;
        rb.linearVelocity = new Vector3(0f, v.y, 0f);   // 수평만 0, 떨어지는 속도(y)는 유지
    }

    public override void TakeDamage(int damage)
    {
        // 무적 상태이면 무시
        if (isInvincible) return;
        // 코드 줄 추가
        base.TakeDamage(damage);
        // 피격 효과
        hitFlash.Flash();
        // 페이즈 체크
        if (!isPhaseChanged)
        {
            if (currentHealth <= phaseChangeHp)
            {
                currentPhase += 1;
                isPhaseChanged = true;
            }
        }
    }

    protected override void Die()
    {
        // 사망 애니메이션

        base.Die();
    }

    public bool PlayerInArena()
    {
        return arenaCollider.bounds.Contains(playerTransform.position);
    }

    // 스킬
    public void StartSkill(BossSkill skill)
    {
        skillRoutine = StartCoroutine(RunSkill(skill));
    }

    public IEnumerator RunSkill(BossSkill _skill)
    {
        IsSkillRunning = true;  // 스킬이돌고 있으면 attackState
        yield return _skill.Execute(this);
        IsSkillRunning = false; // attackState -> recoverState로 넘어감
    }

    public void StopSkill()
    {
        if (skillRoutine != null)
        {
            StopCoroutine(skillRoutine);
        }
        skillRoutine = null;
        IsSkillRunning = false;     // 끊기면 RunSkill 마지막 줄이 실행 안되므로 직접 정리
    }

    // 스킬 리스트에 있는 스킬들을 페이즈 별로 처리 + 하나의 스킬을 랜덤 꺼낸다.
    public BossSkill ProcessSkill()
    {
        List<BossSkill> usable = new List<BossSkill>();

        foreach (BossSkill skill in bossSkills)
        {
            if (skill.Data.minPhase <= currentPhase)
            {
                if(skill.CanUseSkill(this))   // 쿨타임, 거리, 페이즈 체크
                {
                    usable.Add(skill);
                }
            }
        }

        if (usable.Count == 0) return null;
        return usable[UnityEngine.Random.Range(0, usable.Count)];
    }

    // idle -> chase: 플레이어와의 거리
    public bool PlayerInChaseRange()
    {
        return Vector3.Distance(playerTransform.position, this.transform.position) <= chaseStartRange;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, chaseStartRange);
    }

    // 보스맵 게이트
    private void CheckGateClosed()
    {
        isGateClosed = true;
    }
}
