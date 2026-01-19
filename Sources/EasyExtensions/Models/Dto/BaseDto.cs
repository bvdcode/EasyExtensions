// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;

namespace EasyExtensions.Models.Dto
{
    /// <summary>
    /// Base Data Transfer Object (DTO) with generic identifier.
    /// </summary>
    /// <typeparam name="TId"> Type of identifier. </typeparam>
    public abstract class BaseDto<TId> where TId : struct
    {
        /// <summary>
        /// Entity identifier.
        /// </summary>
        public TId Id { get; set; }
        /// <summary>
        /// Created at UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// Updated at UTC.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
