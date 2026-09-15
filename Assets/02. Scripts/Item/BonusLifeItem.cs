using UnityEngine;

/// <summary>
/// 추가 목숨 획득 아이템
/// </summary>
public class BonusLifeItem : MonoBehaviour
{
    [Header ( "----- 컴포넌트 -----" )]
    [SerializeField] Collider2D _collider;


    private void OnTriggerEnter2D ( Collider2D collision )
    {
        if ( collision.CompareTag ( "Player" ) )
        {
            //컴포넌트 가져오기
            Hero hero = collision.GetComponent<Hero> ( );

            if ( hero != null )
            {
                hero.AddLives ( );

                //효과음 재생
                GameManager.Instance.SoundManager.PlaySfx( SfxType.Item );

                Destroy(gameObject);
            }
        }
    }
}
