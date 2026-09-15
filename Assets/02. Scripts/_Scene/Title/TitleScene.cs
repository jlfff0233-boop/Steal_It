using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 타이틀 씬 관리
/// </summary>
public class TitleScene : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TitleSceneView _view;       //타이틀 씬 뷰

    [Header( "----- 패널 -----" )]
    [SerializeField] GameObject _loadPanel;      //불러오기 패널
    [SerializeField] GameObject _settingPanel;   //세팅 패널

    [Header( "----- 메인 버튼 -----" )]
    [SerializeField] Button _startButton;       //시작 버튼
    [SerializeField] Button _loadButton;        //불러오기 버튼
    [SerializeField] Button _settingButton;     //설정 버튼
    [SerializeField] Button _exitButton;        //나가기 버튼

    [Header( "----- 불러오기 버튼 -----" )]
    [SerializeField] Button _loadConfirmButton;         //불러오기 확인 버튼
    [SerializeField] Button _loadCancelButton;          //불러오기 취소 버튼

    [Header( "----- 세팅 버튼 -----" )]
    [SerializeField] Button _settingConfirmButton;      //세팅 확인 버튼
    [SerializeField] Button _settingCancelButton;       //세팅 취소 버튼

    private void Awake ()
    {
        //버튼 이벤트 연결
        AddButtons( );

        //패널 초기화
        CloseAllPanels( );

        //저장 데이터 여부에 따라 불러오기 버튼 상태 갱신
        UpdateLoadButton( );
    }

    private void Start ()
    {
        //배경음악 재생
        GameManager.Instance.SoundManager.PlayBgm( BgmType.Title );
    }

    private void Update ()
    {
        ////0 키를 누르면 저장 데이터 초기화
        //if ( Input.GetKeyDown( KeyCode.Alpha0 ) )
        //{
        //    ResetSaveData( );
        //}
    }

    /// <summary>
    /// 저장 데이터 초기화
    /// </summary>
    public void ResetSaveData ()
    {
        //저장 데이터 삭제
        GameManager.Instance.SaveManager.DeleteData( );

        //불러오기 버튼 상태 갱신
        UpdateLoadButton( );
    }

    /// <summary>
    /// 버튼 이벤트 연결
    /// </summary>
    public void AddButtons ()
    {
        //메인 버튼 이벤트 연결
        _startButton.onClick.AddListener( ClickStartButton );
        _loadButton.onClick.AddListener( ClickLoadButton );
        _settingButton.onClick.AddListener( ClickSettingButton );
        _exitButton.onClick.AddListener( ClickExitButton );

        //불러오기 버튼 이벤트 연결
        _loadConfirmButton.onClick.AddListener( ClickLoadConfirmButton );
        _loadCancelButton.onClick.AddListener( ClickLoadCancelButton );

        //세팅 버튼 이벤트 연결
        _settingConfirmButton.onClick.AddListener( ClickSettingConfirmButton );
        _settingCancelButton.onClick.AddListener( ClickSettingCancelButton );
    }

    /// <summary>
    /// 패널 전체 닫기
    /// </summary>
    public void CloseAllPanels ()
    {
        //패널 초기 상태 설정
        _loadPanel.SetActive( false );
        _settingPanel.SetActive( false );
    }

    /// <summary>
    /// 불러오기 패널 열기
    /// </summary>
    public void OpenLoadPanel ()
    {
        //불러오기 패널 열기
        _loadPanel.SetActive( true );
    }

    /// <summary>
    /// 불러오기 패널 닫기
    /// </summary>
    public void CloseLoadPanel ()
    {
        //불러오기 패널 닫기
        _loadPanel.SetActive( false );
    }

    /// <summary>
    /// 세팅 패널 열기
    /// </summary>
    public void OpenSettingPanel ()
    {
        //세팅 패널 열기
        _settingPanel.SetActive( true );
    }

    /// <summary>
    /// 세팅 패널 닫기
    /// </summary>
    public void CloseSettingPanel ()
    {
        //세팅 패널 닫기
        _settingPanel.SetActive( false );
    }

    /// <summary>
    /// 플레이 버튼
    /// </summary>
    public void ClickStartButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //플레이 씬으로 이동
        SceneManager.LoadScene( "Play" );
    }

    /// <summary>
    /// 불러오기 버튼
    /// </summary>
    public void ClickLoadButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //저장 데이터 미리보기
        SaveData data = GameManager.Instance.SaveManager.GetPreviewData( );

        //저장 데이터를 패널 UI에 표시
        _view.UpdateLoadPanel( data.BestScore, data.SubmittedItemTotalCount );

        //불러오기 패널 열기
        OpenLoadPanel( );
    }

    /// <summary>
    /// 불러오기 확인 버튼
    /// </summary>
    public void ClickLoadConfirmButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //저장 데이터 적용
        GameManager.Instance.SaveManager.Load( );

        //플레이 씬으로 이동
        SceneManager.LoadScene( "Play" );
    }

    /// <summary>
    /// 불러오기 취소 버튼
    /// </summary>
    public void ClickLoadCancelButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //불러오기 패널 닫기
        CloseLoadPanel( );
    }

    /// <summary>
    /// 불러오기 버튼 갱신
    /// </summary>
    public void UpdateLoadButton ()
    {
        //저장 데이터가 없으면 불러오기 버튼 비활성화
        _loadButton.interactable = GameManager.Instance.SaveManager.HasSaveData;
    }

    /// <summary>
    /// 설정 열기 버튼
    /// </summary>
    public void ClickSettingButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        SoundManager soundManager = GameManager.Instance.SoundManager;

        //현재 사운드 값을 세팅 패널에 표시
        _view.UpdateSettingPanel(
            soundManager.BgmVolume,
            soundManager.SfxVolume,
            soundManager.IsBgmMuted,
            soundManager.IsSfxMuted );

        //세팅 패널 열기
        OpenSettingPanel( );
    }

    /// <summary>
    /// 세팅 확인 버튼
    /// </summary>
    public void ClickSettingConfirmButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //사운드 설정 적용
        GameManager.Instance.SoundManager.SetBgmVolume( _view.BgmVolume );
        GameManager.Instance.SoundManager.SetSfxVolume( _view.SfxVolume );
        GameManager.Instance.SoundManager.SetBgmMute( _view.IsBgmMuted );
        GameManager.Instance.SoundManager.SetSfxMute( _view.IsSfxMuted );

        //세팅 패널 닫기
        CloseSettingPanel( );
    }

    /// <summary>
    /// 세팅 취소 버튼
    /// </summary>
    public void ClickSettingCancelButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //세팅 패널 닫기
        CloseSettingPanel( );
    }

    /// <summary>
    /// 게임 종료 버튼
    /// </summary>
    public void ClickExitButton ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.UI );

        //게임 종료
        Application.Quit( );
    }
}
