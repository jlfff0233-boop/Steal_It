using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 대미지 뷰 - 대미지 텍스트 설정, 파괴
/// </summary>
public class DamageView : MonoBehaviour
{
    [Header( "----- 파괴 -----" )]
    [SerializeField] float _duration;       //지속 시간
    [SerializeField] float _timer;      //타이머

    [Header( "----- 대미지 텍스트 -----" )]
    [SerializeField] TMP_Text _damageText;      //대미지 텍스트


    /// <summary>
    /// 대미지 텍스트 설정
    /// </summary>
    /// <param name="damage"></param>
    public void SetDamageText ( float damage )
    {
        _damageText.text = $"{damage}";
        _damageText.color = Color.red;

        //_duration에 걸쳐 투명하게 만들기
        _damageText.DOColor( Color.clear, _duration );

        //위로 올라가다가 일정 시간 후 삭제
        _damageText.rectTransform
            .DOAnchorPosY( _damageText.rectTransform.position.y + 100, _duration )
            .SetEase( Ease.OutCubic )
            .SetLink( gameObject )
            .OnComplete( () => Destroy( gameObject ) );
    }

    /// <summary>
    /// 대미지 텍스트 위치 보정/설정
    /// </summary>
    /// <param name="damage">대미지</param>
    /// <param name="offset">위치 보정 값</param>
    public void SetDamageText ( float damage, Vector2 offset )
    {
        //위치 보정
        _damageText.rectTransform.anchoredPosition = offset;

        //설정
        SetDamageText( damage );
    }
}
