using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 클리어씬 관리 - 기록, 타이틀로 이동
/// </summary>
public class ClearScene : MonoBehaviour
{
    [Header( "----- 뷰 -----" )]
    [SerializeField] ClearSceneView _view;

    [Header( "----- 컴포넌트 -----" )]
    [SerializeField] Button _titleButton;       //타이틀 버튼
    [SerializeField] Button _deleteDataButton;      //데이터 삭제 버튼
    [SerializeField] GameObject _deleteDataPanel;       //데이터 삭제 패널
    [SerializeField] Button _confirmButton;     //확인 버튼
    [SerializeField] Button _cancelButton;      //취소 버튼


    private void Awake ()
    {
        _titleButton.onClick.AddListener( ClickTitleButton );
        _deleteDataButton.onClick.AddListener( ClickDeleteDataButton );
        _confirmButton.onClick.AddListener( ClickConfirmButton );
        _cancelButton.onClick.AddListener( ClickCancelButton );
    }

    private void Start ()
    {
        float score = GameManager.Instance.SaveManager.BestScore;
        int clearCount = GameManager.Instance.SaveManager.ClearCount;

        int collectedItemCount = GameManager.Instance.SaveManager.CollectedItemTotalCount;
        int submittedItemCount = GameManager.Instance.SaveManager.SubmittedItemTotalCount;

        float bonusTime = GameManager.Instance.SaveManager.BonusTimeTotalAmount;
        int bonusLifeCount = GameManager.Instance.SaveManager.BonusLifeTotalCount;
        int enemyStunCount = GameManager.Instance.SaveManager.EnemyStunTotalCount;

        _view.UpdateAllRecord( score, clearCount, collectedItemCount, submittedItemCount, bonusTime, bonusLifeCount, enemyStunCount );
    }


    public void ClickTitleButton ()
    {
        SceneManager.LoadScene( "Title" );
    }

    public void ClickDeleteDataButton ()
    {
        //데이터 삭제 패널 활성화
        _deleteDataPanel.SetActive( true );
    }

    public void ClickConfirmButton ()
    {
        //데이터 삭제
        GameManager.Instance.SaveManager.DeleteData( );

        //타이틀로 이동
        SceneManager.LoadScene( "Title" );
    }

    public void ClickCancelButton ()
    {
        //패널 비활성화
        _deleteDataPanel.SetActive( false );

    }
}
