using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Definitions;
using FireworksMania.Core.Persistence;
using FireworksMania.Core.Messaging;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CustomTubes;

#if UNITY_EDITOR
using UnityEditor;
#endif


    public class TubeCopy : MonoBehaviour
    {
        private bool limit = false;
        public int MaxShells = 0;
        public float heightUp = 1;
        [SerializeField]
        private GameSoundDefinition ErrorSound;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (ErrorSound == null)
            {
                GameSoundDefinition temp = (GameSoundDefinition)AssetDatabase.LoadAssetAtPath("Assets/Reloadable Shells Asset/TubeStuff/TubeOther/zapsplat_multimedia_game_sound_error_incorrect_001_30721.mp3", typeof(GameSoundDefinition));
                ErrorSound = temp;
            }
        }
#endif
        public void CreateCopy()
        {
            if (limit) return;
            limit = true;
            StartCoroutine(SpawnCopies());
        }

        private IEnumerator SpawnCopies()
        {
            SaveableEntity tube = this.gameObject.GetComponent<SaveableEntity>();
            GameObject newtube = Instantiate(tube.EntityDefinition.PrefabGameObject, gameObject.transform.parent.transform);
            newtube.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + heightUp, gameObject.transform.position.z);
            newtube.transform.rotation = this.gameObject.transform.rotation;

            Rigidbody newrig = newtube.GetComponent<Rigidbody>();
            newrig.isKinematic = true;

            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(0.1f);

            BaseFireworkBehavior[] shells = this.gameObject.GetComponentsInChildren<BaseFireworkBehavior>();
            //Debug.Log("number of shells = " + shells.Length);
            if (shells.Length > MaxShells)
            {
                Destroy(newtube);
                Debug.LogError("Failed to copy tube, Too many Shells detected!\n Please wait until all active shell effects have compleately finished before copying.");
                if (ErrorSound != null)
                {
                    Messenger.Broadcast(new MessengerEventPlaySound(ErrorSound.name, this.gameObject.transform, true, true));
                }
                yield return new WaitForSeconds(2.5f);
                limit = false;
                yield break;
            }

            foreach (BaseFireworkBehavior B in shells)
            {
                // Debug.Log("Loading Copied Shell: " + B.gameObject.name);
                //if (B.gameObject.name == this.gameObject.name)
                //{
                //    continue;
                //}
                Vector3 pos = B.gameObject.transform.localPosition;
                BaseEntityDefinition defenition = B.EntityDefinition;

                CustomTubes.ShellTubeMatchData data;
                if (B.GameObject.TryGetComponent<CustomTubes.ShellTubeMatchData>(out data))
                {
                    data.shellData.Shell = defenition.PrefabGameObject;
                    yield return new WaitForSeconds(Time.deltaTime);
                    yield return new WaitForFixedUpdate();
                    //Debug.Log("Sending shell copy message");
                    newtube.BroadcastMessage("addCopiedShell", data, SendMessageOptions.RequireReceiver);
                }
                else
                {
                    Destroy(newtube);
                    Debug.LogError("Failed to copy tube, could not find Matching Data!");
                    if (ErrorSound != null)
                    {
                        Messenger.Broadcast(new MessengerEventPlaySound(ErrorSound.name, this.gameObject.transform, true, true));
                    }
                    break;
                }
            }
            yield return new WaitForSeconds(2.5f);
            limit = false;
        }
    }