using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Messaging;

namespace FireworksMania.Core.Behaviors
{
    [AddComponentMenu("Fireworks Mania/Behaviors/Other/ButtonBehavior")]
    public class ButtonBehaviour : MonoBehaviour, IUseable
    {
        [Header("General")]
        [SerializeField]
        [Tooltip("If filled out, will be shown under the 'Use' tooltip UI in the game.")]
        private string _customText;
        [SerializeField]
        [Tooltip("Indicates if this object should be highlighted or not, when in view of the player.")]
        private bool _showHighlight = true;
        [SerializeField]
        [Tooltip("Indicates if the interaction UI should be shown when the player looks at it.")]
        private bool _showInteractionUI = true;

        [Header("Events")]
        public UnityEvent OnBeginUse;
        public UnityEvent OnEndUse;
        [Header("Button Settings")]
        [Space(10)]
        [Tooltip("The gameobject that will act as the button and move up/down when pressed")]
        public GameObject Button;
        [Tooltip("The distance the button will travel when pressed")]
        public float PressDistance;
        [Tooltip("The sound played when the button is pressed")]
        public GameSoundDefinition clickSound;
        [Tooltip("The sound played when the button is released")]
        public GameSoundDefinition releaseSound;
        private Vector3 presspos = new Vector3();
        private Vector3 origin = new Vector3();


        public void Awake()
        {
            origin = Button.transform.localPosition;
            var move = Button.transform.localPosition.z;
            move = move - PressDistance;
            presspos = new Vector3(Button.transform.localPosition.x, Button.transform.localPosition.y, move);
        }

        public void BeginUse()
        {
            this.IsInUse = true;
            if (this.OnBeginUse == null)
                return;
            this.OnBeginUse.Invoke();

            Button.transform.localPosition = presspos;
            PlaySound(true);
        }

        public void EndUse()
        {
            this.IsInUse = false;
            if (this.OnEndUse == null)
                return;
            this.OnEndUse.Invoke();

            Button.transform.localPosition = origin;
            PlaySound(false);
        }

        public void PlaySound(bool press)
        {
            if (press)
            {
                if (clickSound != null)
                {
                    Messenger.Broadcast(new MessengerEventPlaySound(clickSound.name, this.transform, true, true));
                }
            }
            else
            {
                if (releaseSound != null)
                {
                    Messenger.Broadcast(new MessengerEventPlaySound(releaseSound.name, this.transform, true, true));
                }
            }

        }

        public GameObject GameObject => this.gameObject;

        public bool IsInUse { get; private set; }

        public bool ShowHighlight => this._showHighlight;

        public bool ShowInteractionUI => this._showInteractionUI;

        public string CustomText => this._customText;
    }
}
