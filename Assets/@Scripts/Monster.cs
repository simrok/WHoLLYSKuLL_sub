using System;
using UnityEngine;

public class Monster : MonoBehaviour, IDamageable
{
    protected GameObject Player;
    
    // 컴포넌트
    protected Rigidbody rb;
    public Rigidbody Rb => rb;
    protected Animator animator;
    public Animator Animator => animator;
    [SerializeField] protected MonsterData data;
    public MonsterData Data => data;

    // 데이터
    protected int currentHealth;
    protected bool isDead;


    // HpUI
    public event Action<int, int> onHealthChanged;  // (현재 Hp, 최대 Hp)

    protected virtual void Awake()
    {
        currentHealth = data.maxHealth;
        rb = GetComponentInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    public void DropBone() 
    {
        int dropBone = UnityEngine.Random.Range(data.minRewardBone, data.maxRewardBone+1);
        //Debug.Log("평온의 뼛가루 드랍 개수: " + dropBone);
       // PlayerState.instance.GetBone += dropBone;
    }

    public virtual void TakeDamage(int damage)
    {
        // 이미 죽었는지 확인 (가드)
        if (isDead) return;

        // 체력 깎기
        currentHealth -= damage;
        //Debug.Log(data.monsterName + "가 " + damage + " 만큼의 데미지를 받았다. / hp: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        // Hp UI 표시
        onHealthChanged?.Invoke(currentHealth, data.maxHealth);
    }

    protected virtual void Die()
    {
        // 평온의 뼛가루 드랍
        DropBone();
        // 오브젝트 정리
        Destroy(gameObject, 2f);    // 2초 뒤 삭제
    }
}
