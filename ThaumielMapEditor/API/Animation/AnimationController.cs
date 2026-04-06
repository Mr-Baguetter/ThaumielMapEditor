// -----------------------------------------------------------------------
// <copyright file="AnimationController.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdminToys;
using LabApi.Features.Wrappers;
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
        private readonly List<PrimitiveDummy> _dummies;
        private CoroutineHandle _handle;

        public SchematicData Schematic { get; }
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
                LogManager.Debug($"Loaded controller '{controller.name}' for '{schematic.FileName}'.");
            }
            else
                LogManager.Warn($"No animator controller found for '{schematic.FileName}'. Animation will not play.");

            int primitiveCount = schematic.Primitives.Count();
            _dummies = new List<PrimitiveDummy>(primitiveCount);
            Dictionary<uint, Transform> dummyByNetId = new(primitiveCount);

            foreach (PrimitiveObject primitive in schematic.Primitives)
            {
                if (string.IsNullOrEmpty(primitive.Name))
                {
                    LogManager.Warn($"Skipping unnamed primitive in '{schematic.FileName}'  it cannot be targeted by animation curves.");
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
                RuntimeAnimatorController[] controllers = bundle.LoadAllAssets<RuntimeAnimatorController>();
                if (controllers.Length == 0)
                    continue;

                controller = controllers[0];
                return true;
            }

            string path = Path.Combine(Path.GetDirectoryName(ThaumFileManager.Dir(["Schematics"]))!,$"{schematic.FileName}-Animator");
            if (!File.Exists(path))
                return false;

            AssetBundle loadedBundle = AssetBundle.LoadFromFile(path);
            if (loadedBundle == null)
            {
                LogManager.Warn($"Failed to load asset bundle at '{path}'.");
                return false;
            }

            RuntimeAnimatorController[] bundleControllers = loadedBundle.LoadAllAssets<RuntimeAnimatorController>();
            if (bundleControllers.Length == 0)
                return false;

            controller = bundleControllers[0];
            return true;
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

        public void Play(string stateName)
        {
            Stop();

            if (_animator.runtimeAnimatorController == null)
            {
                LogManager.Error($"No controller could be loaded for '{Schematic.FileName}'  ensure an asset bundle exists alongside the schematic.");
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
            yield return Timing.WaitForOneFrame;

            while (true)
            {
                Vector3 rootPos = schematicRoot.position;
                Quaternion rootInverseRot = Quaternion.Inverse(schematicRoot.rotation);
                Vector3 rootLossyScale = schematicRoot.lossyScale;

                AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

                foreach (PrimitiveDummy dummy in _dummies)
                {
                    dummy.Primitive.Position = schematicRoot.InverseTransformPoint(dummy.Transform.position);
                    dummy.Primitive.Rotation = rootInverseRot * dummy.Transform.rotation;
                    Vector3 dummyScale = dummy.Transform.lossyScale;
                    dummy.Primitive.Scale = new(dummyScale.x / rootLossyScale.x, dummyScale.y / rootLossyScale.y, dummyScale.z / rootLossyScale.z);
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

                yield return GetTickRate(rootPos);
            }
        }

        private static float GetTickRate(Vector3 pos)
        {
            float closest = float.MaxValue;

            foreach (Player player in Player.ReadyList)
            {
                if (!player.IsAlive)
                    continue;

                float distSqr = (player.Position - pos).sqrMagnitude;

                if (distSqr < 25f)
                    return Timing.WaitForOneFrame;

                if (distSqr < closest)
                    closest = distSqr;
            }

            if (closest < 100f)
                return 0.05f;

            return 0.25f;
        }
    }
}