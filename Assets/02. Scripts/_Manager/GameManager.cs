using UnityEngine;

/// <summary>
/// 게임매니저
/// </summary>
public class GameManager : MonoBehaviour
{
    //싱글톤 객체 생성
    static GameManager _instance;

    [SerializeField] SaveManager _saveManager;
    [SerializeField] SoundManager _soundManager;

    //프로퍼티
    public static GameManager Instance => _instance;
    public SaveManager SaveManager => _saveManager;
    public SoundManager SoundManager => _soundManager;

    private void Awake ()
    {
        //싱글톤 객체가 없었으면
        if ( _instance == null )
        {
            //자신을 싱글톤 객체로 지정
            _instance = this;

            //자신 게임오브젝트가 씬 전환 때 파괴되지 않도록 설정
            DontDestroyOnLoad( gameObject );

            //매니저 초기화
            _saveManager.Init( );
        }
        //싱글톤 객체가 있었으면
        else
        {
            Destroy( gameObject );
        }
    }
}

