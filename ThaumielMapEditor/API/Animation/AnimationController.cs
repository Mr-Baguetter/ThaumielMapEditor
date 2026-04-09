// -----------------------------------------------------------------------
// <copyright file="AnimationController.cs" company="Thaumiel Team">
// Copyright (c) Thaumiel Team. All rights reserved.
// Licensed under the GNU General Public License v3.0 (GPL-3.0).
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using ThaumielMapEditor.API.Blocks;
using ThaumielMapEditor.API.Data;
using UnityEngine;

namespace ThaumielMapEditor.API.Animation
{
    public class AnimationController
    {
        internal static readonly Dictionary<SchematicData, AnimationController> Dictionary = [];

        internal AnimationController(SchematicData schematic)
        {
            AttachedSchematic = schematic;

            List<Animator> animators = [];
            foreach (ServerObject obj in schematic.SpawnedServerObjects)
            {
                if (obj.Object != null && obj.Object.TryGetComponent(out Animator animator))
                    animators.Add(animator);
            }

            Animators = animators;
            Dictionary[schematic] = this;
        }

        /// <summary>
        /// Gets the attached <see cref="SchematicData"/>.
        /// </summary>
        public SchematicData AttachedSchematic { get; }

        /// <summary>
        /// Gets a read-only list of all <see cref="Animator"/> components found on the schematic's server-side objects.
        /// </summary>
        public IReadOnlyList<Animator> Animators { get; }

        /// <summary>
        /// Plays an animation state by name on the animator at the given index.
        /// </summary>
        /// <param name="stateName">The state to play.</param>
        /// <param name="animatorIndex">The index of the animator to use.</param>
        public void Play(string stateName, int animatorIndex = 0) => Animators[animatorIndex].Play(stateName);

        /// <summary>
        /// Sets a boolean parameter on the animator at the given index.
        /// </summary>
        /// <param name="animParam">The animator parameter name.</param>
        /// <param name="state">The boolean value to set.</param>
        /// <param name="animatorIndex">The index of the animator to use.</param>
        public void Play(string animParam, bool state, int animatorIndex = 0) => Animators[animatorIndex].SetBool(animParam, state);

        /// <summary>
        /// Plays an animation state on the animator matching the given name.
        /// </summary>
        /// <param name="stateName">The state to play.</param>
        /// <param name="animatorName">The name of the animator GameObject to target.</param>
        public void Play(string stateName, string animatorName) => Animators.FirstOrDefault(a => a.name == animatorName)?.Play(stateName);

        /// <summary>
        /// Stops playback on the animator at the given index.
        /// </summary>
        /// <param name="animatorIndex">The index of the animator to stop.</param>
        public void Stop(int animatorIndex = 0) => Animators[animatorIndex].StopPlayback();

        /// <summary>
        /// Stops playback on the animator matching the given name.
        /// </summary>
        /// <param name="animatorName">The name of the animator GameObject to stop.</param>
        public void Stop(string animatorName) => Animators.FirstOrDefault(a => a.name == animatorName)?.StopPlayback();

        /// <summary>
        /// Gets or creates an <see cref="AnimationController"/> for the given <see cref="SchematicData"/>.
        /// </summary>
        /// <param name="schematic">The schematic to look up.</param>
        /// <returns>The existing or newly created <see cref="AnimationController"/>.</returns>
        public static AnimationController Get(SchematicData schematic) => Dictionary.TryGetValue(schematic, out AnimationController? controller) ? controller : new AnimationController(schematic);

        internal static void Remove(SchematicData schematic) => Dictionary.Remove(schematic);
    }
}