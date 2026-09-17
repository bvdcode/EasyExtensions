// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Npgsql.Extensions;
using EasyExtensions.EntityFrameworkCore.Npgsql.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyExtensions.Tests
{
    [NonParallelizable]
    public class PostgresIndexMetadataIntegrationTests
    {
        private DbContext context = null!;
        private string schemaName = null!;

        [OneTimeSetUp]
        public async Task SetUpAsync()
        {
            string? connectionString = Environment.GetEnvironmentVariable("EASYEXTENSIONS_TEST_POSTGRES");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Assert.Ignore("EASYEXTENSIONS_TEST_POSTGRES must point to a disposable PostgreSQL database with pgvector.");
                return;
            }

            DbContextOptions options = new DbContextOptionsBuilder().UseNpgsql(connectionString).Options;
            context = new DbContext(options);
            await context.Database.OpenConnectionAsync();
            await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector");

            schemaName = "Metadata.Test_" + Guid.NewGuid().ToString("N");
            string createSchema = $"CREATE SCHEMA \"{schemaName}\"";
            await context.Database.ExecuteSqlRawAsync(createSchema);
            string createTable = $"""
                CREATE TABLE "{schemaName}"."Embeddings" (
                    "Embedding" vector,
                    "OtherEmbedding" vector,
                    "IndexVersion" integer,
                    enabled boolean
                )
                """;
            await context.Database.ExecuteSqlRawAsync(createTable);
        }

        [OneTimeTearDown]
        public async Task TearDownAsync()
        {
            if (context is null)
            {
                return;
            }

            if (schemaName is not null)
            {
                string dropSchema = $"DROP SCHEMA \"{schemaName}\" CASCADE";
                await context.Database.ExecuteSqlRawAsync(dropSchema);
            }

            await context.DisposeAsync();
        }

        [Test]
        public async Task QuoteAllIdentifiers_DoesNotChangeCompatibility()
        {
            PostgresVectorIndexDefinition expected = Definition();
            await context.Database.CreateVectorCosineHnswIndexConcurrentlyAsync(expected);

            await context.Database.ExecuteSqlRawAsync("SET quote_all_identifiers = off");
            PostgresIndexStatus normal = await StatusAsync(expected);
            await context.Database.ExecuteSqlRawAsync("SET quote_all_identifiers = on");
            PostgresIndexStatus quoted = await StatusAsync(expected);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(quoted.Definition, Is.Not.EqualTo(normal.Definition));
                Assert.That(normal.IsCompatibleWith(expected), Is.True);
                Assert.That(quoted.IsCompatibleWith(expected), Is.True);
                Assert.That(quoted.VectorDefinition, Is.EqualTo(normal.VectorDefinition));
                Assert.That(quoted.IsValid, Is.True);
                Assert.That(quoted.SizeBytes, Is.GreaterThan(0));
                Assert.That(quoted.IsBuilding, Is.False);
            }
        }

        [TestCase(1)]
        [TestCase(-1)]
        [TestCase(int.MinValue)]
        public async Task TypedDefinition_RoundTripsPredicateValues(int filterValue)
        {
            PostgresVectorIndexDefinition expected = Definition() with { FilterValue = filterValue };
            await context.Database.CreateVectorCosineHnswIndexConcurrentlyAsync(expected);
            PostgresIndexStatus status = await StatusAsync(expected);
            Assert.That(status.IsCompatibleWith(expected), Is.True);
        }

        [Test]
        public async Task EscapedQuotesInIdentifiers_RoundTrip()
        {
            string createTable = $"""
                CREATE TABLE "{schemaName}"."Quoted" ("Emb""edding" vector, "Ver""sion" integer)
                """;
            await context.Database.ExecuteSqlRawAsync(createTable);
            PostgresVectorIndexDefinition expected = Definition() with
            {
                TableName = "Quoted",
                IndexName = "ix\"quoted",
                VectorColumnName = "Emb\"edding",
                FilterColumnName = "Ver\"sion"
            };
            await context.Database.CreateVectorCosineHnswIndexConcurrentlyAsync(expected);
            Assert.That((await StatusAsync(expected)).IsCompatibleWith(expected), Is.True);
        }

        [TestCase("vector_l2_ops", 3, """("IndexVersion" = 1)""")]
        [TestCase("vector_cosine_ops", 2, """("IndexVersion" = 1)""")]
        [TestCase("vector_cosine_ops", 3, """("IndexVersion" = 2)""")]
        [TestCase("vector_cosine_ops", 3, """("IndexVersion" = 1 AND enabled)""")]
        public async Task DifferentStructure_IsNotCompatible(string operatorClass, int dimensions, string predicate)
        {
            PostgresVectorIndexDefinition expected = Definition();
            await context.Database.ExecuteSqlRawAsync(FormattableString.Invariant($"""
                CREATE INDEX "{expected.IndexName}" ON "{schemaName}"."Embeddings"
                USING hnsw (("Embedding"::vector({dimensions})) {operatorClass})
                WHERE {predicate}
                """));
            Assert.That((await StatusAsync(expected)).IsCompatibleWith(expected), Is.False);
        }

        [Test]
        public async Task WrongTableOrNonIndexRelation_DoesNotReportAnIndex()
        {
            PostgresVectorIndexDefinition expected = Definition();
            await context.Database.CreateVectorCosineHnswIndexConcurrentlyAsync(expected);

            PostgresIndexStatus wrongTable = await context.Database.GetIndexStatusAsync(
                schemaName, "MissingTable", expected.IndexName);
            PostgresIndexStatus notIndex = await context.Database.GetIndexStatusAsync(
                schemaName, expected.TableName, expected.TableName);
            PostgresIndexStatus missing = await StatusAsync(expected with { IndexName = "missing_index" });

            using (Assert.EnterMultipleScope())
            {
                Assert.That(wrongTable.Exists, Is.False);
                Assert.That(notIndex.Exists, Is.False);
                Assert.That(missing.Exists, Is.False);
                Assert.That(wrongTable.VectorDefinition, Is.Null);
                Assert.That(notIndex.VectorDefinition, Is.Null);
                Assert.That(missing.IsCompatibleWith(expected), Is.False);
            }
        }

        private PostgresVectorIndexDefinition Definition()
        {
            return new PostgresVectorIndexDefinition(
                schemaName, "Embeddings", "ix_" + Guid.NewGuid().ToString("N"), "Embedding", 3, "IndexVersion", 1);
        }

        private Task<PostgresIndexStatus> StatusAsync(PostgresVectorIndexDefinition definition)
        {
            return context.Database.GetIndexStatusAsync(
                definition.SchemaName, definition.TableName, definition.IndexName);
        }
    }
}
