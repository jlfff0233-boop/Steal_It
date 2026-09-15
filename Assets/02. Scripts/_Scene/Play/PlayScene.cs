using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 플레이씬 담당 - 점수 갱신, 게임 클리어, 이벤트 중재
/// </summary>
public class PlayScene : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Hero _hero;        //히어로
    [SerializeField] Goal _goal;        //골

    [SerializeField] PlaySceneView _view;      //UI 담당
    [SerializeField] InventoryView _inventoryView;      //인벤토리 뷰

    [SerializeField] EnemySpawner _enemySpawner;        //적 스포너
    [SerializeField] ItemBoxSpawner _itemBoxSpawner;        //아이템 박스 스포너

    [SerializeField] TutorialDirector _tutorialDirector;      //튜토리얼 디렉터

    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] float _currentScore;       //현재 점수
    [SerializeField] int _submittedItemCount;       //이번 판 제출 아이템 수

    [SerializeField] float _playTime;       //플레이 시간
    [SerializeField] float _clearTime;      //클리어 시간

    [SerializeField] bool _isGameDone;      //게임 종료 여부
    [SerializeField] bool _isTimerActive = true;     //타이머 동작 여부
    [SerializeField] bool _isTutorialRunning;      //튜토리얼 진행 여부

    #region ----- 이벤트 -----
    /// <summary>
    /// 스테이지 점수 변경 이벤트(현재 점수)
    /// </summary>
    public event UnityAction<float> OnStageScoreChanged;
    /// <summary>
    /// 플레이 타임 변경 이벤트(플레이 시간, 클리어 시간)
    /// </summary>
    public event UnityAction<float, float> OnPlayTimeChanged;
    /// <summary>
    /// 결과 변경 이벤트(결과 텍스트, 현재 점수, 제출 아이템 수)
    /// </summary>
    public event UnityAction<string, float, int> OnResultChanged;
    /// <summary>
    /// 게임 종료 이벤트
    /// </summary>
    public event UnityAction OnGameDone;
    /// <summary>
    /// 인벤토리 아이템 제출 요청 이벤트
    /// </summary>
    public event UnityAction<Dictionary<ItemData, int>> OnInventoryItemSubmitRequested;
    /// <summary>
    /// 상호작용 아이템 제출 요청 이벤트
    /// </summary>
    public event UnityAction<Item> OnInteractItemSubmitRequested;

    #endregion

    private void Awake ()
    {
        #region --- 시작 ---
        //적, 아이템 박스 스폰
        _enemySpawner.SpawnEnemy( );
        _itemBoxSpawner.SpawnItemBox( );

        //점수 UI 갱신
        _view.UpdateScoreText( _currentScore );
        //무게 바 UI 갱신
        _view.UpdateWeightBar( _hero.Model.CurrentWeight, _hero.Model.MaxWeight );
        //인벤토리 UI 갱신
        _inventoryView.Clean( );

        //히어로 초기화
        _hero.Init( );

        #endregion

        #region --- 이벤트 ---
        //히어로 아이템 제출 이벤트 구독
        _hero.OnInventoryItemSubmitted += SubmitInventoryItem;
        _hero.OnInteractItemSubmitted += SubmitInteractItem;

        //점수 변경 이벤트 구독
        _goal.OnScoreChanged += AddScore;
        //제출 아이템 수 변경 이벤트 구독
        _goal.OnItemSubmitted += AddSubmittedItemCount;

        //골 제출 요청 이벤트 구독
        OnInventoryItemSubmitRequested += _goal.SubmitInventoryItem;
        OnInteractItemSubmitRequested += _goal.SubmitInteractItem;

        //목숨 변경 이벤트 구독
        _hero.OnLifeChanged += GameOver;
        //보너스 시간 획득 이벤트 구독
        _hero.OnGetBonusTime += AddBonusTime;

        //아이템 획득 이벤트 구독
        _hero.OnItemCollected += AddCollectedItemCount;
        //보너스 목숨 획득 이벤트 구독
        _hero.OnGetBonusLife += AddBonusLifeCount;

        //튜토리얼 이벤트 구독
        _hero.OnJumped += _tutorialDirector.Jumped;
        _hero.OnFastFalled += _tutorialDirector.FastFalled;
        _hero.OnDashed += _tutorialDirector.Dashed;
        _hero.OnAttacked += _tutorialDirector.Attacked;
        _hero.OnHoldItem += _tutorialDirector.HoldItem;
        _hero.OnPutDownItem += _tutorialDirector.PutDownItem;
        _hero.OnThrownItem += _tutorialDirector.ThrownItem;
        _hero.OnPulledItem += _tutorialDirector.PulledItem;
        _hero.OnItemCollected += _tutorialDirector.OnItemCollected;
        _hero.OnInventoryItemSubmitted += _tutorialDirector.InventoryItemSubmitted;
        _hero.OnInteractItemSubmitted += _tutorialDirector.InteractItemSubmitted;
        _tutorialDirector.OnTutorialStarted += StartTutorial;
        _tutorialDirector.OnTutorialEnded += EndTutorial;
        _tutorialDirector.OnMoveToMainMapRequested += _hero.SetRespawnPoint;

        //UI 갱신
        //히어로 목숨 변경 이벤트 구독
        _hero.OnLifeChanged += _view.UpdateHPImages;
        //히어로 무게 바 변경 이벤트 구독
        _hero.OnWeightChanged += _view.UpdateWeightBar;
        //인벤토리 갱신 이벤트 구독
        _hero.OnInventoryChanged += _inventoryView.UpdateInventory;
        //점수 갱신 이벤트 구독
        OnStageScoreChanged += _view.UpdateScoreText;
        //타이머 갱신 이벤트 구독
        OnPlayTimeChanged += _view.UpdateRemainingTime;
        //결과 텍스트 갱신 이벤트 구독
        OnResultChanged += _view.UpdateResultText;

        //게임 종료 이벤트 구독
        OnGameDone += _enemySpawner.RemoveEnemies;
        OnGameDone += StopHeroControl;
        OnGameDone += StopSpawners;
        #endregion

        #region --- 튜토리얼 시작 ---
        //튜토리얼 디렉터 초기화
        _tutorialDirector.Init( _hero );
        #endregion
    }

    private void Start ()
    {
        //배경음악 재생
        GameManager.Instance.SoundManager.PlayBgm( BgmType.Stage );
    }

    /// <summary>
    /// 파괴 시 튜토리얼 이벤트 구독 해제
    /// </summary>
    private void OnDestroy ()
    {
        //히어로 아이템 제출 이벤트 구독 해제
        _hero.OnInventoryItemSubmitted -= SubmitInventoryItem;
        _hero.OnInteractItemSubmitted -= SubmitInteractItem;

        //골 제출 요청 이벤트 구독 해제
        OnInventoryItemSubmitRequested -= _goal.SubmitInventoryItem;
        OnInteractItemSubmitRequested -= _goal.SubmitInteractItem;

        //튜토리얼 이벤트 구독 해제
        _hero.OnJumped -= _tutorialDirector.Jumped;
        _hero.OnFastFalled -= _tutorialDirector.FastFalled;
        _hero.OnDashed -= _tutorialDirector.Dashed;
        _hero.OnAttacked -= _tutorialDirector.Attacked;
        _hero.OnHoldItem -= _tutorialDirector.HoldItem;
        _hero.OnPutDownItem -= _tutorialDirector.PutDownItem;
        _hero.OnThrownItem -= _tutorialDirector.ThrownItem;
        _hero.OnPulledItem -= _tutorialDirector.PulledItem;
        _hero.OnItemCollected -= _tutorialDirector.OnItemCollected;
        _hero.OnInventoryItemSubmitted -= _tutorialDirector.InventoryItemSubmitted;
        _hero.OnInteractItemSubmitted -= _tutorialDirector.InteractItemSubmitted;
        _tutorialDirector.OnTutorialStarted -= StartTutorial;
        _tutorialDirector.OnTutorialEnded -= EndTutorial;
        _tutorialDirector.OnMoveToMainMapRequested -= _hero.SetRespawnPoint;
    }

    private void Update ()
    {
        //if ( Input.GetKeyDown( KeyCode.Alpha0 ) )
        //{
        //    ClickRestartButton( );
        //}

        //게임이 끝났으면 종료
        if ( _isGameDone == true ) return;

        UpdatePlayTime( );
    }


    #region ----- 플레이 -----

    /// <summary>
    /// 튜토리얼 시작
    /// </summary>
    void StartTutorial ()
    {
        //튜토리얼 진행 상태 설정
        _isTutorialRunning = true;

        //타이머 정지
        SetTimerActive( false );
    }

    /// <summary>
    /// 튜토리얼 종료
    /// </summary>
    void EndTutorial ()
    {
        //튜토리얼 진행 상태 해제
        _isTutorialRunning = false;

        //타이머 재개
        SetTimerActive( true );
    }

    /// <summary>
    /// 인벤토리 아이템 제출
    /// </summary>
    /// <param name="inventory">인벤토리 아이템</param>
    void SubmitInventoryItem ( Dictionary<ItemData, int> inventory )
    {
        //튜토리얼 중이면 점수 제출 생략
        if ( _isTutorialRunning == true ) return;

        //인벤토리 아이템 제출 요청 이벤트 발행
        OnInventoryItemSubmitRequested?.Invoke( inventory );
    }

    /// <summary>
    /// 상호작용 아이템 제출
    /// </summary>
    /// <param name="item">상호작용 아이템</param>
    void SubmitInteractItem ( Item item )
    {
        //튜토리얼 중이면 아이템 제출만 처리
        if ( _isTutorialRunning == true )
        {
            item.Submit( );
            return;
        }

        //상호작용 아이템 제출 요청 이벤트 발행
        OnInteractItemSubmitRequested?.Invoke( item );
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    /// <param name="score">현재 점수</param>
    public void AddScore ( float score )
    {
        //현재 점수 설정
        _currentScore = score;

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Submit );

        //스테이지 점수 변경 이벤트 발행
        OnStageScoreChanged?.Invoke( _currentScore );

        //점수 텍스트 연출
        _view.PlayScoreTextTween( );
    }

    /// <summary>
    /// 제출 아이템 수 증가
    /// </summary>
    /// <param name="count">제출 아이템 수</param>
    public void AddSubmittedItemCount ( int count )
    {
        //제출 아이템 수 증가
        _submittedItemCount += count;
    }

    /// <summary>
    /// 타이머 동작 상태 설정
    /// </summary>
    /// <param name="isActive">타이머 동작 여부</param>
    public void SetTimerActive ( bool isActive )
    {
        _isTimerActive = isActive;
    }

    /// <summary>
    /// 플레이 시간 갱신
    /// </summary>
    public void UpdatePlayTime ()
    {
        //게임이 끝났으면 종료
        if ( _isGameDone == true ) return;

        //타이머가 비활성화면 종료
        if ( _isTimerActive == false ) return;

        _playTime += Time.deltaTime;

        OnPlayTimeChanged?.Invoke( _playTime, _clearTime );

        //시간이 끝났으면 클리어
        if ( _playTime >= _clearTime )
        {
            StageClear( );
        }
    }

    /// <summary>
    /// 보너스 타임 추가
    /// </summary>
    /// <param name="bonus"></param>
    public void AddBonusTime ( float bonus )
    {
        //클리어 시간 총량 증가
        _clearTime += bonus;

        //누적 보너스 시간 저장
        GameManager.Instance.SaveManager.AddBonusTimeAmount( bonus );

        //UI 갱신
        OnPlayTimeChanged?.Invoke( _playTime, _clearTime );

        //시간 보너스 연출
        _view.PlayBonusTimeTween( );
    }

    /// <summary>
    /// 획득 아이템 수 누적
    /// </summary>
    public void AddCollectedItemCount ()
    {
        //튜토리얼 중이면 누적 획득 수에 반영하지 않음
        if ( _isTutorialRunning == true ) return;

        GameManager.Instance.SaveManager.AddCollectedItemCount( );
    }

    /// <summary>
    /// 보너스 목숨 수 누적
    /// </summary>
    public void AddBonusLifeCount ()
    {
        GameManager.Instance.SaveManager.AddBonusLifeCount( );
    }
    #endregion

    #region ----- 게임 클리어/오버 -----

    /// <summary>
    /// 히어로 조작 정지
    /// </summary>
    void StopHeroControl ()
    {
        //히어로 업데이트 정지
        _hero.enabled = false;
    }

    /// <summary>
    /// 스포너 정지
    /// </summary>
    void StopSpawners ()
    {
        //적 스포너 정지
        _enemySpawner.enabled = false;

        //아이템 박스 스포너 정지
        _itemBoxSpawner.enabled = false;
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    /// <param name="resultText">결과 텍스트</param>
    /// <param name="isClear">클리어 여부</param>
    void FinishGame ( string resultText, bool isClear )
    {
        //이미 게임이 끝났으면 종료
        if ( _isGameDone == true ) return;

        //게임 종료 상태로 변경
        _isGameDone = true;

        //최고 점수와 제출 아이템 수 저장
        GameManager.Instance.SaveManager.SaveBestResult( _currentScore, _submittedItemCount, isClear );

        //결과 변경 이벤트 발행
        OnResultChanged?.Invoke( resultText, _currentScore, _submittedItemCount );

        //게임 종료 이벤트 발행
        OnGameDone?.Invoke( );

        //결과 패널 활성화
        _view.PlayResultPanelTween( );
    }

    /// <summary>
    /// 스테이지 클리어
    /// </summary>
    public void StageClear ()
    {
        //게임 클리어 처리
        FinishGame( "Game Clear !", true );
    }

    /// <summary>
    /// 게임오버
    /// </summary>
    public void GameOver ( int count )
    {
        //목숨이 남아 있으면 종료
        if ( count > 0 ) return;

        //게임 오버 처리
        FinishGame( "Game Over !", false );
    }

    /// <summary>
    /// 타이틀로 돌아간다
    /// </summary>
    public void ClickClearButton ()
    {
        SceneManager.LoadScene( "Clear" );
    }

    /// <summary>
    /// 재시작
    /// </summary>
    public void ClickRestartButton ()
    {
        SceneManager.LoadScene( "Play" );
    }

    #endregion
}
