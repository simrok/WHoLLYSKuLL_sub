using NUnit.Framework.Constraints;
using System.Collections;
using UnityEngine;

public class BossHatThrowSkill : BossSkill
{
    [SerializeField] Transform hat;
    [SerializeField] Transform indicator;      // 인디케이터 프리팹
    protected const float Depth = 3f; // 데칼 depth

    [SerializeField] private int hatSpinSpeed = 720;
    [SerializeField] private float hatMoveSpeed = 40f;
    [SerializeField] private float hitRadius = 2f;

    [SerializeField] private float spawnForward = 5f;
    [SerializeField] private float spawnHeight = 2f;
    [SerializeField] private float interval = 0.2f;
    [SerializeField] private float hatReturnDelay = 0.3f;

    private Coroutine hatThrowRoutine;
    private Coroutine hatReturnRoutine;

    private void OnDisable()
    {
        if (hatThrowRoutine != null)
        {
            StopCoroutine(hatThrowRoutine);
            hatThrowRoutine = null;
        }
        if (hatReturnRoutine != null)
        {
            StopCoroutine(hatReturnRoutine);
            hatReturnRoutine = null;
        }
        hat.gameObject.SetActive(true); // 맨머리고 남지 않게 처리
    }
    public override IEnumerator Run(BossMonsterController boss)
    {
        //  방향: 보스가 지금 바라보고 있는 방향(attackState회전에 의존)
        Vector3 dir = boss.Rb.rotation * Vector3.forward;
        dir.y = 0;
        dir.Normalize();

        // 모자 방향 설
        Vector3 startPos = boss.Rb.position + dir * spawnForward;    // 보스위치 + 방향 벡터 * 거리
        startPos.y = boss.playerTransform.position.y + spawnHeight;
        // indicator 방향 설정
        indicator.position = startPos;  
        indicator.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 90f, 0f);   // 보스로 부터 데칼이 나옴(y축 회전 -90) + 데칼이 dir 방향으로 뻗음

        // 몸을 뒤로 젖히는 애니메이션
        boss.Animator.SetTrigger("HatThrow");

        // 바닥에 플레이어 방향으로 직선 장판을 표시 + 모자가 머리 위에서 빙글 빙글 돌기 시작함
        indicator.gameObject.SetActive(true);

        Quaternion hatOriginRot = hat.localRotation;   // 원래 모자의 각도
        float t = 0f;
        while (t < data.telegraphTime)
        {
            t += Time.deltaTime;

            // 처음 turnTime 동안만 보스가 돌고, 그 뒤로는 targetRot에 고정
            //boss.transform.rotation = Quaternion.Slerp(startRot, targetRot, t / turnTime);

            hat.Rotate(hatSpinSpeed * Time.deltaTime, 0f, 0f, Space.Self);
            yield return null;
        }
        hat.localRotation = hatOriginRot;   // 원래 각도로 되돌리기

        // 장판 끄기, 머리 위 모자 끄기
        indicator.gameObject.SetActive(false);
        hat.gameObject.SetActive(false);

        // 모자 던지기 코루틴
        hatThrowRoutine = StartCoroutine(HatThrow(startPos, dir, boss));
        hatReturnRoutine = StartCoroutine(HatReturn());
    }

    private IEnumerator HatThrow(Vector3 startPos, Vector3 dir, BossMonsterController boss)
    {
        // 아레나 콜라이더
        Bounds arena = boss.ArenaCollider.bounds;

        // 모자가 플레이어 높이에서 나타남(순간이동)
        Transform proj = Instantiate(hat, startPos, Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 0f, -90f)); // 모자의 위가 -x축이라 z축으로 -90도 보정
        proj.gameObject.SetActive(true);
        proj.localScale = hat.lossyScale;   // 원래 모자 크기
        yield return new WaitForSeconds(interval);

        while (true)
        {
            // 모자가 회전하면서 수평방향으로 직진
            proj.position += dir * hatMoveSpeed * Time.deltaTime;
            proj.Rotate(0f, hatSpinSpeed * Time.deltaTime, 0f, Space.World);

            // 아레나 끝(ArenaCollider 경계)에 닿으면 사라짐
            Vector3 checkPos = proj.position;
            checkPos.y = arena.center.y;    // 높이는 무시하고, x/z만 검사
            if (!arena.Contains(checkPos))
            {
                // scale이 줄어드는 연출 추가
                Destroy(proj.gameObject);
                yield break;
            }

            // [판정] 플레이어에게 닿으면 한 번 데미지 입히기(하트 1개) + 사라지기
            Collider[] colliders = Physics.OverlapSphere(proj.position, hitRadius);

            foreach(Collider col in colliders)
            {
                Player player = col.GetComponentInParent<Player>(); // 플레이어인지 확인
                if (player != null)
                {
                    player.TakeDamage(data.baseDamage);    // 하트 1개
                    Destroy(proj.gameObject);
                    yield break;
                }
            }
            yield return null;
        }
    }

    private IEnumerator HatReturn()
    {
        // 모자 없는 맨머리 상태로 hatReturnDelay초 동안 유지
        yield return new WaitForSeconds(hatReturnDelay);
        hat.gameObject.SetActive(true); // 모자 쓰기
        hatReturnRoutine = null;
    }
}
