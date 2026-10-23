using UnityEngine;

public class Boss : MonoBehaviour
{
    private Transform Target_Player;

    public float Boss_Hp = 1f;
    [SerializeField] private float Boss_Spd = 1f;

    [SerializeField] private float Boss_FireRate = 1f;
    //public int Boss_BulletType = 0;
    public int Boss_Pattern;

    [SerializeField] private GameObject Missile;
    [SerializeField] private GameObject Bullet;

    [SerializeField] private float bulletSpeed = 8f;

    void Awake()
    {
        Boss_Pattern = Random.Range(1, 6);               // 공격 패턴, 총 5개
    }

    void Missile_Luncher()
    {
        if (Missile != null)
        {
            Instantiate(Missile, transform.position, Quaternion.identity);          // 보스의 위치에서 미사일 소환

            Debug.Log("보스가 유도 미사일을 소환했습니다!");
        }
    }
    void SpawnShotgunBullet()
    {
        // 플레이어 찾기
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return; // 플레이어가 없으면 발사 안 함

        Vector2 baseDirection = (player.transform.position - transform.position).normalized; // 보스 -> 플레이어 방향 구하기

        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;    // 방향의 각도 구함

        int bulletCount = Random.Range(5, 9);                                               // 탄수 = 5~8

        float totalSpreadAngle = 60f;                                                       // 탄퍼짐 각도
        float angleStep = totalSpreadAngle / (bulletCount - 1);
        float startAngle = baseAngle - (totalSpreadAngle / 2f);

        for (int i = 0; i < bulletCount; i++)                                               // 총알 소환 및 발사
        {
            float targetAngle = startAngle + (angleStep * i);                               // 총알이 날아갈 각도 계산

            float rad = targetAngle * Mathf.Deg2Rad;                                        // 각도를 다시 방향으로 변환
            Vector2 shotDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Quaternion bulletRotation = Quaternion.Euler(0, 0, targetAngle);                // 총알이 나라가는 방향을 보게함


            GameObject bullet = Instantiate(Bullet, transform.position, bulletRotation);    // 총알 생성

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        }
    }


    void Boss_Pattern_1()
    {
        SpawnShotgunBullet();
    }

    void Boss_Pattern_2()
    {

    }

    void Boss_Pattern_3()
    {

    }

    void Boss_Pattern_4()
    {

    }

    void Boss_Pattern_5()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌");
            Boss_Hp -= 1;

            if (Boss_Hp <= 0)
            {
                Debug.Log("죽었습니다.");
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (Boss_Pattern ==  0)
        {
            Boss_Pattern_1();
        }

        if (Boss_Pattern == 1)
        {
            Boss_Pattern_2();
        }

        if (Boss_Pattern == 2)
        {
            Boss_Pattern_3();
        }

        if (Boss_Pattern == 3)
        {
            Boss_Pattern_4();
        }

        if (Boss_Pattern == 4)
        {
            Boss_Pattern_5();
        }
    }
}
