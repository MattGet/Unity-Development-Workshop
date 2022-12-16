using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Threading.Tasks;
using FireworksMania.Core.Behaviors.Fireworks;
using CustomTubes;

namespace FireworksMania.Core.Behaviors
{
    [AddComponentMenu("Fireworks Mania/Behaviors/Other/TubeErase")]
    [DisallowMultipleComponent]
    public class TubeErase : MonoBehaviour, IErasable
    {
        private CancellationToken _cancellationTokentoken;
        private bool _isErasing;
        private bool erasedShells;
        private void Awake() => this._cancellationTokentoken = this.GetCancellationTokenOnDestroy();

        public async void Erase()
        {
            if (this._isErasing)
                return;
            this._isErasing = true;
            await this.EraseAsync(this._cancellationTokentoken);
            this._isErasing = false;
        }

        private async UniTask EraseAsync(CancellationToken token)
        {
            erasedShells = false;
            TubeErase erasableBehavior = this;
            LoadableTubeBehaviour[] shells = erasableBehavior.gameObject.GetComponentsInChildren<LoadableTubeBehaviour>();
            if (shells.Length != 0 || shells != null)
            {
                //Debug.Log("Destroying Shells");
                foreach (LoadableTubeBehaviour B in shells)
                {
                    if (B.Shell != null)
                    {
                        //Debug.Log("Destroying " + B.Shell);
                        Object.Destroy((Object)B.Shell);
                        erasedShells = true;
                    }
                    else
                    {
                        //Debug.Log("Tube " + B.gameObject + " was empty");
                    }
                    TubeIgniteComponent ignite;
                    if (B.gameObject.TryGetComponent(out ignite))
                    {
                        Destroy(ignite);
                    }
                }
            }
            if (erasedShells == false)
            {
                //Debug.Log("Destroying prefab");
                //Tween shake = erasableBehavior.gameObject.transform.DOShakeScale(0.3f, 0.5f, 5, 50f);
                //await UniTask.WaitUntil(() => shake.IsActive() == false);
                //Debug.Log("Destroying prefab1");
                Tween scale = erasableBehavior.gameObject.transform.DOScale(0.0f, UnityEngine.Random.Range(0.1f, 0.2f));
                await UniTask.WaitUntil(() => scale.IsActive() == false);
                //Debug.Log("Destroying prefab2");
                Object.Destroy((Object)erasableBehavior.gameObject);
            }
            //await UniTask.WaitForEndOfFrame(this);
        }
    }
}