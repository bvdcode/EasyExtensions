// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System.Globalization;
using System.Text.RegularExpressions;
using EasyExtensions.EntityFrameworkCore.Npgsql.Models;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Metadata
{
    internal static class PostgresVectorIndexMetadata
    {
        private const string Identifier = """(?:"(?:""|[^"])+"|[\p{L}_][\p{L}\p{Nd}_$]*)""";
        private static readonly Regex VectorExpression = new(
            """\A\s*\(*\s*(?<column>""" + Identifier +
            """)\s*\)*\s*::\s*(?:(?:""" + Identifier +
            """)\s*\.\s*)?(?:"vector"|vector)\s*\(\s*(?<dimensions>[0-9]+)\s*\)\s*\)*\s*\z""",
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);
        private static readonly Regex IntegerPredicate = new(
            """\A\s*\(*\s*(?<column>""" + Identifier +
            """)\s*\)*\s*=\s*\(*\s*(?:(?<value>-?[0-9]+)|'(?<value>-?[0-9]+)'::(?:integer|bigint|smallint))\s*\)*\s*\z""",
            RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

        public static PostgresVectorIndexDefinition? Parse(PostgresIndexMetadataRow row)
        {
            if (!row.Exists || row.AccessMethod != "hnsw" ||
                row.OperatorClass != "vector_cosine_ops" ||
                !row.IsVectorType || !row.IsVectorOperatorClass ||
                row.KeyCount != 1 || row.AttributeCount != 1 ||
                row.Dimensions is not > 0 ||
                row.SchemaName is null || row.TableName is null || row.IndexName is null ||
                row.Predicate is null)
            {
                return null;
            }

            string? vectorColumn = row.ColumnName;
            if (row.Expression is not null)
            {
                Match expression = VectorExpression.Match(row.Expression);
                if (!expression.Success ||
                    !int.TryParse(expression.Groups["dimensions"].Value, CultureInfo.InvariantCulture, out int dimensions) ||
                    dimensions != row.Dimensions)
                {
                    return null;
                }

                vectorColumn = Unquote(expression.Groups["column"].Value);
            }

            Match predicate = IntegerPredicate.Match(row.Predicate);
            if (vectorColumn is null || !predicate.Success ||
                !int.TryParse(predicate.Groups["value"].Value, CultureInfo.InvariantCulture, out int filterValue))
            {
                return null;
            }

            return new PostgresVectorIndexDefinition(
                row.SchemaName,
                row.TableName,
                row.IndexName,
                vectorColumn,
                row.Dimensions.Value,
                Unquote(predicate.Groups["column"].Value),
                filterValue);
        }

        private static string Unquote(string identifier)
        {
            if (identifier.StartsWith('"'))
            {
                return identifier[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal);
            }

            return identifier;
        }
    }
}
