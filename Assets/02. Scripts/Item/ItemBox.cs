using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


/// <summary>
/// 아이템 박스 - 피격, 파괴, 아이템 드롭
/// </summary>
public class ItemBox : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image _hpBarImage;
    [SerializeField] Animator _anim;
    [SerializeField] DamageViewSpawner _damageViewSpawner;

    [Header( "----- 프리팹 -----" )]
    [SerializeField] Item [ ] _items;       //아이템 배열
    [SerializeField] GameObject [ ] _bonusItems;        //보너스 아이템 배열

    [Header( "----- 런타임 데이터 -----" )]
    [Header( "--- 피격 ---" )]
    [SerializeField] float _maxHp;      //최대 체력
    [SerializeField] float _currentHp;      //현재 체력
    [SerializeField] bool _isOpened;        //개봉 여부
    [SerializeField] int _hitCount;     //피격 횟수

    [SerializeField] bool _isTutorialBox;      //튜토리얼 박스 여부

    [Header( "--- 아이템 스폰 개수 ---" )]
    [SerializeField] int _minCount;     //최소 개수
    [SerializeField] int _maxCount;     //최대 개수

    [Header( "--- 아이템 드롭 위치 ---" )]
    [SerializeField] float _dropRadius;     //드롭 좌우 간격
    [SerializeField] float _dropUpOffset;       //드롭 위쪽 보정 값

    [Header( "--- 보너스 아이템 ---" )]
    [SerializeField] float _bonusItemRate;      //보너스 아이템 스폰 확률


    /// <summary>
    /// 체력 바 트윈
    /// </summary>
    Tween _hpBarTween;
    /// <summary>
    /// 박스 피격 트윈
    /// </summary>
    Tween _hitTween;
    Vector3 _originScale;      //원래 크기
    bool _isInited;        //초기화 여부


    #region ----- 이벤트 -----
    /// <summary>
    /// 개봉 이벤트
    /// </summary>
    public static event UnityAction<ItemBox> OnOpened;
    /// <summary>
    /// 피격 상자 알림 이벤트
    /// </summary>
    public static event UnityAction<ItemBox> OnHasHit;
    /// <summary>
    /// 피격 횟수 변경 이벤트
    /// </summary>
    public event UnityAction<int> OnHitCountChanged;
    /// <summary>
    /// 피격 이벤트
    /// </summary>
    public event UnityAction<float> OnHit;

    #endregion

    #region ----- 프로퍼티 -----
    /// <summary>
    /// 피격 횟수
    /// </summary>
    public int HitCount => _hitCount;
    /// <summary>
    /// 열렸는지 여부
    /// </summary>
    public bool IsOpened => _isOpened;
    #endregion


    /// <summary>
    /// 아이템 박스 초기화
    /// </summary>
    public void Init ()
    {
        //이미 초기화됐으면 종료
        if ( _isInited == true ) return;

        //초기화 상태 처리
        _isInited = true;

        //원래 크기 저장
        _originScale = transform.localScale;

        //체력 초기화
        _currentHp = _maxHp;

        //개봉 상태 초기화
        _isOpened = false;

        //피격 횟수 초기화
        _hitCount = 0;

        //대미지 뷰 이벤트 구독
        if ( _damageViewSpawner != null )
        {
            OnHit += _damageViewSpawner.SpawnDamageView;
        }

        //체력바 갱신
        UpdateHpBar( );
    }

    #region ----- 피격 -----
    /// <summary>
    /// 피격
    /// </summary>
    /// <param name="damage"></param>
    public void TakeHit ( float damage )
    {
        //대미지 입기
        //대미지를 입지 않았으면 종료
        if ( TakeDamage( damage ) == false ) return;

        //피격 횟수 증가
        _hitCount++;

        //피격 횟수 변경 이벤트 발행
        OnHitCountChanged?.Invoke( _hitCount );
        //피격 상자 알림 이벤트 발행
        OnHasHit?.Invoke( this );
        //피격 이벤트 발행
        OnHit?.Invoke( damage );

        //체력바 갱신
        UpdateHpBar( );

        //피격 연출
        PlayHitTween( );

        //튜토리얼 박스는 한 대 맞으면 바로 개봉
        if ( _isTutorialBox == true )
        {
            Open( );
            return;
        }

        //체력이 0보다 크면 종료
        if ( _currentHp > 0 ) return;

        //개봉
        Open( );
    }

    /// <summary>
    /// 대미지를 입는다
    /// </summary>
    /// <param name="damage">대미지</param>
    public bool TakeDamage ( float damage )
    {
        //음수 대미지 차단
        if ( damage < 0 ) return false;

        //개봉됐으면 종료
        if ( _isOpened == true ) return false;

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Hit );

        //체력 범위 설정
        _currentHp = Mathf.Clamp( _currentHp - damage, 0, _maxHp );

        return true;
    }

    /// <summary>
    /// 피격 횟수 초기화
    /// </summary>
    public void ResetHitCount ()
    {
        _hitCount = 0;
        OnHitCountChanged?.Invoke( _hitCount );
    }

    /// <summary>
    /// 이벤트 발행, 아이템 스폰, 스스로 파괴
    /// </summary>
    public void Open ()
    {
        //이미 열렸으면 종료
        if ( _isOpened == true ) return;

        //개봉 상태 처리
        _isOpened = true;

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Open );

        //애니메이션 재생
        _anim.SetTrigger( "Open" );

        //상자 열림 이벤트 발행
        OnOpened?.Invoke( this );

        //아이템 스폰
        SpawnItems( );

        //튜토리얼 박스가 아니면 보너스 아이템 스폰
        if ( _isTutorialBox == false )
        {
            SpawnBonusItems( );
        }

        Destroy( gameObject, 0.5f );
    }

    /// <summary>
    /// 피격 연출
    /// </summary>
    void PlayHitTween ()
    {
        //기존 트윈 정리
        _hitTween?.Kill( );

        //크기 초기화
        transform.localScale = _originScale;

        //박스 피격 흔들림 연출
        _hitTween = transform.DOShakeScale( 0.12f, 0.12f, 12, 90f )
            .SetEase( Ease.OutCubic );
    }

    /// <summary>
    /// 파괴될 때 트윈 정리
    /// </summary>
    private void OnDestroy ()
    {
        //체력바 트윈 정리
        _hpBarTween?.Kill( );

        //피격 트윈 정리
        _hitTween?.Kill( );

        //대미지 뷰 이벤트 구독 해제
        if ( _damageViewSpawner != null )
        {
            OnHit -= _damageViewSpawner.SpawnDamageView;
        }
    }

    #endregion

    #region ----- 스폰 -----
    /// <summary>
    /// 아이템들을 스폰한다
    /// </summary>
    public void SpawnItems ()
    {
        //튜토리얼 박스면 고정 아이템 스폰
        if ( _isTutorialBox == true )
        {
            SpawnTutorialItems( );
            return;
        }

        //스폰할 아이템이 없으면 종료
        if ( _items.Length <= 0 ) return;

        //최대 개수까지 포함해서 랜덤 개수 설정
        int randomNum = Random.Range( _minCount, _maxCount + 1 );

        for ( int i = 0; i < randomNum; i++ )
        {
            int randomIndex = GetRandomIndex( );

            //아이템들이 한 위치에 겹치지 않도록 위치 분산
            Vector2 dropPos = GetDropPosition( i, randomNum );

            Item item = Instantiate( _items [ randomIndex ], dropPos, transform.rotation );

            item.Init( );
        }
    }

    /// <summary>
    /// 튜토리얼 고정 아이템을 스폰한다
    /// </summary>
    public void SpawnTutorialItems ()
    {
        //스폰할 아이템이 없으면 종료
        if ( _items.Length <= 0 ) return;

        for ( int i = 0; i < _items.Length; i++ )
        {
            //기존 아이템 배열의 모든 아이템을 겹치지 않도록 위치 분산
            Vector2 dropPos = GetDropPosition( i, _items.Length );

            //튜토리얼 아이템 생성
            Item item = Instantiate( _items [ i ], dropPos, transform.rotation );

            //아이템 초기화
            item.Init( );
        }
    }

    /// <summary>
    /// 보너스 아이템을 스폰한다
    /// </summary>
    public void SpawnBonusItems ()
    {
        if ( _bonusItems.Length <= 0 ) return;

        //당첨 x 시 종료
        float randomNum = Random.value;
        if ( randomNum > _bonusItemRate ) return;

        //랜덤 인덱스
        int randomIndex = Random.Range( 0, _bonusItems.Length );

        Vector2 dropPos = GetDropPosition( 0, 1 );

        Instantiate( _bonusItems [ randomIndex ], dropPos, transform.rotation );
    }

    /// <summary>
    /// 아이템 드롭 위치 계산
    /// </summary>
    /// <param name="index">현재 아이템 순서</param>
    /// <param name="totalCount">전체 아이템 개수</param>
    /// <returns>드롭 위치</returns>
    public Vector2 GetDropPosition ( int index, int totalCount )
    {
        //아이템이 1개면 박스 중심 위에 생성
        if ( totalCount <= 1 )
        {
            return ( Vector2 ) transform.position + Vector2.up * _dropUpOffset;
        }

        //좌우로 일정 간격을 두고 생성
        float startX = -_dropRadius;
        float endX = _dropRadius;

        float ratio = index / ( float ) ( totalCount - 1 );

        float xOffset = Mathf.Lerp( startX, endX, ratio );

        return ( Vector2 ) transform.position + new Vector2( xOffset, _dropUpOffset );
    }

    /// <summary>
    /// 스폰 비율에 따른 아이템 인덱스 랜덤 반환
    /// </summary>
    /// <returns></returns>
    public int GetRandomIndex ()
    {
        //전체 비율 합
        float total = 0;

        foreach ( var item in _items )
        {
            total += item.SpawnRate;
        }

        //스폰 비율이 없으면 첫 번째 아이템 반환
        if ( total <= 0 )
        {
            return 0;
        }

        //랜덤 값 생성
        float randomPoint = Random.Range( 0, total );

        for ( int i = 0; i < _items.Length; i++ )
        {
            //랜덤 값 당첨
            if ( randomPoint < _items [ i ].SpawnRate )
            {
                return i;
            }
            //당첨 x 시 값 빼기
            else
            {
                randomPoint -= _items [ i ].SpawnRate;
            }
        }
        //마지막까지 당첨 x라면 마지막 인덱스 반환
        return _items.Length - 1;
    }

    #endregion

    #region ----- UI -----
    /// <summary>
    /// 체력 바 갱신
    /// </summary>
    public void UpdateHpBar ()
    {
        //체력바가 없으면 종료
        if ( _hpBarImage == null ) return;

        //기존 트윈 정리
        _hpBarTween?.Kill( );

        _hpBarTween = _hpBarImage.DOFillAmount( _currentHp / _maxHp, 0.2f )
            .SetEase( Ease.OutExpo );
    }

    #endregion
}
