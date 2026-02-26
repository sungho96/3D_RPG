using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Login Scene 전용 UI
/// - ID/PW 입력 → 로그인 버튼 클릭
/// - 실패: 메시지 패널 + 확인 버튼(Confirm) 활성화
/// - 성공: 메시지 패널 활성화(Confirm 비활성) 후 일정 시간 뒤 Game 씬 이동
/// </summary>
public class UI_Login : UI_Scene
{
    // UI_Base Bind용 enum (Hierarchy 오브젝트 이름과 동일해야 함)
    enum GameObjects
    {
        MessagePanel, // 로그인 결과/안내 메시지 패널
    }

    enum Inputs
    {
        IdInput, // ID 입력창(TMP_InputField)
        PwInput, // PW 입력창(TMP_InputField)
    }

    enum Buttons
    {
        LoginButton,   // 로그인 버튼
        ConfirmButton, // 실패 메시지 확인 버튼(패널 닫기)
    }

    enum Texts
    {
        MessageText, // 메시지 패널에 표시되는 안내 문구
    }

    // Init 중복 실행 방지(예: Start에서 Init을 부르는데, 다른 곳에서도 Init이 호출될 가능성 대비)
    bool _inited = false;

    private void Start()
    {
        // UI_HUD와 동일 패턴:
        // UI가 Instantiate된 뒤 Start에서 Init을 1회 호출하여 바인딩/이벤트 연결을 보장
        if (_inited) return;
        Init();
    }

    public override void Init()
    {
        base.Init();
        Debug.Log("UI_Login Init");

        // Hierarchy 이름 기준으로 UI 요소 찾아서 캐싱/바인딩(UI_Base의 Bind 시스템)
        Bind<GameObject>(typeof(GameObjects));
        Bind<TMP_InputField>(typeof(Inputs));
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));

        // Scene UI는 sortingOrder=0 고정(팝업처럼 겹치지 않는 기본 UI)
        Managers.UI.SetCanvas(gameObject, sort: false);

        // 비밀번호 입력창은 Password 타입으로 강제(보안/UX)
        Get<TMP_InputField>((int)Inputs.PwInput).contentType = TMP_InputField.ContentType.Password;
        Get<TMP_InputField>((int)Inputs.PwInput).ForceLabelUpdate();

        // 시작 시 메시지 패널은 숨김(필요할 때만 표시)
        Get<GameObject>((int)GameObjects.MessagePanel).SetActive(false);

        // 버튼 이벤트 연결
        // - LoginButton 클릭 → OnClickLogin
        // - ConfirmButton 클릭 → 메시지 패널 닫기 + 입력 포커스 복구
        Get<Button>((int)Buttons.LoginButton).onClick.AddListener(OnClickLogin);
        Get<Button>((int)Buttons.ConfirmButton).onClick.AddListener(OnClickConfirmMessage);
    }

    /// <summary>
    /// 로그인 버튼 클릭 처리
    /// - 빈칸 검사 → 실패 메시지
    /// - 더미 인증(admin/1234) → 성공 시 메시지 출력 후 씬 이동
    /// </summary>
    void OnClickLogin()
    {
        Managers.Sound.Play("SFX/UI/Click", Define.Sound.Effect);//SFX추가
        // 입력값 읽기
        string id = Get<TMP_InputField>((int)Inputs.IdInput).text;
        string pw = Get<TMP_InputField>((int)Inputs.PwInput).text;

        // 1) 빈 입력 방지
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(pw))
        {
            ShowMessageFail("아이디/비밀번호를 입력해주세요");
            return;
        }

        // 2) 더미 로그인(나중에 서버 인증으로 교체 예정)
        bool ok = (id == "admin" && pw == "1234");

        // 3) 실패 처리
        if (!ok)
        {
            ShowMessageFail("아이디 또는 비밀번호가 틀렸습니다. \n 다시 입력해주세요.");
            return;
        }

        // 4) 성공 처리: Confirm 버튼 없이 메시지만 보여주고 잠시 후 Game 씬 이동
        ShowMessageSucess("로그인 성공! 잠시 후 이동합니다..");
        StartCoroutine(LoadGameAfterDelay(0.6f));
    }

    // ====== 메시지 패널 제어 유틸 ======

    // 메시지 패널 표시/숨김
    void SetMessagePanel(bool active)
    {
        Get<GameObject>((int)GameObjects.MessagePanel).SetActive(active);
    }

    // Confirm 버튼 표시/숨김
    // - 성공: false (사용자가 누를 필요 없음)
    // - 실패: true  (사용자가 확인 후 닫도록)
    void SetConfirmButton(bool active)
    {
        Get<Button>((int)Buttons.ConfirmButton).gameObject.SetActive(active);
    }

    // 메시지 텍스트 변경
    void SetMessageText(string msg)
    {
        Get<TMP_Text>((int)Texts.MessageText).text = msg;
    }

    /// <summary>
    /// 성공 메시지 표시
    /// - MessagePanel: ON
    /// - ConfirmButton: OFF (자동 이동 예정)
    /// </summary>
    void ShowMessageSucess(string msg)
    {
        SetMessagePanel(active: true);
        SetConfirmButton(active: false);
        SetMessageText(msg);
    }

    /// <summary>
    /// 실패 메시지 표시
    /// - MessagePanel: ON
    /// - ConfirmButton: ON (사용자가 확인하고 닫기)
    /// </summary>
    void ShowMessageFail(string msg)
    {
        SetMessagePanel(active: true);
        SetConfirmButton(active: true);
        SetMessageText(msg);
    }

    /// <summary>
    /// Confirm 버튼 클릭 처리(실패 케이스에서만 등장)
    /// - 메시지 패널 닫기
    /// - 다시 입력하기 편하게 ID 입력창 포커스
    /// </summary>
    void OnClickConfirmMessage()
    {
        Managers.Sound.Play("SFX/UI/Click", Define.Sound.Effect);//SFX추가

        SetMessagePanel(active: false);

        var id = Get<TMP_InputField>((int)Inputs.IdInput);
        id.Select();
        id.ActivateInputField();
    }

    /// <summary>
    /// 일정 시간 대기 후 Game 씬 로드
    /// - 성공 메시지를 잠깐 보여주기 위한 딜레이
    /// </summary>
    IEnumerator LoadGameAfterDelay(float sec)
    {
        yield return new WaitForSeconds(sec);
        Managers.Scene.LoadScene(Define.Scene.Game);
    }
}