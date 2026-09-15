using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 히어로 모델 - 런타임 데이터(목숨, 공격, 무게, 인벤토리)
/// </summary>
public class HeroModel : MonoBehaviour
{
    [Header( "----- 런타임 데이터 -----" )]
    [Header( "--- 공격 ---" )]
    [SerializeField] float _damage;
    [SerializeField] float _attackRange;        //공격 샂어거리
    [SerializeField] float _attackCooltime;     //공격 쿨타임
    [SerializeField] float _attackTimer;        //공격 타이머
    [SerializeField] Transform _center;         //중심 트랜스폼
    [SerializeField] LayerMask _targetLayerMask;        //공격 대상 레이어마스크

    [Header( "--- 목숨 ---" )]
    [SerializeField] int _currentLives;     //현재 목숨
    [SerializeField] int _maxLives;     //최대 목숨

    [Header( "--- 아이템 ---" )]
    [SerializeField] float _currentWeight;      //현재 무게 
    [SerializeField] float _maxWeight;      //최대 무게

    [SerializeField] float _putDownDist;        //아이템 내려놓을 거리
    [SerializeField] float _putDownCheckRadius;     //내려놓기 위치 체크 반지름
    [SerializeField] LayerMask _putDownBlockLayerMask;      //내려놓기 방해 레이어

    [SerializeField] float _throwCooltime;      //던지기 쿨타임
    [SerializeField] float _throwTimer;     //던지기 타이머


    /// <summary>
    /// 인벤토리 딕셔너리(아이템 데이터, 아이템 개수)
    /// </summary>
    Dictionary<ItemData, int> _inventory = new Dictionary<ItemData, int>( );        //인벤토리 딕셔너리

    #region ----- 프로퍼티 -----

    public int CurrentLives => _currentLives;

    /// <summary>
    /// 현재 무게 조회 및 수정
    /// </summary>
    public float CurrentWeight
    {
        get => _currentWeight;
        set => _currentWeight = value;
    }

    public float MaxWeight => _maxWeight;

    public float PutDownDist => _putDownDist;
    public float PutDownCheckRadius => _putDownCheckRadius;
    public LayerMask PutDownBlockLayerMask => _putDownBlockLayerMask;

    public float ThrowCooltime => _throwCooltime;
    public float ThrowTimer
    {
        get => _throwTimer;
        set => _throwTimer = value;
    }

    /// <summary>
    /// 인벤토리 - 보유 아이템 조회(아이템, 개수)
    /// </summary>
    public Dictionary<ItemData, int> Inventory => _inventory;

    #region --- 공격 ---
    /// <summary>
    /// 공격력
    /// </summary>
    public float Damage => _damage;
    /// <summary>
    /// 공격 쿨타임
    /// </summary>
    public float AttackCooltime => _attackCooltime;
    /// <summary>
    /// 중심(공격 범위용)
    /// </summary>
    public Transform Center => _center;
    /// <summary>
    /// 공격 범위
    /// </summary>
    public float AttackRange => _attackRange;
    /// <summary>
    /// 공격 대상 레이어마스크
    /// </summary>
    public LayerMask TargetLayerMask => _targetLayerMask;

    /// <summary>
    /// 공격 타이머 조회 및 수정
    /// </summary>
    public float AttackTimer
    {
        get => _attackTimer;
        set => _attackTimer = value;
    }
    #endregion
    #endregion


    /// <summary>
    /// 인벤토리에 아이템 추가
    /// </summary>
    /// <param name="data">추가할 아이템 데이터</param>
    /// <returns>추가 성공 여부</returns>
    public bool AddItem ( ItemData data )
    {
        //최대 무게를 넘으면 종료
        if ( _currentWeight + data.Weight > _maxWeight ) return false;

        //처음 얻은 아이템이면 개수 초기화
        if ( _inventory.ContainsKey( data ) == false )
        {
            _inventory [ data ] = 1;
        }
        //아니면 개수 증가
        else
        {
            _inventory [ data ]++;
        }

        //현재 무게 갱신
        _currentWeight += data.Weight;

        //효과음 재생
        GameManager.Instance.SoundManager.PlaySfx( SfxType.Item );

        return true;
    }


    /// <summary>
    /// 인벤토리, 무게 초기화
    /// </summary>
    public void ClearInventory ()
    {
        //인벤토리 초기화
        _inventory.Clear( );

        //무게 초기화
        _currentWeight = 0;
    }


    /// <summary>
    /// 목숨을 1 잃는다
    /// </summary>
    public void LoseLife ()
    {
        _currentLives--;
    }

    /// <summary>
    /// 목숨을 1 회복한다
    /// </summary>
    public void AddLife ()
    {
        //최대 목숨이면 종료
        if ( _currentLives == _maxLives ) return;

        _currentLives++;
    }
}
