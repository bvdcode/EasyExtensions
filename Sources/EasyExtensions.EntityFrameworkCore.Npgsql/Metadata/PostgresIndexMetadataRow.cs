// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Metadata
{
    internal class PostgresIndexMetadataRow
    {
        public bool Exists { get; init; }
        public bool IsValid { get; init; }
        public string Definition { get; init; } = string.Empty;
        public long SizeBytes { get; init; }
        public bool IsBuilding { get; init; }
        public string? SchemaName { get; init; }
        public string? TableName { get; init; }
        public string? IndexName { get; init; }
        public string? AccessMethod { get; init; }
        public string? OperatorClass { get; init; }
        public bool IsVectorType { get; init; }
        public bool IsVectorOperatorClass { get; init; }
        public int? Dimensions { get; init; }
        public int? KeyCount { get; init; }
        public int? AttributeCount { get; init; }
        public string? ColumnName { get; init; }
        public string? Expression { get; init; }
        public string? Predicate { get; init; }
    }
}
