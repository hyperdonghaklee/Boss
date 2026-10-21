using UnityEngine;

public class Boss : MonoBehaviour
{
    public Transform Target_Player;

    public float Boss_Hp = 1f;
    [SerializeField] private float Boss_Spd = 1f;

    [SerializeField] private float Boss_FireRate = 1f;
    //public int Boss_BulletType = 0;
    public int Boss_Pattern = Random.Range(1, 6);               // 공격 패턴, 총 5개


    void Awake()
    {

    }


    void Start()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("플레이어와 충돌");
            Boss_Hp -= 1;
        }
    }

    void Update()
    {
        
    }
}
