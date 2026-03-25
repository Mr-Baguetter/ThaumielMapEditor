using System.Collections.Generic;
using System.Linq;
using AdminToys;
using MEC;
using ThaumielMapEditor.API.Blocks.ClientSide;
using ThaumielMapEditor.API.Data;
using ThaumielMapEditor.API.Helpers;
using UnityEngine;

namespace ThaumielMapEditor.API.Animation
{
    public class AnimationController
    {
        public static readonly Dictionary<SchematicData, AnimationController> AnimationSchematics = [];
        private readonly record struct PrimitiveDummy(PrimitiveObject Primitive, Transform Transform, DummyAnimatable Animatable);

        private readonly GameObject _dummyRoot;
        private readonly Animator _animator;
        private readonly List<PrimitiveDummy> _dummies = [];
        private CoroutineHandle _handle;

        public SchematicData Schematic { get; }
        public bool IsPlaying { get; private set; }

        internal AnimationController(SchematicData schematic)
        {
            Schematic = schematic;
            _dummyRoot = new GameObject($"[AnimDummy] {schematic.FileName}");
            _animator = _dummyRoot.AddComponent<Animator>();
            _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            Dictionary<uint, Transform> dummyByNetId = [];

            foreach (PrimitiveObject primitive in schematic.SpawnedClientObjects.OfType<PrimitiveObject>())
            {
                if (string.IsNullOrEmpty(primitive.Name))
                {
                    LogManager.Warn($"Skipping unnamed primitive in '{schematic.FileName}' — it cannot be targeted by animation curves.");
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

        public static AnimationController GetOrCreate(SchematicData schematic)
        {
            if (AnimationSchematics.TryGetValue(schematic, out AnimationController? existing))
                return existing;

            return new AnimationController(schematic);
        }

        public void SetController(RuntimeAnimatorController controller)
        {
            _animator.runtimeAnimatorController = controller;
            LogManager.Debug($"Controller '{controller.name}' assigned to '{Schematic.FileName}'.");
        }

        /// <summary>
        /// Plays an animator state by name. Call SetController first.
        /// </summary>
        public void Play(string stateName)
        {
            Stop();

            if (_animator.runtimeAnimatorController == null)
            {
                LogManager.Error($"No controller assigned on '{Schematic.FileName}' — call SetController first.");
                return;
            }

            IsPlaying = true;
            _animator.Play(stateName);
            _handle = Timing.RunCoroutine(AnimationCoroutine(stateName));
            LogManager.Info($"Playing state '{stateName}' on '{Schematic.FileName}'.");
        }

        public void Stop()
        {
            if (_handle.IsRunning)
                Timing.KillCoroutines(_handle);

            _animator.StopPlayback();
            IsPlaying = false;
        }

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

            // Wait one frame for the Animator to initialise the state.
            yield return Timing.WaitForOneFrame;

            while (true)
            {
                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                foreach (PrimitiveDummy dummy in _dummies)
                {
                    dummy.Primitive.Position = schematicRoot.InverseTransformPoint(dummy.Transform.position);
                    dummy.Primitive.Rotation = Quaternion.Inverse(schematicRoot.rotation) * dummy.Transform.rotation;
                    dummy.Primitive.Scale = new Vector3(
                        dummy.Transform.lossyScale.x / schematicRoot.lossyScale.x,
                        dummy.Transform.lossyScale.y / schematicRoot.lossyScale.y,
                        dummy.Transform.lossyScale.z / schematicRoot.lossyScale.z);
                    dummy.Primitive.Color = dummy.Animatable.color;
                    dummy.Primitive.PrimitiveType = (PrimitiveType)dummy.Animatable.primitiveType;
                    dummy.Primitive.PrimitiveFlags = (PrimitiveFlags)dummy.Animatable.primitiveFlags;

                    dummy.Primitive.FlushSync();
                }

                // Stop once a non-looping state has finished its first cycle.
                if (!stateInfo.loop && stateInfo.normalizedTime >= 1f)
                {
                    IsPlaying = false;
                    LogManager.Info($"State '{stateName}' finished on '{Schematic.FileName}'.");
                    yield break;
                }

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}