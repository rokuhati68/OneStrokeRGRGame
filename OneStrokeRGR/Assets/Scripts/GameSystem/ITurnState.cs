using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public interface ITurnState
{
    
    // この状態が実行する非同期処理
    UniTask ExecuteAsync(CancellationToken token);
}