﻿using GabrielBigardi.SpriteAnimator;
using UnityEngine;
using Weapons;
using Weapons.Services;

namespace Player.Components
{
    /// <summary>
    /// Interface for transformation coordinators that handle different aspects of transformations
    /// </summary>
    public interface ITransformationCoordinator
    {
        /// <summary>
        /// Apply transformation logic
        /// </summary>
        void ApplyTransformation(SpriteAnimationObject animationObject, Sprite transitionTexture, WeaponType weapon);
        
        /// <summary>
        /// Revert transformation logic  
        /// </summary>
        void RevertTransformation();
    }
}
