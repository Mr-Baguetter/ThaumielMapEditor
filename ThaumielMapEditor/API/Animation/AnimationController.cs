using System.Collections.Generic;
using System.IO;
using AdminToys;
using LabApi.Features.Wrappers;
using MEC;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Animation
{
    // I have no clue how animations work in unity so I hope this works :D
    public class AnimationController
    {
        /// <summary>
        /// A dictionary containing all active <see cref="AnimationController"/> instances, keyed by their associated <see cref="SchematicData"/>.
        /// </summary>
        public static readonly Dictionary<SchematicData, AnimationController> AnimationSchematics = [];

        private readonly record struct PrimitiveDummy(PrimitiveObject Primitive, Transform Transform, DummyAnimatable Animatable);
        private readonly GameObject _dummyRoot;
        private readonly Animator _animator;
        private readonly List<PrimitiveDummy> _dummies = [];
        private CoroutineHandle _handle;

        /// <summary>
        /// Gets the <see cref="SchematicData"/> this controller is associated with.
        /// </summary>
        public SchematicData Schematic { get; }

        /// <summary>
        /// Gets a value indicating whether an animation is currently playing.
        /// </summary>
        public bool IsPlaying { get; private set; }

        internal AnimationController(SchematicData schematic)
        {
            Schematic = schematic;
            _dummyRoot = new GameObject($"[AnimDummy] {schematic.FileName}");
            _animator = _dummyRoot.AddComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            if (TryLoadAnimatorController(schematic, out RuntimeAnimatorController controller))
            {
                _animator.runtimeAnimatorController = controller;
                LogManager.Debug($"loaded controller '{controller.name}' for '{schematic.FileName}'.");
            }
            else
                LogManager.Warn($"No animator controller found for '{schematic.FileName}'. Animation will not play.");

            Dictionary<uint, Transform> dummyByNetId = [];
            foreach (PrimitiveObject primitive in schematic.Primitives)
            {
                if (string.IsNullOrEmpty(primitive.Name))
                {
                    LogManager.Warn($"Skipping unnamed primitive in '{schematic.FileName}' it cannot be targeted by animation curves.");
                    continue;
                }

                Transform parentTransform = dummyByNetId.TryGetValue(primitive.ParentId, out Transform? parentDummy) ? parentDummy : _dummyRoot.transform;

                GameObject child = new(primitive.Name);
                child.transform.SetParent(parentTransform, false);
                child.transform.localPosition = primitive.Position;
                child.transform.localRotation = primitive.Rotation;
                child.transform.localScale = primitive.Scale;

                DummyAnimatable animatable = child.AddComponent<DummyAnimatable>();
                animatable.color = primitive.Color;
                animatable.primitiveType = (int)primitive.PrimitiveType;
                animatable.primitiveFlags = (int)primitive.PrimitiveFlags;

                _dummies.Add(new PrimitiveDummy(primitive, child.transform, animatable));
                dummyByNetId[primitive.NetId] = child.transform;
            }

            AnimationSchematics[schematic] = this;
            LogManager.Debug($"Built for '{schematic.FileName}' with {_dummies.Count} animatable primitives.");
        }

        private static bool TryLoadAnimatorController(SchematicData schematic, out RuntimeAnimatorController controller)
        {
            controller = null!;

            foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                Object[] assets = bundle.LoadAllAssets();
                foreach (Object asset in assets)
                {
                    if (asset is not RuntimeAnimatorController runtimeController)
                        continue;

                    controller = runtimeController;
                    return true;
                }
            }

            string path = Path.Combine(Path.GetDirectoryName(ThaumFileManager.Dir(["Schematics"]))!, $"{schematic.FileName}-Animator");
            if (!File.Exists(path))
                return false;

            AssetBundle loadedBundle = AssetBundle.LoadFromFile(path);
            if (loadedBundle == null)
            {
                LogManager.Warn($"Failed to load asset bundle at '{path}'.");
                return false;
            }

            Object[] bundleAssets = loadedBundle.LoadAllAssets();
            foreach (Object asset in bundleAssets)
            {
                if (asset is not RuntimeAnimatorController runtimeController)
                    continue;

                controller = runtimeController;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an existing <see cref="AnimationController"/> for the given <see cref="SchematicData"/>, or creates a new one if none exists.
        /// </summary>
        /// <param name="schematic">The schematic to get or create a controller for.</param>
        /// <returns>The existing or newly created <see cref="AnimationController"/>.</returns>
        public static AnimationController GetOrCreate(SchematicData schematic)
        {
            if (AnimationSchematics.TryGetValue(schematic, out AnimationController? existing))
                return existing;

            return new AnimationController(schematic);
        }

        /// <summary>
        /// Overrides the automatically loaded animator controller with the specified one.
        /// </summary>
        /// <param name="controller">The <see cref="RuntimeAnimatorController"/> to assign.</param>
        public void SetController(RuntimeAnimatorController controller)
        {
            _animator.runtimeAnimatorController = controller;
            LogManager.Debug($"Controller '{controller.name}' assigned to '{Schematic.FileName}'.");
        }

        /// <summary>
        /// Plays the animator state with the given name. Stops any currently playing animation first.
        /// <para>Requires a <see cref="RuntimeAnimatorController"/> to be loaded either automatically from an asset bundle or manually via <see cref="SetController"/>.</para>
        /// </summary>
        /// <param name="stateName">The name of the animator state to play.</param>
        public void Play(string stateName)
        {
            Stop();

            if (_animator.runtimeAnimatorController == null)
            {
                LogManager.Error($"No controller could be loaded for '{Schematic.FileName}' ensure an asset bundle exists alongside the schematic.");
                return;
            }

            IsPlaying = true;
            _animator.Play(stateName);
            _handle = Timing.RunCoroutine(AnimationCoroutine(stateName));
            LogManager.Info($"Playing state '{stateName}' on '{Schematic.FileName}'.");
        }

        /// <summary>
        /// Stops the currently playing animation and kills the sync coroutine.
        /// </summary>
        public void Stop()
        {
            if (_handle.IsRunning)
                Timing.KillCoroutines(_handle);

            _animator.StopPlayback();
            IsPlaying = false;
        }

        /// <summary>
        /// Stops the animation, removes this controller from <see cref="AnimationSchematics"/>, and destroys the underlying dummy <see cref="GameObject"/>.
        /// </summary>
        public void Destroy()
        {
            Stop();
            AnimationSchematics.Remove(Schematic);

            if (_dummyRoot != null)
                Object.Destroy(_dummyRoot);

            LogManager.Debug($"Destroyed for '{Schematic.FileName}'.");
        }

        private IEnumerator<float> AnimationCoroutine(string stateName)
        {
            Transform schematicRoot = Schematic.Primitive.Transform;
            yield return Timing.WaitForOneFrame;
            Quaternion rootInverseRot = Quaternion.Inverse(schematicRoot.rotation);

            while (true)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                foreach (PrimitiveDummy dummy in _dummies)
                {
                    dummy.Primitive.Position = schematicRoot.InverseTransformPoint(dummy.Transform.position);
                    dummy.Primitive.Rotation = rootInverseRot * dummy.Transform.rotation;
                    dummy.Primitive.Scale = new(dummy.Transform.lossyScale.x / schematicRoot.lossyScale.x, dummy.Transform.lossyScale.y / schematicRoot.lossyScale.y, dummy.Transform.lossyScale.z / schematicRoot.lossyScale.z);
                    dummy.Primitive.Color = dummy.Animatable.color;
                    dummy.Primitive.PrimitiveType = (PrimitiveType)dummy.Animatable.primitiveType;
                    dummy.Primitive.PrimitiveFlags = PrimitiveFlags.None;

                    dummy.Primitive.FlushSync();
                }
                
                if (!stateInfo.loop && stateInfo.normalizedTime >= 1f)
                {
                    IsPlaying = false;
                    LogManager.Info($"State '{stateName}' finished on '{Schematic.FileName}'.");
                    yield break;
                }

                yield return GetTickRate(Schematic.Position);
            }
        }

        private float GetTickRate(Vector3 pos)
        {
            float closest = float.MaxValue;
            foreach (Player player in Player.ReadyList)
            {
                if (!player.IsAlive)
                    continue;

                closest = Mathf.Min(closest, (player.Position - pos).sqrMagnitude);
            }

            if (closest < 25f)
                return Timing.WaitForOneFrame;

            if (closest < 100f)
                return 0.05f;

            return 0.25f;
        }
    }
}