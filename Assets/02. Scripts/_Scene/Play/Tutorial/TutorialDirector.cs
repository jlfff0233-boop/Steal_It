using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 진행 순서
/// </summary>
public enum TutorialStep
{
    Jump,       //점프
    FastFall,       //빠른 하강
    Dash,       //대시
    Attack,     //공격
    PickUpItem,     //줍기
    HoldItem,       //들기
    PutDownItem,        //내려놓기
    PullReadyInfo,      //당기기 준비 안내
    PullItem,       //당기기
    ThrowReadyInfo,     //던지기 준비 안내
    ThrowItem,      //던지기
    EnemyStunInfo,      //적 스턴 안내
    ItemBreakInfo,      //아이템 파괴 안내
    SubmitItem,     //제출
    ShowGoal,       //목표 제시
    ShowMapCamera      //맵 카메라 표시
}

/// <summary>
/// 튜토리얼 연출 디렉터 - 연출 진행 순서
/// </summary>
public class TutorialDirector : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] TutorialView _view;      //튜토리얼 UI
    [SerializeField] CameraCtrl _cameraCtrl;      //카메라 제어
    [SerializeField] Enemy _tutorialEnemy;      //튜토리얼 적
    [SerializeField] Transform _mainMapStartPoint;        //본 맵 시작 위치

    [Header( "----- 튜토리얼 진행 -----" )]
    [SerializeField] TutorialStep _step;      //현재 튜토리얼 단계
    [SerializeField] float _goalGuideTime;       //목표 안내 시간
    [SerializeField] float _cameraViewDelay;     //카메라 표시 대기 시간
    [SerializeField] Transform [ ] _cameraViewPoints;     //카메라 표시 지점

    float _stepTimer;       //단계 진행 시간
    bool _isRunning;        //튜토리얼 진행 여부
    Coroutine _cameraRoutine;      //카메라 연출 코루틴
    bool _hasJumped;        //점프 완료 여부
    bool _hasFastFalled;        //빠른 하강 완료 여부
    bool _hasDashed;        //대시 완료 여부
    bool _hasAttacked;      //공격 완료 여부
    bool _hasCollectedItem;     //아이템 획득 완료 여부
    bool _hasHeldItem;      //아이템 들기 완료 여부
    bool _hasPutDownItem;       //아이템 내려놓기 완료 여부
    bool _hasPulledItem;        //아이템 당기기 완료 여부
    bool _hasThrownItem;        //아이템 던지기 완료 여부
    bool _hasSubmittedItem;     //아이템 제출 완료 여부

    /// <summary>
    /// 튜토리얼 시작 이벤트
    /// </summary>
    public event Action OnTutorialStarted;
    /// <summary>
    /// 튜토리얼 종료 이벤트
    /// </summary>
    public event Action OnTutorialEnded;
    /// <summary>
    /// 메인 맵 이동 요청 이벤트
    /// </summary>
    public event Action<Transform> OnMoveToMainMapRequested;

    /// <summary>
    /// 초기화
    /// </summary>
    /// <param name="hero">히어로</param>
    public void Init ( Hero hero )
    {
        //튜토리얼 적 초기화
        _tutorialEnemy.Init( hero );

        //이미 튜토리얼을 봤거나 저장 데이터가 있으면 종료
        if ( GameManager.Instance.SaveManager.HasSeenTutorial == true
            || GameManager.Instance.SaveManager.HasSaveData == true )
        {
            //튜토리얼 진행 상태 해제
            _isRunning = false;

            //튜토리얼 UI 전체 숨김
            _view.HideTutorial( );

            //메인 맵 이동 요청 이벤트 발행
            OnMoveToMainMapRequested?.Invoke( _mainMapStartPoint );

            //카메라 히어로 추적 복구
            _cameraCtrl.FollowHero( );
            return;
        }

        StartTutorial( );
    }

    /// <summary>
    /// 단계 확인
    /// </summary>
    /// <returns>시간 진행 단계 여부</returns>
    bool IsTimedStep ()
    {
        return _step == TutorialStep.PullReadyInfo
            || _step == TutorialStep.ThrowReadyInfo
            || _step == TutorialStep.EnemyStunInfo
            || _step == TutorialStep.ItemBreakInfo
            || _step == TutorialStep.ShowGoal;
    }

    /// <summary>
    /// 다음 단계 반환
    /// </summary>
    /// <returns>다음 튜토리얼 단계</returns>
    TutorialStep GetNextTimedStep ()
    {
        //당기기 준비 안내 다음은 당기기
        if ( _step == TutorialStep.PullReadyInfo ) return TutorialStep.PullItem;

        //던지기 준비 안내 다음은 던지기
        if ( _step == TutorialStep.ThrowReadyInfo ) return TutorialStep.ThrowItem;

        //적 스턴 안내 다음은 아이템 파괴 안내
        if ( _step == TutorialStep.EnemyStunInfo ) return TutorialStep.ItemBreakInfo;

        //아이템 파괴 안내 다음은 제출 안내
        if ( _step == TutorialStep.ItemBreakInfo ) return TutorialStep.SubmitItem;

        //목표 안내 다음은 맵 카메라 표시
        return TutorialStep.ShowMapCamera;
    }

    private void Update ()
    {
        //튜토리얼 진행 중이 아니면 종료
        if ( _isRunning == false ) return;

        //시간 진행 단계가 아니면 종료
        if ( IsTimedStep( ) == false ) return;

        //단계 시간 갱신
        _stepTimer += Time.deltaTime;

        //안내 시간이 끝나면 다음 단계로 이동
        if ( _stepTimer >= _goalGuideTime )
        {
            ChangeStep( GetNextTimedStep( ) );
        }
    }

    /// <summary>
    /// 파괴 시 카메라 연출 코루틴을 정리
    /// </summary>
    private void OnDestroy ()
    {
        //카메라 연출 코루틴 정리
        if ( _cameraRoutine != null )
        {
            StopCoroutine( _cameraRoutine );
            _cameraRoutine = null;
        }
    }

    /// <summary>
    /// 맵 카메라 연출 실행
    /// </summary>
    IEnumerator ShowMapCameraRoutine ()
    {
        //카메라 제어가 없으면 튜토리얼 종료
        if ( _cameraCtrl == null )
        {
            _cameraRoutine = null;
            EndTutorial( );
            yield break;
        }
        //카메라 포인트 순서대로 추적
        for ( int i = 0; i < _cameraViewPoints.Length; i++ )
        {
            //카메라 표시 지점이 없으면 다음으로 넘김
            if ( _cameraViewPoints [ i ] == null ) continue;

            //카메라 추적 대상을 표시 지점으로 변경
            _cameraCtrl.SetFollowTarget( _cameraViewPoints [ i ] );

            //카메라 표시 대기
            yield return new WaitForSeconds( _cameraViewDelay );
        }

        //메인 맵 이동 요청 이벤트 발행
        OnMoveToMainMapRequested?.Invoke( _mainMapStartPoint );

        //메인 시작 위치 설정
        _cameraCtrl.SetFollowTarget( _mainMapStartPoint );

        //메인 시작 위치 표시 대기
        yield return new WaitForSeconds( _cameraViewDelay );

        //히어로 추적 복구
        _cameraCtrl.FollowHero( );
        //카메라 연출 코루틴 초기화
        _cameraRoutine = null;

        //튜토리얼 종료
        EndTutorial( );
    }

    /// <summary>
    /// 튜토리얼 시작
    /// </summary>
    public void StartTutorial ()
    {
        //행동 완료 상태 초기화
        ResetStepFlags( );

        //튜토리얼 시작 이벤트 발행
        OnTutorialStarted?.Invoke( );

        //튜토리얼 진행 상태 설정
        _isRunning = true;

        //튜토리얼 적 정지
        if ( _tutorialEnemy != null )
        {
            _tutorialEnemy.SetTutorialIdle( true );
        }

        //첫 단계 표시
        ChangeStep( TutorialStep.Jump );
    }

    /// <summary>
    /// 튜토리얼 종료
    /// </summary>
    public void EndTutorial ()
    {
        //튜토리얼 종료 상태 설정
        _isRunning = false;

        //튜토리얼 종료 이벤트 발행
        OnTutorialEnded?.Invoke( );

        //안내 숨김
        _view.HideGuide( );

        //메인 맵 배경음악 처음부터 재생
        GameManager.Instance.SoundManager.PlayBgm( BgmType.Stage );

        //튜토리얼 완료 저장
        GameManager.Instance.SaveManager.SetHasSeenTutorial( );
        GameManager.Instance.SaveManager.Save( );

        //튜토리얼 적 정지 해제
        if ( _tutorialEnemy != null )
        {
            _tutorialEnemy.SetTutorialIdle( false );
        }

    }

    /// <summary>
    /// 행동 완료 상태 초기화
    /// </summary>
    void ResetStepFlags ()
    {
        _hasJumped = false;
        _hasFastFalled = false;
        _hasDashed = false;
        _hasAttacked = false;
        _hasCollectedItem = false;
        _hasHeldItem = false;
        _hasPutDownItem = false;
        _hasPulledItem = false;
        _hasThrownItem = false;
        _hasSubmittedItem = false;
    }

    /// <summary>
    /// 현재 단계 완료 여부 확인
    /// </summary>
    /// <returns>완료 여부</returns>
    bool IsCurrentStepDone ()
    {
        switch ( _step )
        {
            case TutorialStep.Jump:
                return _hasJumped;

            case TutorialStep.FastFall:
                return _hasFastFalled;

            case TutorialStep.Dash:
                return _hasDashed;

            case TutorialStep.Attack:
                return _hasAttacked;

            case TutorialStep.PickUpItem:
                return _hasCollectedItem;

            case TutorialStep.HoldItem:
                return _hasHeldItem;

            case TutorialStep.PutDownItem:
                return _hasPutDownItem;

            case TutorialStep.PullItem:
                return _hasPulledItem;

            case TutorialStep.ThrowItem:
                return _hasThrownItem;

            case TutorialStep.SubmitItem:
                return _hasSubmittedItem;
        }

        return false;
    }

    /// <summary>
    /// 다음 행동 단계로 이동
    /// </summary>
    void ChangeNextActionStep ()
    {
        switch ( _step )
        {
            case TutorialStep.Jump:
                ChangeStep( TutorialStep.FastFall );
                break;

            case TutorialStep.FastFall:
                ChangeStep( TutorialStep.Dash );
                break;

            case TutorialStep.Dash:
                ChangeStep( TutorialStep.Attack );
                break;

            case TutorialStep.Attack:
                ChangeStep( TutorialStep.PickUpItem );
                break;

            case TutorialStep.PickUpItem:
                ChangeStep( TutorialStep.HoldItem );
                break;

            case TutorialStep.HoldItem:
                ChangeStep( TutorialStep.PutDownItem );
                break;

            case TutorialStep.PutDownItem:
                ChangeStep( TutorialStep.PullReadyInfo );
                break;

            case TutorialStep.PullItem:
                ChangeStep( TutorialStep.ThrowReadyInfo );
                break;

            case TutorialStep.ThrowItem:
                ChangeStep( TutorialStep.EnemyStunInfo );
                break;

            case TutorialStep.SubmitItem:
                ChangeStep( TutorialStep.ShowGoal );
                break;
        }
    }

    /// <summary>
    /// 완료된 단계면 넘김
    /// </summary>
    void CheckCurrentStepDone ()
    {
        if ( IsCurrentStepDone( ) == false ) return;

        ChangeNextActionStep( );
    }

    /// <summary>
    /// 튜토리얼 단계 변경
    /// </summary>
    /// <param name="step">변경할 단계</param>
    void ChangeStep ( TutorialStep step )
    {
        //튜토리얼 진행 중이 아니면 단계 변경하지 않음
        if ( _isRunning == false ) return;

        //단계 설정
        _step = step;

        //단계 시간 초기화
        _stepTimer = 0f;

        //현재 단계 안내 표시
        ShowCurrentStep( );

        //이미 완료된 단계면 넘김
        CheckCurrentStepDone( );
    }

    /// <summary>
    /// 현재 단계 안내 표시
    /// </summary>
    void ShowCurrentStep ()
    {
        switch ( _step )
        {
            case TutorialStep.Jump:
                _view.ShowJumpGuide( );
                break;

            case TutorialStep.FastFall:
                _view.ShowFastFallGuide( );
                break;

            case TutorialStep.Dash:
                _view.ShowDashGuide( );
                break;

            case TutorialStep.Attack:
                _view.ShowAttackGuide( );
                break;

            case TutorialStep.PickUpItem:
                _view.ShowPickUpGuide( );
                break;

            case TutorialStep.HoldItem:
                _view.ShowHoldGuide( );
                break;

            case TutorialStep.PutDownItem:
                _view.ShowPutDownItem( );
                break;

            case TutorialStep.PullReadyInfo:
                _view.ShowPullReadyGuide( );
                break;

            case TutorialStep.PullItem:
                _view.ShowPullGuide( );
                break;

            case TutorialStep.ThrowReadyInfo:
                _view.ShowThrowReadyGuide( );
                break;

            case TutorialStep.ThrowItem:
                _view.ShowThrowGuide( );
                break;

            case TutorialStep.EnemyStunInfo:
                _view.ShowEnemyStunGuide( );
                break;

            case TutorialStep.ItemBreakInfo:
                _view.ShowItemBreakGuide( );
                break;

            case TutorialStep.SubmitItem:
                _view.ShowSubmitGuide( );
                break;

            case TutorialStep.ShowGoal:
                _view.ShowGoalGuide( );
                break;

            case TutorialStep.ShowMapCamera:
                //카메라 연출 코루틴이 없을 때만 시작
                if ( _cameraRoutine == null )
                {
                    _cameraRoutine = StartCoroutine( ShowMapCameraRoutine( ) );
                }
                break;
        }
    }

    /// <summary>
    /// 점프 성공 시 다음 단계로 이동
    /// </summary>
    public void Jumped ()
    {
        //점프 완료
        _hasJumped = true;

        //점프 단계가 아니면 종료
        if ( _step != TutorialStep.Jump ) return;

        //빠른 하강 안내로 이동
        ChangeStep( TutorialStep.FastFall );
    }

    /// <summary>
    /// 빠른 하강 성공 시 다음 단계로 이동
    /// </summary>
    public void FastFalled ()
    {
        //빠른 하강 완료
        _hasFastFalled = true;

        //빠른 하강 단계가 아니면 종료
        if ( _step != TutorialStep.FastFall ) return;

        //대시 안내로 이동
        ChangeStep( TutorialStep.Dash );
    }

    /// <summary>
    /// 대시 성공 시 다음 단계로 이동
    /// </summary>
    public void Dashed ()
    {
        //대시 완료
        _hasDashed = true;

        //대시 단계가 아니면 종료
        if ( _step != TutorialStep.Dash ) return;

        //공격 안내로 이동
        ChangeStep( TutorialStep.Attack );
    }

    /// <summary>
    /// 공격 성공 시 다음 단계로 이동
    /// </summary>
    public void Attacked ()
    {
        //공격 완료
        _hasAttacked = true;

        //공격 단계가 아니면 종료
        if ( _step != TutorialStep.Attack ) return;

        //아이템 획득 안내로 이동
        ChangeStep( TutorialStep.PickUpItem );
    }

    /// <summary>
    /// 아이템 들기 성공 시 다음 단계로 이동
    /// </summary>
    public void HoldItem ()
    {
        //아이템 들기 완료
        _hasHeldItem = true;

        //아이템 들기 단계가 아니면 종료
        if ( _step != TutorialStep.HoldItem ) return;

        //내려놓기 안내로 이동
        ChangeStep( TutorialStep.PutDownItem );
    }

    /// <summary>
    /// 아이템 내려놓기 성공 시 다음 단계로 이동
    /// </summary>
    public void PutDownItem ()
    {
        //아이템 내려놓기 완료
        _hasPutDownItem = true;

        //아이템 내려놓기 단계가 아니면 종료
        if ( _step != TutorialStep.PutDownItem ) return;

        //당기기 준비 안내로 이동
        ChangeStep( TutorialStep.PullReadyInfo );
    }

    /// <summary>
    /// 아이템 당기기 성공 시 다음 단계로 이동
    /// </summary>
    public void PulledItem ()
    {
        //아이템 당기기 완료
        _hasPulledItem = true;

        //아이템 당기기 단계가 아니면 종료
        if ( _step != TutorialStep.PullItem ) return;

        //던지기 준비 안내로 이동
        ChangeStep( TutorialStep.ThrowReadyInfo );
    }

    /// <summary>
    /// 아이템 던지기 성공 시 적 스턴 안내로 이동
    /// </summary>
    public void ThrownItem ()
    {
        //아이템 던지기 완료
        _hasThrownItem = true;

        //아이템 던지기 단계가 아니면 종료
        if ( _step != TutorialStep.ThrowItem ) return;

        //적 스턴 안내로 이동
        ChangeStep( TutorialStep.EnemyStunInfo );
    }

    /// <summary>
    /// 아이템 획득 시 다음 단계로 이동
    /// </summary>
    public void OnItemCollected ()
    {
        //아이템 획득 완료
        _hasCollectedItem = true;

        //아이템 획득 단계가 아니면 종료
        if ( _step != TutorialStep.PickUpItem ) return;

        //아이템 들기 안내로 이동
        ChangeStep( TutorialStep.HoldItem );
    }

    /// <summary>
    /// 인벤토리 아이템 제출 시 다음 단계로 이동
    /// </summary>
    /// <param name="inventory">제출한 인벤토리</param>
    public void InventoryItemSubmitted ( Dictionary<ItemData, int> inventory )
    {
        //아이템 제출 단계가 아니면 종료
        if ( _step != TutorialStep.SubmitItem ) return;

        //아이템 제출 완료
        _hasSubmittedItem = true;

        //목표 안내로 이동
        ChangeStep( TutorialStep.ShowGoal );
    }

    /// <summary>
    /// 상호작용 아이템 제출 시 다음 단계로 이동
    /// </summary>
    /// <param name="item">제출한 아이템</param>
    public void InteractItemSubmitted ( Item item )
    {
        //아이템 제출 단계가 아니면 종료
        if ( _step != TutorialStep.SubmitItem ) return;

        //아이템 제출 완료
        _hasSubmittedItem = true;

        //목표 안내로 이동
        ChangeStep( TutorialStep.ShowGoal );
    }
}
