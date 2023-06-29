using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// 점수, 스테이지 관리
public class GameManager : MonoBehaviour
{
    public int totalPoint;
    public int stagePoint;
    public int stageIndex;  // stageIndex에 따라 스테이지 활성화/비활성화
    public int health;  // 플레이어 체력(총 3)
    public PlayerMove player;
    public GameObject[] Stages;

    // UI
    public Image[] UIhealth;
    public TextMeshProUGUI UIPoint;
    public TextMeshProUGUI UIStage;
    public GameObject RestartBtn;
    public GameObject StartImage;

    public void OnClickStartButton()
    {
        StartImage.SetActive(false);
    }

    void Update()
    {
        // 점수 UI
        UIPoint.text = (totalPoint + stagePoint).ToString();
    }

    public void NextStage()
    {
        // 스테이지 변경
        if(stageIndex < Stages.Length - 1){ // 스테이지 갯수 확인 => 다음 스테이지 이동 및 종료 제어
            Stages[stageIndex].SetActive(false);  // 현재 스테이지 비활성화
            stageIndex++;
            Stages[stageIndex].SetActive(true); // 다음 스테이지 활성화
            PlayerReposition();

            // 스테이지 UI
            UIStage.text = "Stage " + (stageIndex + 1);
        }
        else {  // 마지막 결승점에 도달하여 게임을 클리어 했다면
            // 플레이어 컨트롤 제어
            Time.timeScale = 0; // 시간 멈춤
            // 결과 UI
            Debug.Log("과탑 등극! 축하합니다!");
            // 재시작 UI
            TextMeshProUGUI btnText = RestartBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "과탑 등극! 축하합니다!";
            RestartBtn.SetActive(true);
        }

        // 점수 합산
        totalPoint += stagePoint;
        stagePoint = 0;
    }

    // 체력감소 로직
    public void HealthDown(){
        if(health > 1){
            health--;
            // 체력 감소시 체력 UI 어둡게 변경
            UIhealth[health].color = new Color(0, 0, 0, 0.4f);
        }
        else {  // 체력이 0이라면
            // 사망 시 체력 UI
            UIhealth[0].color = new Color(0, 0, 0, 0.4f);
            // 플레이어 사망 효과
            player.OnDie();
            // 결과 UI
            Debug.Log("자네는 F일세.");
            // 재시작 UI
            TextMeshProUGUI btnText = RestartBtn.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = "자네는 F일세.";
            RestartBtn.SetActive(true);
        }
    }

    // 추락 로직
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 맵 아래로 추락한다면
        if(collision.gameObject.tag == "Player"){
            // 추락 후 시작지점 복귀(체력이 1 이상일때만)
            if(health > 1){
                // collision.attachedRigidbody.velocity = Vector2.zero;
                // collision.transform.position = new Vector3(-32.92f, 0.45f, 0);
                PlayerReposition();
            }

            // 체력 1 감소
            HealthDown();
        }
    }

    // 플레이어 시작지점
    void PlayerReposition()
    {
        player.transform.position = new Vector3(-32.92f, 0.45f, 0);
        player.VelocityZero();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
