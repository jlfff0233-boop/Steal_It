using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인벤토리뷰 - 인벤토리 UI 갱신
/// </summary>
public class InventoryView : MonoBehaviour
{
    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Image [ ] _inventoryImages;        //인벤토리 아이콘 이미지 배열
    [SerializeField] TMP_Text [ ] _inventoryTexts;      //인벤토리 개수 텍스트 배열

    Tween [ ] _inventoryTweens;     //인벤토리 슬롯 트윈 배열

    /// <summary>
    /// 인벤토리 슬롯 트윈 배열 초기화
    /// </summary>
    void InitInventoryTweens ()
    {
        //없으면 종료
        if ( _inventoryTweens != null ) return;

        //인벤토리 아이콘 개수 기준으로 배열 생성
        _inventoryTweens = new Tween [ _inventoryImages.Length ];
    }

    /// <summary>
    /// 인벤토리 슬롯 연출
    /// </summary>
    /// <param name="index">슬롯 인덱스</param>
    void PlayInventoryTween ( int index )
    {
        //인벤토리 슬롯 트윈 배열 초기화
        InitInventoryTweens( );

        //기존 트윈 정리
        if ( _inventoryTweens [ index ] != null ) _inventoryTweens [ index ].Kill( );

        //크기 초기화
        _inventoryImages [ index ].transform.localScale = Vector3.one;

        //아이콘 크기 연출
        _inventoryTweens [ index ] = _inventoryImages [ index ].transform.DOScale( 1.15f, 0.1f )
            .SetLoops( 2, LoopType.Yoyo )
            .SetEase( Ease.OutCubic );
    }

    /// <summary>
    /// 인벤토리 UI 갱신
    /// </summary>
    /// <param name="inventory">인벤토리 딕셔너리</param>
    public void UpdateInventory ( Dictionary<ItemData, int> inventory )
    {
        //초기화
        Clean( );

        //슬롯 인덱스
        int slotIndex = 0;

        foreach ( var item in inventory )
        {
            //범위 보정
            if ( slotIndex < 0 || slotIndex >= _inventoryImages.Length ) break;

            //아이콘 가져오기
            _inventoryImages [ slotIndex ].sprite = item.Key.IconSprite;

            //아이콘 활성화
            _inventoryImages [ slotIndex ].gameObject.SetActive( true );

            //개수 텍스트 갱신
            _inventoryTexts [ slotIndex ].text = $"x {item.Value}";

            //인벤토리 슬롯 연출
            PlayInventoryTween( slotIndex );

            //다음 슬롯으로 이동
            slotIndex++;
        }
    }

    /// <summary>
    /// 인벤토리 UI 초기화
    /// </summary>
    public void Clean ()
    {
        for ( int i = 0; i < _inventoryImages.Length; i++ )
        {
            //아이콘 비활성화
            _inventoryImages [ i ].gameObject.SetActive( false );

            //아이콘 초기화
            _inventoryImages [ i ].sprite = null;

            //개수 텍스트 초기화
            _inventoryTexts [ i ].text = "x 0";
        }
    }

    /// <summary>
    /// 트윈 정리
    /// </summary>
    private void OnDestroy ()
    {
        //인벤토리 트윈이 없으면 종료
        if ( _inventoryTweens == null ) return;

        for ( int i = 0; i < _inventoryTweens.Length; i++ )
        {
            //인벤토리 트윈 정리
            if ( _inventoryTweens [ i ] != null ) _inventoryTweens [ i ].Kill( );
        }
    }
}
