using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
public class PlayerInputState : ITurnState
{
    private readonly TurnStateContext _ctx;

    public PlayerInputState(TurnStateContext ctx) => _ctx = ctx;

    public async UniTask ExecuteAsync(CancellationToken token)
    {
        bool isDecisionMade = false;

        while (!isDecisionMade)
        {
            // 1. 初期化して入力を有効にする
            _ctx.pathManager.ClearPath();
            _ctx.pathManager.IsActive = true;
            _ctx.uiManager.confirmPanel.SetActive(false); // ボタン群を隠す

            // 2. プレイヤーが指を離すのを待つ
            await UniTask.WaitUntil(() => _ctx.pathManager.IsPathConfirmed, cancellationToken: token);

            // 3. 確認用ボタンを表示
            _ctx.uiManager.confirmPanel.SetActive(true);

            // 4. 「決定」か「やり直し」か、どちらかのボタンが押されるのを待つ
            // UniTask.WhenAny を使うと、先に押された方のインデックスが返ってくる
            int buttonIndex = await UniTask.WhenAny(
                _ctx.uiManager.decisionButton.OnClickAsync(token), // 0番
                _ctx.uiManager.retryButton.OnClickAsync(token)     // 1番
            );

            if (buttonIndex == 0)
            {
                // 決定！
                isDecisionMade = true;
            }
            else
            {
                // やり直し！ ループの最初に戻る
                Debug.Log("Retry selected.");
            }
        }

        // 5. 次のStateへ遷移
        _ctx.uiManager.confirmPanel.SetActive(false);
        //_ctx.ChangeState(new ResolvePathState(_ctx)).Forget();
    }
}