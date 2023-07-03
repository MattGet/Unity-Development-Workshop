using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireworksMania.Core.Behaviors.Fireworks;
using FireworksMania.Core.Behaviors.Fireworks.Parts;

namespace CustomTubes
{
    public class TubeIgniteComponent : MonoBehaviour, IHaveFuse, IHaveFuseConnectionPoint, IIgnitable
    {
        public Fuse _fuse;

        public TubeIgniteComponent(Fuse fuse)
        {
            this._fuse = fuse;
        }

        public Fuse GetFuse() => this._fuse;

        public void Ignite(float ignitionForce) => this._fuse.Ignite(ignitionForce);

        public void IgniteInstant() => this._fuse.IgniteInstant();

        public Transform IgnitePositionTransform => this._fuse.IgnitePositionTransform;

        public IFuseConnectionPoint ConnectionPoint => this._fuse.ConnectionPoint;

        public bool Enabled => this._fuse.Enabled;

        public bool IsIgnited => this._fuse.IsIgnited;
    }

}
