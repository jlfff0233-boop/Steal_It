using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 씬 UI
/// </summary>
public class TitleSceneView : MonoBehaviour
{
    [Header( "----- 타이틀 -----" )]
    [SerializeField] TMP_Text _titleText;       //타이틀 텍스트

    [Header( "----- 불러오기 패널 -----" )]
    [SerializeField] TMP_Text _bestScoreText;           //최고 점수 텍스트
    [SerializeField] TMP_Text _submittedItemCountText;  //누적 제출 아이템 수 텍스트

    [Header( "----- 세팅 패널 -----" )]
    [SerializeField] Slider _bgmVolumeSlider;           //배경음악 볼륨 슬라이더
    [SerializeField] Slider _sfxVolumeSlider;           //효과음 볼륨 슬라이더
    [SerializeField] Toggle _bgmMuteToggle;             //배경음악 음소거 토글
    [SerializeField] Toggle _sfxMuteToggle;             //효과음 음소거 토글

    public float BgmVolume => _bgmVolumeSlider.value;
    public float SfxVolume => _sfxVolumeSlider.value;
    public bool IsBgmMuted => _bgmMuteToggle.isOn;
    public bool IsSfxMuted => _sfxMuteToggle.isOn;

    Tween _titleTween;      //타이틀 텍스트 트윈
    Vector3 _titleOriginScale;      //타이틀 원본 크기

    private void Awake ()
    {
        //타이틀 원본 크기 저장
        _titleOriginScale = _titleText.rectTransform.localScale;

        //타이틀 트윈 시작
        StartTitleTween( );
    }

    /// <summary>
    /// 타이틀 트윈 시작
    /// </summary>
    void StartTitleTween ()
    {
        //기존 트윈 정리
        _titleTween?.Kill( );

        //원본 크기로 초기화
        _titleText.rectTransform.localScale = _titleOriginScale;

        //타이틀 크기 반복 연출
        _titleTween = _titleText.rectTransform
            .DOScale ( _titleOriginScale * 1.5f , 1.0f )
            .SetEase ( Ease.InOutSine )
            .SetLoops ( -1 , LoopType.Yoyo );
    }

    /// <summary>
    /// 불러오기 패널 값 갱신
    /// </summary>
    /// <param name="bestScore">최고 점수</param>
    /// <param name="submittedItemCount">누적 제출 아이템 수</param>
    public void UpdateLoadPanel ( float bestScore, int submittedItemCount )
    {
        //저장 데이터 표시
        _bestScoreText.text = $"최고 점수: {bestScore}점";
        _submittedItemCountText.text = $"누적 제출 아이템 수: {submittedItemCount}개";
    }

    /// <summary>
    /// 세팅 패널 값 갱신
    /// </summary>
    /// <param name="bgmVolume">배경음악 볼륨</param>
    /// <param name="sfxVolume">효과음 볼륨</param>
    /// <param name="isBgmMuted">배경음악 음소거 여부</param>
    /// <param name="isSfxMuted">효과음 음소거 여부</param>
    public void UpdateSettingPanel ( float bgmVolume, float sfxVolume, bool isBgmMuted, bool isSfxMuted )
    {
        //전달받은 사운드 값을 UI에 반영
        _bgmVolumeSlider.value = bgmVolume;
        _sfxVolumeSlider.value = sfxVolume;

        _bgmMuteToggle.isOn = isBgmMuted;
        _sfxMuteToggle.isOn = isSfxMuted;
    }

    private void OnDestroy ()
    {
        //타이틀 트윈 정리
        _titleTween?.Kill( );

        //타이틀 대상 트윈 정리
        _titleText.rectTransform.DOKill( );
    }
}
