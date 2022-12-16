using UnityEngine;
using FireworksMania.Core.Definitions;

namespace CustomTubes
{
    
    public class TubeClass : MonoBehaviour
    {
        [ContextMenu("Generate new unique Class Name (based on GameObject name)")]
        private void ClassNameFromName() => this.ClassName = this.name;

        public string ClassName;

        public float radius;
        public float height;
        public float ExplosionForce;
        public GameSoundDefinition LoadSound;
        public GameSoundDefinition DropSound;
    }
}

