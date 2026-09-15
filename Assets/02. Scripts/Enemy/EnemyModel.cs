using System;
using UnityEngine;

/// <summary>
/// 적 캐릭터 런타임 데이터 모델
/// </summary>
public class EnemyModel : MonoBehaviour
{
    [Header( "----- 런타임 데이터 -----" )]
    [Header( "--- 추격 ---" )]
    [SerializeField] float _targetCheckRadius;      //목표 체크 반지름
    [SerializeField] LayerMask _targetLayerMask;        //목표 레이어마스크
    [SerializeField] bool _isTargetIn;        //목표가 영역에 있는지 여부

    [SerializeField] float _itemBoxCheckRadius;     //아이템 박스 체크 반지름

    [SerializeField] float _originalMoveSpeed;
    [SerializeField] float _chaseSpeed;
    [SerializeField] float _chaseDuration;      //추격 지속 시간

    [Header( "----- 피격 -----" )]
    [SerializeField] float _stunSpan;       //스턴 간격
    [SerializeField] bool _isStunned;       //스턴 상태 여부

    #region ----- 프로퍼티 -----
    public float TargetCheckRadius => _targetCheckRadius;
    public LayerMask TargetCheckLayerMask => _targetLayerMask;
    public bool IsTargetIn
    {
        get => _isTargetIn;
        set => _isTargetIn = value;
    }

    public float ItemBoxCheckRadius => _itemBoxCheckRadius;

    public float ChaseSpeed => _chaseSpeed;
    public float ChaseDuration => _chaseDuration;
    public float OriginalMoveSpeed => _originalMoveSpeed;

    public float StunSpan
    {
        get => _stunSpan;
        set => _stunSpan = value;
    }
    public bool IsStunned
    {
        get => _isStunned;
        set => _isStunned = value;
    }
    #endregion

    public void Init ( Hero hero, float moveSpeed )
    {
        //원래 속도 저장
        _originalMoveSpeed = moveSpeed;
    }

}
