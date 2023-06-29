using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public float maxSpeed;  // 플레이어의 최대 속력
    public float jumpPower;
    public float jumpCount;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    CapsuleCollider2D capsuleCollider;

    // 초기화
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    // 단발적인 키 입력 제어
    void Update()   
    {
        // 점프(2단 점프까지 가능)
        if(Input.GetKeyDown(KeyCode.Space) && jumpCount < 2){
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            jumpCount++;
            anim.SetBool("isJumping", true);
        }

        // 정지 속도
        if(Input.GetButtonUp("Horizontal")) {
            // rigid.velocity.normalized => 벡터 크기를 1로 변경(방향 구하기)
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        // 플레이어 방향 전환
        if(Input.GetButton("Horizontal"))
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") == -1;

        // 플레이어 애니메이션
        if(Mathf.Abs(rigid.velocity.x) < 0.5)   // 플레이어의 속력이 0.3 이하로 떨어진다면(정지 상태라면)
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
    }

    void FixedUpdate()
    {
        // 플레이어 이동(방향키 이용), 이동 속도
        float h = Input.GetAxisRaw("Horizontal");
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        // 최대 속도
        if(rigid.velocity.x > maxSpeed) // rigid.velocity == 플레이어 속도, 오른쪽 이동 시 최대 속도 제어
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);    // y축 0으로 잡지 않도록 주의(0으로 주면 점프 불가)
        else if(rigid.velocity.x < maxSpeed * (-1))    // 왼쪽 이동 시 최대 속도 제어
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        // Landing Platform
        if(rigid.velocity.y < 0) {
            // Debug.DrawRay == 에디터 상에서 Ray를 그림
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0,1,0)); 

            // RayCastHit == Ray에 닿은 오브젝트, GetMask == 레이어 이름에 해당하는 정수값 리턴
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform")); 

            // 만약 오브젝트가 Ray를 맞았다면
            if(rayHit.collider != null) {
                if(rayHit.distance < 2.5f) // distance == Ray에 닿았을 때의 거리, 보통 우측 숫자는 Player collider 크기의 절반으로 설정
                    // Debug.Log(rayHit.collider.name); // 어느 Platform(== floor)에 닿는지 콘솔창에서 Platform 이름 확인
                    jumpCount = 0;
            }
            anim.SetBool("isJumping", false);
        }
    }

    // 피격 판정(== 충돌 판정)
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "enemy") {
            // 플레이어가 몬스터보다 위에 있고, 아래로 낙하중이라면 => 즉, 플레이어가 몬스터를 밟는 다면
            if(rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y){    
                OnAttack(collision.transform);
            }
            else
                OnDamaged(collision.transform.position);
        }
    }

    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 아이템
        if(collision.gameObject.tag == "Item") {
            // 점수
            bool isItemA = collision.gameObject.name.Contains("Item_A");
            bool isItemB = collision.gameObject.name.Contains("Item_B");
            bool isItemC = collision.gameObject.name.Contains("Item_C");

            if(isItemA)
                gameManager.stagePoint += 30;
            else if(isItemB)
                gameManager.stagePoint += 20;
            else if(isItemC)
                gameManager.stagePoint += 10;

            // 아이템 소멸
            collision.gameObject.SetActive(false);
        }
        // 결승점(finish point)
        else if(collision.gameObject.tag == "Finish") {
            // 다음 스테이지로 전환
            gameManager.NextStage();
        }
    }

    // 몬스터 밟기
    void OnAttack(Transform enemy)
    {
        // 점수
        gameManager.stagePoint += 10;

        // 몬스터 밟은 후 플레이어 반동
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        // 몬스터 소멸
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }

    // 몬스터에 피격되었다면 무적 효과 부여
    void OnDamaged(Vector2 targetPos)
    {
        // 체력 1 감소
        gameManager.HealthDown();

        // 레이어 변경(Player -> PlayerDamaged)
        gameObject.layer = 11;

        // 피격 시 플레이어 색상 반투명하게 변경
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);

        // 피격 시 플레이어가 뒤로 밀려남
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1)*7, ForceMode2D.Impulse);

        // 피격 애니메이션
        anim.SetTrigger("doDamaged");

        // 무적 효과는 3초 동안 지속
        Invoke("OffDamaged", 3);
    }

    // 무적 효과 해제
    void OffDamaged()
    {
        // 레이어 변경(PlayerDamaged -> Player)
        gameObject.layer = 10;
        // 플레이어 색상을 반투명 색상에서 정상으로 변경
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    // 체력(==health)가 0이 되면 사망
    public void OnDie()
    {
        // 색상 흐릿하게
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // 뒤집어짐
        spriteRenderer.flipY = true;
        // collider 비활성화
        capsuleCollider.enabled = false;
        // 살짝 점프 후 맵 밑으로 추락
        rigid.AddForce(Vector2.up * 3, ForceMode2D.Impulse);
    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
}