using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

// 注意: このクラスは旧実装です。後で新しいGamePresenterに置き換えます
public class NewStageState : ITurnState
{
    private readonly TurnStateContext _ctx;

    public NewStageState(TurnStateContext ctx)
    {
        _ctx = ctx;
    }

    public async UniTask ExecuteAsync(CancellationToken token)
    {
        // 旧実装 - TileCreateManagerは削除済み
        Debug.Log("NewStageState: ステージ生成（旧実装はコメントアウト）");
        // _ctx.tileCreateManager.CreateStage();

        // 2. 少し余韻（ウェイト）を入れる
        await UniTask.Delay(500, cancellationToken: token);

        // 3. 次の状態へ
        _ctx.ChangeState(TurnState.PlayerInput).Forget();
    }
}