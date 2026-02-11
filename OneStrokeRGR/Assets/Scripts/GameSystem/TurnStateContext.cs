using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public enum TurnState
{
    PlayerInput,
    Resolving,
    Reward,
    SetUp,
    NewStage,
}

public class TurnStateContext : MonoBehaviour
{
    private ITurnState _currentState;
    // public TileCreateManager tileCreateManager; // 旧実装 - 削除済み
    public PathManager pathManager;
    public UIManager uiManager; // ボタンなどのUIを管理する想定

    public void Start()
    {
        // 最初の状態を開始。Forget()で投げっぱなしにする
        ChangeState(TurnState.NewStage).Forget();
    }

    /// <summary>
    /// 引数のStateに変更し、そのStateが「完了」するまで待機する
    /// </summary>
    public async UniTaskVoid ChangeState(TurnState next)
    {
        var token = this.GetCancellationTokenOnDestroy();

        // 1. 次のStateオブジェクトを生成
        _currentState = CreateState(next);

        if (_currentState != null)
        {
            Debug.Log($"State Changed to: {next}");
            
            // 2. Stateのメイン処理を実行し、その終了を非同期で待つ
            // これにより、Stateの中で「ボタンが押されるまで待機」などの処理が可能になる
            await _currentState.ExecuteAsync(token);
        }
    }

    private ITurnState CreateState(TurnState next)
    {
        return next switch
        {
            TurnState.NewStage => new NewStageState(this),
            TurnState.PlayerInput => new PlayerInputState(this),
            // TurnState.Resolving => new ResolvingState(this), // 今後作成
            _ => null
        };
    }
}