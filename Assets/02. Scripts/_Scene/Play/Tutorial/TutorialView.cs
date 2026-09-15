using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 튜토리얼뷰 - UI 연출
/// </summary>
public class TutorialView : MonoBehaviour
{
    [Header( "----- 튜토리얼 UI -----" )]
    [SerializeField] GameObject _tutorialPanel;        //튜토리얼 패널
    [SerializeField] TMP_Text _guideText;      //안내 텍스트

    [Header( "----- 키 이미지 -----" )]
    [SerializeField] Image [ ] _keyImages;      //키 이미지 슬롯
    [SerializeField] Sprite [ ] _normalKeySprites;      //기본 키 스프라이트
    [SerializeField] Sprite [ ] _pressedKeySprites;     //눌림 키 스프라이트
    [SerializeField] float _keyFrameDelay;      //키 프레임 전환 간격
    [SerializeField] Vector2 _normalKeySize;        //일반 키 이미지 크기
    [SerializeField] Vector2 _spaceKeySize;     //스페이스 키 이미지 크기

    Coroutine _keyRoutine;      //키 애니메이션 코루틴

    /// <summary>
    /// 키 이미지 초기화
    /// </summary>
    void HideAllKeys ()
    {
        for ( int i = 0; i < _keyImages.Length; i++ )
        {
            _keyImages [ i ].gameObject.SetActive( false );
        }
    }

    /// <summary>
    /// 키 이미지 표시
    /// </summary>
    /// <param name="slotIndex">표시 슬롯 인덱스</param>
    /// <param name="keyIndex">키 인덱스</param>
    void ShowKey ( int slotIndex, int keyIndex )
    {
        //범위 보정
        if ( slotIndex < 0 || slotIndex >= _keyImages.Length ) return;
        if ( keyIndex < 0 || keyIndex >= _normalKeySprites.Length ) return;

        //키 이미지 크기 설정
        SetKeySize( slotIndex, keyIndex );

        //기본 스프라이트 설정
        _keyImages [ slotIndex ].sprite = _normalKeySprites [ keyIndex ];

        //비율 유지
        _keyImages [ slotIndex ].preserveAspect = true;

        //키 이미지 활성화
        _keyImages [ slotIndex ].gameObject.SetActive( true );
    }

    /// <summary>
    /// 키 이미지 크기 설정
    /// </summary>
    /// <param name="slotIndex">표시 슬롯 인덱스</param>
    /// <param name="keyIndex">키 인덱스</param>
    void SetKeySize ( int slotIndex, int keyIndex )
    {
        //스페이스 키와 시프트 키는 가로로 긴 크기 적용
        if ( keyIndex == 0 || keyIndex == 5 )
        {
            _keyImages [ slotIndex ].rectTransform.sizeDelta = _spaceKeySize;
            return;
        }

        //일반 키 크기 적용
        _keyImages [ slotIndex ].rectTransform.sizeDelta = _normalKeySize;
    }

    /// <summary>
    /// 키 애니메이션 재생
    /// </summary>
    /// <param name="keyIndexes">키 인덱스 배열</param>
    void StartKeyAnimation ( int [ ] keyIndexes )
    {
        //이전 코루틴 정리
        StopKeyAnimation( );

        //표시할 키가 없으면 종료
        if ( keyIndexes.Length == 0 ) return;

        //키 애니메이션 시작
        _keyRoutine = StartCoroutine( KeyAnimationRoutine( keyIndexes ) );
    }

    /// <summary>
    /// 키 애니메이션 정지
    /// </summary>
    void StopKeyAnimation ()
    {
        //코루틴이 없으면 종료
        if ( _keyRoutine == null ) return;

        //코루틴 정지
        StopCoroutine( _keyRoutine );

        //코루틴 초기화
        _keyRoutine = null;
    }

    /// <summary>
    /// 키 애니메이션 반복
    /// </summary>
    /// <param name="keyIndexes">키 인덱스 배열</param>
    IEnumerator KeyAnimationRoutine ( int [ ] keyIndexes )
    {
        while ( true )
        {
            for ( int i = 0; i < keyIndexes.Length; i++ )
            {
                //표시 슬롯이 부족하면 종료
                if ( i >= _keyImages.Length ) break;

                //키 인덱스 가져오기
                int keyIndex = keyIndexes [ i ];

                //스프라이트 범위가 맞으면 기본 스프라이트 적용
                if ( keyIndex >= 0 && keyIndex < _normalKeySprites.Length )
                {
                    _keyImages [ i ].sprite = _normalKeySprites [ keyIndex ];
                }
            }

            yield return new WaitForSeconds( _keyFrameDelay );

            for ( int i = 0; i < keyIndexes.Length; i++ )
            {
                //표시 슬롯이 부족하면 종료
                if ( i >= _keyImages.Length ) break;

                //키 인덱스 가져오기
                int keyIndex = keyIndexes [ i ];

                //스프라이트 범위가 맞으면 눌림 스프라이트 적용
                if ( keyIndex >= 0 && keyIndex < _pressedKeySprites.Length )
                {
                    _keyImages [ i ].sprite = _pressedKeySprites [ keyIndex ];
                }
            }

            yield return new WaitForSeconds( _keyFrameDelay );
        }
    }

    /// <summary>
    /// 안내 표시
    /// </summary>
    /// <param name="text">안내 문구</param>
    /// <param name="keyIndexes">키 인덱스 배열</param>
    public void ShowGuide ( string text, int [ ] keyIndexes )
    {
        //튜토리얼 뷰 활성화
        gameObject.SetActive( true );

        //키 애니메이션 정지
        StopKeyAnimation( );

        //키 이미지 초기화
        HideAllKeys( );

        //안내 문구 갱신
        _guideText.text = text;

        //패널 활성화
        _tutorialPanel.SetActive( true );

        for ( int i = 0; i < keyIndexes.Length; i++ )
        {
            //키 이미지 표시
            ShowKey( i, keyIndexes [ i ] );
        }

        //키 애니메이션 재생
        StartKeyAnimation( keyIndexes );
    }

    /// <summary>
    /// 점프 안내
    /// </summary>
    public void ShowJumpGuide ()
    {
        ShowGuide( "Space 키로 점프합니다.", new int [ ] { 0 } );
    }

    /// <summary>
    /// 빠른 하강 안내
    /// </summary>
    public void ShowFastFallGuide ()
    {
        ShowGuide( "S 키로 빠르게 하강합니다.", new int [ ] { 1 } );
    }

    /// <summary>
    /// 대시 안내
    /// </summary>
    public void ShowDashGuide ()
    {
        ShowGuide( "Shift 키로 대시합니다.", new int [ ] { 5 } );
    }

    /// <summary>
    /// 공격 안내
    /// </summary>
    public void ShowAttackGuide ()
    {
        ShowGuide( "좌클릭으로 상자를 공격합니다.", new int [ ] { } );
    }

    /// <summary>
    /// 아이템 획득 안내
    /// </summary>
    public void ShowPickUpGuide ()
    {
        ShowGuide( "E 키로 아이템을 획득합니다.", new int [ ] { 2 } );
    }

    /// <summary>
    /// 아이템 들기 안내
    /// </summary>
    public void ShowHoldGuide ()
    {
        ShowGuide( "E 키로 아이템을 듭니다.", new int [ ] { 2 } );
    }

    /// <summary>
    /// 아이템 내려놓기 안내
    /// </summary>
    public void ShowPutDownItem ()
    {
        ShowGuide( "R 키로 아이템을 내려놓습니다.", new int [ ] { 3 } );
    }

    /// <summary>
    /// 당기기 준비 안내
    /// </summary>
    public void ShowPullReadyGuide ()
    {
        ShowGuide( "잼과 마주보고 서세요.", new int [ ] { } );
    }

    /// <summary>
    /// 아이템 당기기 안내
    /// </summary>
    public void ShowPullGuide ()
    {
        ShowGuide( "Q 키를 누르면 당겨집니다.", new int [ ] { 6 } );
    }

    /// <summary>
    /// 던지기 준비 안내
    /// </summary>
    public void ShowThrowReadyGuide ()
    {
        ShowGuide( "토마토를 들어 주세요.", new int [ ] { 2 } );
    }

    /// <summary>
    /// 아이템 던지기 안내
    /// </summary>
    public void ShowThrowGuide ()
    {
        ShowGuide( "F 키로 조준 후 우클릭으로 던집니다.", new int [ ] { 4 } );
    }

    /// <summary>
    /// 적 스턴 안내
    /// </summary>
    public void ShowEnemyStunGuide ()
    {
        ShowGuide( "아이템에 맞은 적은 스턴 상태가 됩니다.", new int [ ] { } );
    }

    /// <summary>
    /// 아이템 파괴 안내
    /// </summary>
    public void ShowItemBreakGuide ()
    {
        ShowGuide( "3회 사용된 아이템은 파괴됩니다.", new int [ ] { } );
    }

    /// <summary>
    /// 아이템 제출 안내
    /// </summary>
    public void ShowSubmitGuide ()
    {
        ShowGuide( "목표 지점에서 E로 제출합니다.", new int [ ] { 2 } );
    }

    /// <summary>
    /// 목표 안내 
    /// </summary>
    public void ShowGoalGuide ()
    {
        ShowGuide( "제한 시간 안에 최대한 많은 점수를 모아 보세요!", new int [ ] { } );
    }

    /// <summary>
    /// 안내 숨김
    /// </summary>
    public void HideGuide ()
    {
        //키 애니메이션 정지
        StopKeyAnimation( );

        //키 이미지 초기화
        HideAllKeys( );

        //패널 비활성화
        _tutorialPanel.SetActive( false );
    }

    /// <summary>
    /// 튜토리얼 UI 전체 숨김
    /// </summary>
    public void HideTutorial ()
    {
        //안내 숨김
        HideGuide( );

        //튜토리얼 뷰 비활성화
        gameObject.SetActive( false );
    }

    private void OnDestroy ()
    {
        //키 애니메이션 정리
        StopKeyAnimation( );
    }
}
