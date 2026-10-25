using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int ItemID;              // 아이템 ID
    public string ItemDes;          // 아이템 설명
    public int ItemCost;            // 아이템 가격
    public bool isItem;             // 아이템 사용형, 장착형 구분

    public enum ItemPart            // 파츠 부위
    {
        Wing = 1,                   // 날개
        Body = 2,                   // 몸체
        Engine = 3                  // 엔진
    }

    [Header("아이템 설정 변수")]
    [SerializeField] private float timeFreezeDuration = 3f;     // 시간 정지 지속 시간
    //[SerializeField] private float reflectRadius = 1.5f;        // 공격 반사 인식 범위
    [SerializeField] private float slowFieldRadius = 4.0f;      // 강속장 범위
    [SerializeField] private float slowFieldDuration = 4.0f;    // 강속장 지속 시간
    [SerializeField] private float slowSpeedAmount = 2f;        // 강속장 내 미사일 속도

    [Header("블랙홀 설정")]
    [SerializeField] private float blackHoleDuration = 3f;      // 블랙홀 유지 시간
    [SerializeField] private float blackHolePullRadius = 5f;    // 블랙홀 흡수 범위
    [SerializeField] private float blackHolePullSpeed = 8f;     // 블랙홀 흡수 속도

    private bool isGlobalTimeStopped = false;                                                               // 시간 정지 상태 여부
    private HashSet<GameObject> reflectedProjectiles = new HashSet<GameObject>();                           // 반사된 미사일 목록
    private Dictionary<GameObject, Transform> forcedTargets = new Dictionary<GameObject, Transform>();      // 강제 유도 중인 미사일과 목표(적) 목록
    private HashSet<GameObject> slowedProjectiles = new HashSet<GameObject>();                              // 강속장 범위 안의 미사일 목록

    /*
     날개 : 강제 유도, 시간 정지
     몸통 : 공격 반사, 블랙홀
     엔진 : 강속장
    */


    void Update()
    {
        ManageOverrideLogic();
    }

    private void ManageOverrideLogic()
    {
        GameObject[] throws = GameObject.FindGameObjectsWithTag("Throw");             // Throw 테그를 가진 오브젝트 불러오기

        foreach (GameObject obj in throws)
        {
            if (obj == null) continue;                                                // 이미 사라진 오브젝트는 넘기기

            Boss_Missile missileScript = obj.GetComponent<Boss_Missile>();            // 미사일 연산값 빼오기
            if (missileScript == null) continue;                                      // 미사일이 아니면 넘기기

            if (forcedTargets.ContainsKey(obj) && forcedTargets[obj] != null)         // 능력 1(강제유도)의 경우
            {
                Vector3 originalMove = (Vector3)missileScript.dir.normalized * Time.deltaTime * missileScript.Missile_Spd;    // 미사일이 원래 이동하려던 값
                obj.transform.position -= originalMove;                                         // 기존 플레이어 타겟팅 이동 롤백

                Transform targetTransform = forcedTargets[obj];                                 // 유도 목표(적)
                Vector2 newDir = targetTransform.position - obj.transform.position;             // 미사일 -> 목표 방향 구하기
                float angle = Mathf.Atan2(newDir.y, newDir.x) * Mathf.Rad2Deg;                  // 방향의 각도 구함
                obj.transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);    // 미사일이 목표를 바라보게 함 (y축이 정면)
                obj.transform.position += (Vector3)newDir.normalized * Time.deltaTime * missileScript.Missile_Spd;    // 목표 방향으로 이동
                continue;
            }

            if (isGlobalTimeStopped)                                            // 능력 2(시간정지)의 경우
            {
                obj.transform.position -= (Vector3)missileScript.dir.normalized * Time.deltaTime * missileScript.Missile_Spd;       // 이전 프레임의 위치로 고정
                continue;
            }

            if (reflectedProjectiles.Contains(obj))                             // 능력 3(공격반사)의 경우
            {
                
            }

            if (slowedProjectiles.Contains(obj))                                // 능력 5(강속장)의 경우
            {
                obj.transform.position -= (Vector3)missileScript.dir.normalized * Time.deltaTime * missileScript.Missile_Spd;    // 원래 이동량 롤백
                obj.transform.position += (Vector3)missileScript.dir.normalized * Time.deltaTime * slowSpeedAmount;              // 느려진 속도(slowSpeedAmount)로 다시 이동
            }
        }
    }

    public void ExecuteForcedGuidance()     // 강제 유도
    {
        Debug.Log("강제 유도 발동");
        GameObject targetEnemy = GameObject.FindWithTag("Boss") ?? GameObject.FindWithTag("Enemy");    // 보스를 우선으로, 없으면 일반 적을 목표로 설정
        if (targetEnemy == null) return;                                    // 목표가 없으면 발동 취소

        GameObject[] throws = GameObject.FindGameObjectsWithTag("Throw");    // Throw 태그를 가진 오브젝트 불러오기
        foreach (GameObject obj in throws)
        {
            if (obj != null && !forcedTargets.ContainsKey(obj))             // 이미 유도 중인 미사일은 제외
            {
                forcedTargets.Add(obj, targetEnemy.transform);              // 미사일에 유도 목표 등록
            }
        }
    }

    public void ExecuteTimeStop()       // 시간 정지
    {
        Debug.Log("시간 정지 발동");
        StartCoroutine(TimeStopRoutine());
    }

    private IEnumerator TimeStopRoutine()
    {
        isGlobalTimeStopped = true;                                         // 시간 정지 시작
        yield return new WaitForSeconds(timeFreezeDuration);                // 지속 시간만큼 대기
        isGlobalTimeStopped = false;                                        // 시간 정지 종료
        Debug.Log("시간 정지 해제");
    }

    public void ExecuteReflection()     // 공격 반사
    {
        
    }

    public void ExecuteBlackHole()      // 블랙홀
    {
        Debug.Log("블랙홀 생성");
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);    // 마우스 위치를 월드 좌표로 변환
        mouseWorldPos.z = 0f;                                               // 2D라서 z값 고정

        StartCoroutine(BlackHoleRoutine(mouseWorldPos));                    // 마우스 위치에 블랙홀 생성
    }

    private IEnumerator BlackHoleRoutine(Vector3 spawnPos)
    {
        float elapsed = 0f;                                                 // 경과 시간
        while (elapsed < blackHoleDuration)                                 // 블랙홀 유지 시간동안 반복
        {
            GameObject[] throws = GameObject.FindGameObjectsWithTag("Throw");    // Throw 태그를 가진 오브젝트 불러오기
            foreach (GameObject obj in throws)
            {
                if (obj == null) continue;                                  // 이미 사라진 오브젝트는 넘기기

                float distance = Vector2.Distance(spawnPos, obj.transform.position);    // 블랙홀 중심과 미사일의 거리
                if (distance <= blackHolePullRadius)                        // 흡수 범위 안이면
                {
                    if (distance <= 0.3f)                                   // 중심에 거의 닿았으면
                    {
                        Destroy(obj); // 사정거리 중심에 잡히면 즉시 증발
                    }
                    else
                    {
                        Vector3 pullDirection = (spawnPos - obj.transform.position).normalized;         // 블랙홀 중심으로 좌표 강제 이동
                        obj.transform.position += pullDirection * blackHolePullSpeed * Time.deltaTime;
                    }
                }
            }
            elapsed += Time.deltaTime;                                      // 경과 시간 누적
            yield return null;                                              // 다음 프레임까지 대기
        }
        Debug.Log("블랙홀 소멸");
    }

    public void ExecuteSlowField()      // 강속장
    {
        Debug.Log("강속장 발동");
        StartCoroutine(SlowFieldRoutine());
    }

    private IEnumerator SlowFieldRoutine()
    {
        float elapsed = 0f;                                                 // 경과 시간
        while (elapsed < slowFieldDuration)                                 // 강속장 지속 시간동안 반복
        {
            GameObject[] throws = GameObject.FindGameObjectsWithTag("Throw");    // Throw 태그를 가진 오브젝트 불러오기
            foreach (GameObject obj in throws)
            {
                if (obj == null) continue;                                  // 이미 사라진 오브젝트는 넘기기
                float distance = Vector2.Distance(transform.position, obj.transform.position);    // 플레이어와 미사일의 거리

                if (distance <= slowFieldRadius)                            // 강속장 범위 안이면
                {
                    if (!slowedProjectiles.Contains(obj)) slowedProjectiles.Add(obj);    // 느려질 미사일로 등록
                }
                else
                {
                    if (slowedProjectiles.Contains(obj)) slowedProjectiles.Remove(obj);    // 범위를 벗어나면 원래 속도로 복구
                }
            }
            elapsed += Time.deltaTime;                                      // 경과 시간 누적
            yield return null;                                              // 다음 프레임까지 대기
        }
        slowedProjectiles.Clear();                                          // 강속장 종료 시 목록 초기화
        Debug.Log("강속장 해제");
    }
}