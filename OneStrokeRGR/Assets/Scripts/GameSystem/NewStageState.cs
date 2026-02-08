using Cysharp.Threading.Tasks;
using System.Threading;

public class NewStageState : ITurnState
{
    private readonly TurnStateContext _ctx;

    public NewStageState(TurnStateContext ctx)
    {
        _ctx = ctx;
    }

    public async UniTask ExecuteAsync(CancellationToken token)
    {
        // 1. ステージ生成
        _ctx.tileCreateManager.CreateStage();

        // 2. 少し余韻（ウェイト）を入れる
        await UniTask.Delay(500, cancellationToken: token);

        // 3. 次の状態へ
        _ctx.ChangeState(TurnState.PlayerInput).Forget();
    }
}