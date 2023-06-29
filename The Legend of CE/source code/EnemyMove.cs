using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    public int nextMove;    // 몬스터의 행동지표를 결정할 변수

    // 초기화
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        // 5초 후 Think() 함수 호출
        Invoke("Think", 5);
    }

    void FixedUpdate()
    {
        // 왼쪽으로 이동
        rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

        // 지형 체크(낭떠러지인지)
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove*0.2f, rigid.position.y);

        Debug.DrawRay(frontVec, Vector3.down, new Color(0,1,0)); 
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform")); 
        
        if(rayHit.collider == null) 
            Turn();
    }

    // 행동지표(nextMove)를 바꿔줄 함수 Think()
    void Think()
    {
        // 몬스터의 다음 활동(이동) 설정
        nextMove = Random.Range(-1, 2);   // 2는 Range에 포함되지 않고 1까지만 포함됨 주의

        // 애니메이션 방향 전환(Sprite Animation)
        anim.SetInteger("WalkSpeed", nextMove);

        // 몬스터가 서 있지 않은 경우에만 방향 전환(Flip Sprite)
        if(nextMove != 0)   
            spriteRenderer.flipX = nextMove == 1;

        // 행동지표를 바꾸는 시간을 Random하게 지정, 재귀
        float nextThinkTime = Random.Range(2f, 5f);
        Invoke("Think", nextThinkTime);
    }

    // 몬스터 방향 전환
    void Turn()
    {
        nextMove = nextMove * -1;   // 앞이 낭떠러지라면 반대편으로 방향 전환
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke(); // 현재 작동 중인 모든 Invoke() 함수 정지
        Invoke("Think", 2);
    }

    // 몬스터 소멸 시 액션
    public void OnDamaged()
    {
        // 색상 흐릿하게
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        // 뒤집어짐
        spriteRenderer.flipY = true;
        // collider 비활성화
        capsuleCollider.enabled = false;
        // 살짝 점프 후 맵 밑으로 추락
        rigid.AddForce(Vector2.up * 3, ForceMode2D.Impulse);
        // 완전히 소멸
        Invoke("DeActive", 5);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
