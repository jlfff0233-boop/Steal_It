using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 세션 뷰 UI 갱신(무게바, 점수, 시간)
/// </summary>
public class PlaySceneView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _weightBarImage;     //무게바 이미지
    [SerializeField] TMP_Text _weightText;       //무게 텍스트
    [SerializeField] TMP_Text _currentScoreText;        //현재 점수 텍스트

    [SerializeField] TMP_Text _timeLeftText;        //남은 시간 텍스트
    [SerializeField] Image _timeLeftBar;        //남은 시간 바 이미지

    [SerializeField] GameObject [ ] _lives;     //목숨 게임오브젝트 배열

    [SerializeField] GameObject _resultPanel;       //결과 패널
    [SerializeField] RectTransform _resultPanelTransform;       //결과 패널 트랜스폼
    [SerializeField] TMP_Text _resultPanelTitleText;     //결과 텍스트
    [SerializeField] TMP_Text _recordText;      //기록 텍스트

    Tween _scoreTextTween;      //점수 텍스트 트윈
    Tween _timeTextTween;       //시간 텍스트 트윈
    Tween _weightBarTween;      //무게바 트윈
    Tween _lifeTween;        //목숨 UI 트윈
    Tween _resultPanelTween;        //결과 패널 트윈
    int _prevLifeCount;      //이전 목숨 수
    bool _isLifeInitialized;     //목숨 UI 초기화 여부
    Vector3 _lifeOriginScale;        //목숨 원본 크기

    /// <summary>
    /// 목숨 UI 원본 크기 초기화
    /// </summary>
    private void Awake ()
    {
        _lifeOriginScale = _lives [ 0 ].transform.localScale;
    }

    /// <summary>
    /// 점수 텍스트 연출
    /// </summary>
    public void PlayScoreTextTween ()
    {
        //기존 트윈 정리
        if ( _scoreTextTween != null ) _scoreTextTween.Kill( );

        //크기 초기화
        _currentScoreText.transform.localScale = Vector3.one;

        //점수 텍스트 크기 연출
        _scoreTextTween = _currentScoreText.transform.DOScale( 1.15f, 0.1f )
            .SetLoops( 2, LoopType.Yoyo )
            .SetEase( Ease.OutCubic );
    }

    /// <summary>
    /// 시간 보너스 연출
    /// </summary>
    public void PlayBonusTimeTween ()
    {
        //기존 트윈 정리
        _timeTextTween?.Kill( );

        //크기 초기화
        _timeLeftText.transform.localScale = Vector3.one;

        //시간 텍스트 두근 연출
        _timeTextTween = _timeLeftText.transform.DOScale( 1.25f, 0.12f )
            .SetLoops( 2, LoopType.Yoyo )
            .SetEase( Ease.OutCubic );
    }

    /// <summary>
    /// 무게바 UI 갱신
    /// </summary>
    /// <param name="currentWeight">현재 무게</param>
    /// <param name="maxWeight">최대 무게</param>
    public void UpdateWeightBar ( float currentWeight, float maxWeight )
    {
        //무게바 목표값 계산
        float targetFillAmount = currentWeight / maxWeight;

        //기존 트윈 정리
        if ( _weightBarTween != null ) _weightBarTween.Kill( );

        //무게바 이미지 갱신
        _weightBarTween = _weightBarImage.DOFillAmount( targetFillAmount, 0.2f )
            .SetEase( Ease.OutCubic );

        //무게바 텍스트 갱신
        _weightText.text = $"{currentWeight:F2} / {maxWeight:F2} Kg";
    }

    /// <summary>
    /// 점수 텍스트 갱신
    /// </summary>
    /// <param name="score">현재 점수</param>
    public void UpdateScoreText ( float currentScore )
    {
        _currentScoreText.text = $"현재 점수: {currentScore:F2}";
    }

    /// <summary>
    /// 목숨 이미지 갱신
    /// </summary>
    /// <param name="count">현재 목숨 수</param>
    public void UpdateHPImages ( int count )
    {
        if ( count > _lives.Length ) return;

        for ( int i = 0; i < _lives.Length; i++ )
        {
            //현재 목숨 수만큼 활성화
            _lives [ i ].SetActive( i < count );
        }

        //목숨 수가 바뀌었으면 연출
        if ( _isLifeInitialized == true && count != _prevLifeCount )
        {
            int tweenIndex = Mathf.Clamp( count - 1, 0, _lives.Length - 1 );

            PlayLifeTween( tweenIndex );
        }

        //이전 목숨 수 갱신
        _prevLifeCount = count;

        //목숨 UI 초기화 완료
        _isLifeInitialized = true;
    }

    /// <summary>
    /// 타이머 갱신
    /// </summary>
    /// <param name="remainingTime">남은 시간</param>
    public void UpdateRemainingTime ( float playTime, float clearTime )
    {
        //남은 시간 계산 및 음수 제한
        float remainingTime = Mathf.Max( 0f, clearTime - playTime );

        int minutes = Mathf.FloorToInt( remainingTime / 60 );
        int seconds = Mathf.FloorToInt( remainingTime % 60 );

        _timeLeftText.text = $"{minutes}:{seconds:00}";

        //남은 시간 바 갱신
        _timeLeftBar.fillAmount = remainingTime / clearTime;
    }

    /// <summary>
    /// 결과에 따라 텍스트 갱신
    /// </summary>
    /// <param name="text"></param>
    public void UpdateResultText ( string text, float score, int count )
    {
        _resultPanelTitleText.text = $"{text}";

        //현재 점수 및 제출 아이템 수
        _recordText.text = $"점수: {score:F2}\n" +
            $"제출 아이템 수: {count}\n" +
            $"최고 점수: {GameManager.Instance.SaveManager.BestScore:F2}";
    }

    /// <summary>
    /// 결과 패널 열기 연출
    /// </summary>
    public void PlayResultPanelTween ()
    {
        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Panel );

        //기존 트윈 정리
        if ( _resultPanelTween != null ) _resultPanelTween.Kill( );

        //크기 초기화
        _resultPanelTransform.localScale = Vector3.one * 0.8f;

        //패널 활성화
        _resultPanel.SetActive( true );

        //연출
        _resultPanelTween = _resultPanelTransform.DOScale( 1f, 0.2f ).SetEase( Ease.OutCubic );
    }

    /// <summary>
    /// 목숨 회복 연출
    /// </summary>
    /// <param name="index">연출할 목숨 인덱스</param>
    void PlayLifeTween ( int index )
    {
        //범위 보정
        if ( index < 0 || index >= _lives.Length ) return;

        //기존 트윈 정리
        _lifeTween?.Kill( );

        //크기 초기화
        _lives [ index ].transform.localScale = _lifeOriginScale;

        //목숨 두근 연출
        _lifeTween = _lives [ index ].transform.DOScale( _lifeOriginScale * 1.25f, 0.12f )
            .SetLoops( 2, LoopType.Yoyo )
            .SetEase( Ease.OutCubic );
    }

    /// <summary>
    /// 트윈 정리
    /// </summary>
    private void OnDestroy ()
    {
        //사용 중인 트윈 정리
        if ( _scoreTextTween != null ) _scoreTextTween.Kill( );
        if ( _timeTextTween != null ) _timeTextTween.Kill( );
        if ( _weightBarTween != null ) _weightBarTween.Kill( );
        if ( _lifeTween != null ) _lifeTween.Kill( );
        if ( _resultPanelTween != null ) _resultPanelTween.Kill( );
    }

}
