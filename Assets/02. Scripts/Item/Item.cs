using UnityEngine;

/// <summary>
/// 아이템 - 데이터, 들기, 놓기, 던지기, 제출
/// </summary>
public class Item : MonoBehaviour
{
    [Header( "----- 설정 데이터 -----" )]
    [SerializeField] protected ItemData _data;

    [Header( "----- 런타임 데이터 -----" )]
    [SerializeField] bool _isSubmitted;        //제출 여부

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] protected Rigidbody2D _rigid;        //리지드바디
    [SerializeField] protected Collider2D _collider;      //콜라이더
    [SerializeField] SpriteRenderer _renderer;      //렌더러

    #region ----- 프로퍼티 -----
    public ItemData ItemData => _data;
    public float ItemScore => _data.Score;
    public float ItemWeight => _data.Weight;
    public float SpawnRate => _data.SpawnRate;

    public CarryType CarryType => _data.CarryType;
    public ShapeType ShapeType => _data.ShapeType;
    public Collider2D Collider => _collider;
    #endregion


    public void Init ()
    {
        SetIcon( );
    }

    /// <summary>
    /// 아이템 제출(중복 제출 방지)
    /// </summary>
    /// <returns>성공 여부</returns>
    public virtual bool Submit ()
    {
        //이미 제출된 상태면 종료
        if ( _isSubmitted == true ) return false;

        //제출 상태 처리
        _isSubmitted = true;

        //부모 해제
        transform.SetParent( null );

        //제출된 아이템 제거
        RemoveItem( );

        return true;
    }

    /// <summary>
    /// 아이콘 설정
    /// </summary>
    public void SetIcon ()
    {
        _renderer.sprite = _data.IconSprite;
    }

    /// <summary>
    /// 아이템 제거
    /// </summary>
    public virtual void RemoveItem ()
    {
        Destroy( gameObject );
    }
}
