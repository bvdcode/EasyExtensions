// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Npgsql.Metadata;
using EasyExtensions.EntityFrameworkCore.Npgsql.Models;

namespace EasyExtensions.Tests
{
    public class PostgresIndexMetadataTests
    {
        private static readonly PostgresVectorIndexDefinition Expected = new(
            "public", "embeddings", "ix_embeddings", "embedding", 1024, "index_version", 1);

        [TestCase("(embedding)::vector(1024)", "(index_version = 1)")]
        [TestCase("(\"embedding\")::\"vector\"(1024)", "(\"index_version\" = 1)")]
        [TestCase("((\"embedding\")::\"public\".\"vector\"(1024))", "(\"index_version\" = 1)")]
        [TestCase("embedding::public.vector(1024)", "index_version = 1")]
        public void EquivalentRendering_ReturnsSameDefinition(string expression, string predicate)
        {
            PostgresIndexMetadataRow row = CreateRow(expression, predicate);
            Assert.That(PostgresVectorIndexMetadata.Parse(row), Is.EqualTo(Expected));
        }

        [Test]
        public void QuotedIdentifiers_PreserveCasePunctuationAndEscapedQuotes()
        {
            PostgresIndexMetadataRow row = CreateRow(
                "(\"Embedding \"\"Value\"\"\")::\"vector\"(1024)",
                "(\"Version \"\"Number\"\"\" = 1)");

            PostgresVectorIndexDefinition expected = Expected with
            {
                VectorColumnName = "Embedding \"Value\"",
                FilterColumnName = "Version \"Number\""
            };

            Assert.That(PostgresVectorIndexMetadata.Parse(row), Is.EqualTo(expected));
        }

        [TestCase("index_version = '-1'::integer", -1)]
        [TestCase("(index_version = 2147483647)", int.MaxValue)]
        [TestCase("(index_version = '-2147483648'::integer)", int.MinValue)]
        public void IntegerPredicate_PreservesValue(string predicate, int value)
        {
            PostgresIndexMetadataRow row = CreateRow(predicate: predicate);
            Assert.That(PostgresVectorIndexMetadata.Parse(row), Is.EqualTo(Expected with { FilterValue = value }));
        }

        [TestCase("(index_version = 1) AND (enabled = true)")]
        [TestCase("(index_version = 1) OR (index_version = 2)")]
        [TestCase("(index_version <> 1)")]
        [TestCase("((index_version + 1) = 1)")]
        [TestCase("(index_version = 2147483648)")]
        [TestCase(null)]
        public void UnsupportedPredicate_HasNoCompatibleDefinition(string? predicate)
        {
            PostgresIndexMetadataRow row = CreateRow(predicate: predicate);
            Assert.That(PostgresVectorIndexMetadata.Parse(row), Is.Null);
        }

        [TestCase("(other_embedding)::vector(1024)")]
        [TestCase("(embedding)::vector(768)")]
        [TestCase("(normalize(embedding))::vector(1024)")]
        [TestCase("(embedding + other_embedding)::vector(1024)")]
        public void WrongVectorExpression_IsNotCompatible(string expression)
        {
            PostgresIndexStatus status = ToStatus(CreateRow(expression));
            Assert.That(status.IsCompatibleWith(Expected), Is.False);
        }

        [TestCase("ivfflat", "vector_cosine_ops", true, true, 1)]
        [TestCase("hnsw", "vector_l2_ops", true, true, 1)]
        [TestCase("hnsw", "vector_cosine_ops", false, true, 1)]
        [TestCase("hnsw", "vector_cosine_ops", true, false, 1)]
        [TestCase("hnsw", "vector_cosine_ops", true, true, 2)]
        public void DifferentCatalogStructure_IsNotCompatible(
            string method, string operatorClass, bool vectorType, bool vectorOperatorClass, int keyCount)
        {
            PostgresIndexMetadataRow row = CreateRow(
                method: method, operatorClass: operatorClass,
                vectorType: vectorType, vectorOperatorClass: vectorOperatorClass, keyCount: keyCount);
            Assert.That(ToStatus(row).IsCompatibleWith(Expected), Is.False);
        }

        [Test]
        public void DirectVectorColumn_UsesCatalogColumnAndDimensions()
        {
            PostgresIndexMetadataRow row = CreateRow(expression: null, columnName: "embedding");
            Assert.That(PostgresVectorIndexMetadata.Parse(row), Is.EqualTo(Expected));
        }

        [Test]
        public void Compatibility_UsesTypedValuesAndDoesNotDependOnDisplaySql()
        {
            PostgresIndexStatus status = ToStatus(CreateRow());
            using (Assert.EnterMultipleScope())
            {
                Assert.That(status.IsCompatibleWith(Expected), Is.True);
                Assert.That(status.IsCompatibleWith(Expected with { SchemaName = "other" }), Is.False);
                Assert.That(status.IsCompatibleWith(Expected with { TableName = "other" }), Is.False);
                Assert.That(status.IsCompatibleWith(Expected with { IndexName = "other" }), Is.False);
                Assert.That(status.IsCompatibleWith(Expected with { VectorColumnName = "other" }), Is.False);
                Assert.That(status.IsCompatibleWith(Expected with { Dimensions = 768 }), Is.False);
                Assert.That(status.IsCompatibleWith(Expected with { FilterColumnName = "other" }), Is.False);
                Assert.That(status.IsCompatibleWith(Expected with { FilterValue = 2 }), Is.False);
            }
        }

        [Test]
        public void AbsentIndex_IsNotCompatible()
        {
            PostgresIndexStatus status = new();
            Assert.That(status.IsCompatibleWith(Expected), Is.False);
        }

        private static PostgresIndexStatus ToStatus(PostgresIndexMetadataRow row)
        {
            return new PostgresIndexStatus
            {
                Exists = row.Exists,
                Definition = "Diagnostic SQL is not the compatibility contract.",
                VectorDefinition = PostgresVectorIndexMetadata.Parse(row)
            };
        }

        private static PostgresIndexMetadataRow CreateRow(
            string? expression = "(embedding)::vector(1024)",
            string? predicate = "(index_version = 1)",
            string method = "hnsw",
            string operatorClass = "vector_cosine_ops",
            bool vectorType = true,
            bool vectorOperatorClass = true,
            int keyCount = 1,
            string? columnName = null)
        {
            return new PostgresIndexMetadataRow
            {
                Exists = true,
                SchemaName = Expected.SchemaName,
                TableName = Expected.TableName,
                IndexName = Expected.IndexName,
                AccessMethod = method,
                OperatorClass = operatorClass,
                IsVectorType = vectorType,
                IsVectorOperatorClass = vectorOperatorClass,
                Dimensions = Expected.Dimensions,
                KeyCount = keyCount,
                AttributeCount = keyCount,
                ColumnName = columnName,
                Expression = expression,
                Predicate = predicate
            };
        }
    }
}
