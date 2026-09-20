// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Npgsql.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace EasyExtensions.Tests
{
    public class PostgresHnswScanTests
    {
        [Test]
        public void NullDatabase_IsRejected()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() =>
                ((DatabaseFacade)null!).EnableHnswStrictOrderScanAsync());
        }

        [Test]
        public void NoTransaction_IsRejectedWithoutOpeningConnection()
        {
            using DbContext context = new(new DbContextOptionsBuilder()
                .UseNpgsql("Host=localhost;Database=unused;Username=postgres").Options);
            Assert.ThrowsAsync<InvalidOperationException>(() => context.Database.EnableHnswStrictOrderScanAsync());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Setting_IsLocalToTransaction(bool commit)
        {
            string? connectionString = Environment.GetEnvironmentVariable("EASYEXTENSIONS_TEST_POSTGRES");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                Assert.Ignore("EASYEXTENSIONS_TEST_POSTGRES must point to a disposable PostgreSQL database.");
                return;
            }

            await using DbContext context = new(new DbContextOptionsBuilder().UseNpgsql(connectionString).Options);
            await context.Database.OpenConnectionAsync();
            await context.Database.ExecuteSqlRawAsync("SET hnsw.iterative_scan = off;");
            await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
            await context.Database.EnableHnswStrictOrderScanAsync();
            Assert.That(await ReadSettingAsync(context), Is.EqualTo("strict_order"));

            if (commit)
            {
                await transaction.CommitAsync();
            }
            else
            {
                await transaction.RollbackAsync();
            }

            Assert.That(await ReadSettingAsync(context), Is.EqualTo("off"));
        }

        private static Task<string> ReadSettingAsync(DbContext context)
        {
            return context.Database.SqlQueryRaw<string>(
                "SELECT current_setting('hnsw.iterative_scan') AS \"Value\"").SingleAsync();
        }
    }
}
