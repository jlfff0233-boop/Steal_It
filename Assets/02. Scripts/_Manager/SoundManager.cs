using UnityEngine;

/// <summary>
/// 배경음악 종류
/// </summary>
public enum BgmType
{
    Title,
    Stage,
    Clear,
}

/// <summary>
/// 효과음 종류
/// </summary>
public enum SfxType
{
    Item,
    Hit,
    Jump,
    Dash,
    Open,
    Submit,
    Attack,
    UI,
    Panel,
}

/// <summary>
/// 사운드 매니저 - 배경음악/효과음 재생, 정지, 볼륨 조절, 음소거
/// </summary>
public class SoundManager : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] AudioSource _bgmSource;        //배경음악 오디오소스
    [SerializeField] AudioSource _sfxSource;        //효과음 오디오소스

    [Header( "----- 리소스 -----" )]
    [SerializeField] AudioClip [ ] _bgmClips;       //배경음악 클립 배열
    [SerializeField] AudioClip [ ] _sfxClips;       //효과음 클립 배열

    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] float _bgmVolume;      //배경음악 볼륨
    [SerializeField] float _sfxVolume;      //효과음 볼륨

    [SerializeField] bool _isBgmMuted;      //배경음악 음소거 여부
    [SerializeField] bool _isSfxMuted;      //효과음 음소거 여부

    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;

    public bool IsBgmMuted => _isBgmMuted;
    public bool IsSfxMuted => _isSfxMuted;

    /// <summary>
    /// 배경음악 재생
    /// </summary>
    /// <param name="bgmType">종류</param>

    public void PlayBgm ( BgmType bgmType )
    {
        int index = ( int ) bgmType;

        //인덱스 범위 보정
        if ( index < 0 || index >= _bgmClips.Length ) return;

        //오디오소스의 클립을 index번 클립으로 설정
        _bgmSource.clip = _bgmClips [ index ];

        //오디오 재생
        _bgmSource.Play( );
    }

    /// <summary>
    /// 배경음악 정지
    /// </summary>
    public void StopBgm ()
    {
        _bgmSource.Stop( );
    }

    /// <summary>
    /// 배경음악 일시정지
    /// </summary>
    public void PauseBgm ()
    {
        _bgmSource.Pause( );
    }

    /// <summary>
    /// 배경음악 일시정지 해제
    /// </summary>
    public void UnPauseBgm ()
    {
        _bgmSource.UnPause( );
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="sfxType"></param>
    public void PlaySfx ( SfxType sfxType )
    {
        int index = ( int ) sfxType;

        if ( index < 0 || index >= _sfxClips.Length ) return;

        //일회성 재생
        _sfxSource.PlayOneShot( _sfxClips [ index ] );
    }

    /// <summary>
    /// 배경음악 볼륨 설정
    /// </summary>
    /// <param name="volume"></param>
    public void SetBgmVolume ( float volume )
    {
        _bgmVolume = Mathf.Clamp( volume, 0.0f, 1.0f );

        _bgmSource.volume = _bgmVolume;
    }

    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    /// <param name="volume"></param>
    public void SetSfxVolume ( float volume )
    {
        _sfxVolume = Mathf.Clamp( volume, 0.0f, 1.0f );

        _sfxSource.volume = _sfxVolume;
    }

    /// <summary>
    /// 배경음악 뮤트 설정
    /// </summary>
    /// <param name="mute"></param>
    public void SetBgmMute ( bool mute )
    {
        _isBgmMuted = mute;

        _bgmSource.mute = mute;
    }

    /// <summary>
    /// 효과음 뮤트 설정
    /// </summary>
    /// <param name="mute"></param>
    public void SetSfxMute ( bool mute )
    {
        _isSfxMuted = mute;

        _sfxSource.mute = mute;
    }
}
