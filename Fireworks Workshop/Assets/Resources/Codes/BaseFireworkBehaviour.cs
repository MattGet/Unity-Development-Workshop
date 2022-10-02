// Decompiled with JetBrains decompiler
// Type: FireworksMania.Core.Behaviors.Fireworks.BaseFireworkBehavior
// Assembly: FireworksMania.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FC022E74-5B2A-4AC1-AC52-607786D4C4BB
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Fireworks Mania\Fireworks Mania_Data\Managed\FireworksMania.Core.dll

using Cysharp.Threading.Tasks;
using DG.Tweening;
using FireworksMania.Core.Behaviors.Fireworks.Parts;
using FireworksMania.Core.Definitions.EntityDefinitions;
using FireworksMania.Core.Interactions;
using FireworksMania.Core.Persistence;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

namespace FireworksMania.Core.Behaviors.Fireworks
{
  public abstract class BaseFireworkBehavior : 
    MonoBehaviour,
    IHaveObjectInfo,
    ISaveableComponent,
    IHaveBaseEntityDefinition,
    IIgnitable,
    IHaveFuse,
    IHaveFuseConnectionPoint
  {
    [Header("General")]
    [FormerlySerializedAs("_metadata")]
    [SerializeField]
    private FireworkEntityDefinition _entityDefinition;
    [SerializeField]
    protected Fuse _fuse;
    protected CancellationToken _cancellationTokentoken;
    private GameObject _gameObject;
    private SaveableEntity _saveableEntity;
    public Action<BaseFireworkBehavior> OnDestroyed;

    protected virtual void Awake()
    {
      if ((UnityEngine.Object) this._entityDefinition == (UnityEngine.Object) null)
      {
        Debug.LogError((object) ("Missing FireworkEntityDefinition on '" + this.gameObject.name + "' - everything will go wrong this way!"), (UnityEngine.Object) this);
      }
      else
      {
        if (this.GetComponent<IErasable>() == null)
          this.gameObject.AddComponent<ErasableBehavior>();
        this._saveableEntity = this.GetComponent<SaveableEntity>();
        if ((UnityEngine.Object) this._saveableEntity == (UnityEngine.Object) null)
        {
          Debug.LogError((object) ("Missing 'SaveableEntity' which is a required component - make sure '" + this.name + "' have one"), (UnityEngine.Object) this);
        }
        else
        {
          this._gameObject = this.gameObject;
          this._fuse.SaveableEntityOwner = this._saveableEntity;
          this._cancellationTokentoken = this.GetCancellationTokenOnDestroy();
        }
      }
    }

    protected virtual void OnValidate()
    {
      if (Application.isPlaying)
        return;
      if ((UnityEngine.Object) this._entityDefinition == (UnityEngine.Object) null)
      {
        Debug.LogError((object) ("Missing 'FireworkEntityDefinition' on '" + this.gameObject.name + "'"), (UnityEngine.Object) this);
      }
      else
      {
        this.ValidateErasableBehavior();
        this.ValidateSaveableEntity();
      }
    }

    private void ValidateSaveableEntity()
    {
      SaveableEntity[] components = this.GetComponents<SaveableEntity>();
      if (components.Length > 1)
        Debug.LogError((object) string.Format("'{0}' have '{1}' '{2}'s' - it can have one and only one - please delete so only one is left else it will be saved multiple times in blueprints", (object) this.EntityDefinition?.Id, (object) components.Length, (object) "SaveableEntity"), (UnityEngine.Object) this);
      this._saveableEntity = this.GetComponent<SaveableEntity>();
      if ((UnityEngine.Object) this._saveableEntity == (UnityEngine.Object) null)
        this._saveableEntity = this.gameObject.AddComponent<SaveableEntity>();
      if (!((UnityEngine.Object) this._saveableEntity.EntityDefinition != (UnityEngine.Object) null) || !(this._saveableEntity.EntityDefinition.Id != this._entityDefinition.Id))
        return;
      Debug.LogError((object) ("'BaseEntityDefinition' was different on '" + ((object) this._saveableEntity).GetType().Name + "' on '" + this.gameObject.name + "', excepted '" + this._entityDefinition.Id + "' but was '" + this._saveableEntity.EntityDefinition.Id + "', please fix else save/load won't work"), (UnityEngine.Object) this);
    }

    private void ValidateErasableBehavior()
    {
      ErasableBehavior[] components = this.GetComponents<ErasableBehavior>();
      if (components.Length == 0)
      {
        this.gameObject.AddComponent<ErasableBehavior>();
        Debug.Log((object) "Added required 'ErasableBehavior' to this entity can be removed via the Eraser Tool in game", (UnityEngine.Object) this.gameObject);
      }
      if (components.Length <= 1)
        return;
      Debug.LogWarning((object) string.Format("'{0}' have '{1}' '{2}'s' it should have one and onlye one - removing all the extra ones", (object) this.EntityDefinition?.Id, (object) components.Length, (object) "ErasableBehavior"), (UnityEngine.Object) this);
    }

    protected virtual void Start() => this._fuse.OnFuseCompleted += new Action(this.OnFuseCompleted);

    protected virtual void OnDestroy()
    {
      if (!((UnityEngine.Object) this._fuse != (UnityEngine.Object) null))
        return;
      this._fuse.OnFuseCompleted -= new Action(this.OnFuseCompleted);
    }

    private async void OnFuseCompleted()
    {
      int num = await this.LaunchInternalAsync(this._cancellationTokentoken).SuppressCancellationThrow() ? 1 : 0;
    }

    protected virtual async UniTask DestroyFireworkAsync(CancellationToken token)
    {
      if (token.IsCancellationRequested)
        return;
      int num = await this.DestroyFireworkAnimatedAsync(token).SuppressCancellationThrow() ? 1 : 0;
    }

    protected abstract UniTask LaunchInternalAsync(CancellationToken token);

    private async UniTask DestroyFireworkAnimatedAsync(CancellationToken token)
    {
      BaseFireworkBehavior fireworkBehavior = this;
      UniTask uniTask = fireworkBehavior.transform.DOShakeScale(0.3f, 0.5f, 5, 50f).WithCancellation(token);
      await uniTask;
      token.ThrowIfCancellationRequested();
      uniTask = fireworkBehavior.transform.DOScale(0.0f, UnityEngine.Random.Range(0.1f, 0.2f)).WithCancellation(token);
      await uniTask;
      token.ThrowIfCancellationRequested();
      Action<BaseFireworkBehavior> onDestroyed = fireworkBehavior.OnDestroyed;
      if (onDestroyed != null)
        onDestroyed(fireworkBehavior);
      UnityEngine.Object.Destroy((UnityEngine.Object) fireworkBehavior.gameObject);
    }

    public virtual CustomEntityComponentData CaptureState()
    {
      CustomEntityComponentData entityComponentData = new CustomEntityComponentData();
      Rigidbody component = this.GetComponent<Rigidbody>();
      entityComponentData.Add<SerializableVector3>("Position", new SerializableVector3()
      {
        X = this.transform.position.x,
        Y = this.transform.position.y,
        Z = this.transform.position.z
      });
      entityComponentData.Add<SerializableRotation>("Rotation", new SerializableRotation()
      {
        X = this.transform.rotation.x,
        Y = this.transform.rotation.y,
        Z = this.transform.rotation.z,
        W = this.transform.rotation.w
      });
      entityComponentData.Add<bool>("IsKinematic", (UnityEngine.Object) component != (UnityEngine.Object) null && component.isKinematic);
      return entityComponentData;
    }

    public virtual void RestoreState(CustomEntityComponentData customComponentData)
    {
      SerializableVector3 serializableVector3 = customComponentData.Get<SerializableVector3>("Position");
      SerializableRotation serializableRotation = customComponentData.Get<SerializableRotation>("Rotation");
      bool flag = customComponentData.Get<bool>("IsKinematic");
      this.transform.position = new Vector3(serializableVector3.X, serializableVector3.Y, serializableVector3.Z);
      this.transform.rotation = new Quaternion(serializableRotation.X, serializableRotation.Y, serializableRotation.Z, serializableRotation.W);
      Rigidbody component = this.GetComponent<Rigidbody>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.isKinematic = flag;
    }

    public void Ignite(float ignitionForce) => this._fuse.Ignite(ignitionForce);

    public void IgniteInstant() => this._fuse.IgniteInstant();

    public Fuse GetFuse() => this._fuse;

    public string SaveableComponentTypeId => ((object) this).GetType().Name;

    public string Name => this._entityDefinition.ItemName;

    public GameObject GameObject => this._gameObject;

    public BaseEntityDefinition EntityDefinition => (BaseEntityDefinition) this._entityDefinition;

    public Transform IgnitePositionTransform => this._fuse.IgnitePositionTransform;

    public IFuseConnectionPoint ConnectionPoint => this._fuse.ConnectionPoint;

    public bool Enabled => this._fuse.Enabled;
  }
}
