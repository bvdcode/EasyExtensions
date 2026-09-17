// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using EasyExtensions.EntityFrameworkCore.Npgsql.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EasyExtensions.EntityFrameworkCore.Npgsql.Metadata
{
    internal static class PostgresIndexMetadataReader
    {
        private const string Query = """
            SELECT i.indexrelid IS NOT NULL AS "Exists",
                   COALESCE(i.indisvalid, false) AS "IsValid",
                   COALESCE(pg_catalog.pg_get_indexdef(i.indexrelid), '') AS "Definition",
                   COALESCE(pg_catalog.pg_relation_size(i.indexrelid), 0) AS "SizeBytes",
                   EXISTS (
                       SELECT 1
                       FROM pg_catalog.pg_stat_progress_create_index p
                       WHERE p.datname = current_database()
                         AND p.relid = pg_catalog.to_regclass({1})
                   ) AS "IsBuilding",
                   ns.nspname::text AS "SchemaName",
                   tbl.relname::text AS "TableName",
                   idx.relname::text AS "IndexName",
                   am.amname::text AS "AccessMethod",
                   opc.opcname::text AS "OperatorClass",
                   EXISTS (
                       SELECT 1 FROM pg_catalog.pg_depend d
                       JOIN pg_catalog.pg_extension e ON e.oid = d.refobjid
                       WHERE d.classid = 'pg_catalog.pg_type'::regclass
                         AND d.objid = typ.oid AND d.refclassid = 'pg_catalog.pg_extension'::regclass
                         AND d.deptype = 'e' AND e.extname = 'vector' AND typ.typname = 'vector'
                   ) AS "IsVectorType",
                   EXISTS (
                       SELECT 1 FROM pg_catalog.pg_depend d
                       JOIN pg_catalog.pg_extension e ON e.oid = d.refobjid
                       WHERE d.classid = 'pg_catalog.pg_opclass'::regclass
                         AND d.objid = opc.oid AND d.refclassid = 'pg_catalog.pg_extension'::regclass
                         AND d.deptype = 'e' AND e.extname = 'vector'
                   ) AS "IsVectorOperatorClass",
                   key.atttypmod AS "Dimensions",
                   i.indnkeyatts::integer AS "KeyCount",
                   i.indnatts::integer AS "AttributeCount",
                   col.attname::text AS "ColumnName",
                   pg_catalog.pg_get_expr(i.indexprs, i.indrelid, false) AS "Expression",
                   pg_catalog.pg_get_expr(i.indpred, i.indrelid, false) AS "Predicate"
            FROM (SELECT pg_catalog.to_regclass({0}) AS oid) target
            LEFT JOIN pg_catalog.pg_index i
                ON i.indexrelid = target.oid AND i.indrelid = pg_catalog.to_regclass({1})
            LEFT JOIN pg_catalog.pg_class idx ON idx.oid = i.indexrelid
            LEFT JOIN pg_catalog.pg_class tbl ON tbl.oid = i.indrelid
            LEFT JOIN pg_catalog.pg_namespace ns ON ns.oid = tbl.relnamespace
            LEFT JOIN pg_catalog.pg_am am ON am.oid = idx.relam
            LEFT JOIN pg_catalog.pg_opclass opc ON opc.oid = i.indclass[0]
            LEFT JOIN pg_catalog.pg_attribute key ON key.attrelid = i.indexrelid AND key.attnum = 1
            LEFT JOIN pg_catalog.pg_type typ ON typ.oid = key.atttypid
            LEFT JOIN pg_catalog.pg_attribute col ON col.attrelid = i.indrelid AND col.attnum = i.indkey[0]
            """;

        public static async Task<PostgresIndexStatus> ReadAsync(
            DatabaseFacade database,
            string qualifiedIndexName,
            string qualifiedTableName,
            CancellationToken cancellationToken)
        {
            PostgresIndexMetadataRow row = await database
                .SqlQueryRaw<PostgresIndexMetadataRow>(Query, qualifiedIndexName, qualifiedTableName)
                .SingleAsync(cancellationToken);

            return new PostgresIndexStatus
            {
                Exists = row.Exists,
                IsValid = row.IsValid,
                Definition = row.Definition,
                SizeBytes = row.SizeBytes,
                IsBuilding = row.IsBuilding,
                VectorDefinition = PostgresVectorIndexMetadata.Parse(row)
            };
        }
    }
}
