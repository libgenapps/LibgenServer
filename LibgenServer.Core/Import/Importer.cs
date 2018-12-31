using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LibgenServer.Core.Database;
using LibgenServer.Core.Entities;
using LibgenServer.Core.Import.SqlDump;
using static LibgenServer.Core.Common.Constants;

namespace LibgenServer.Core.Import
{
    public abstract class Importer
    {
        public delegate void ImportProgressReporter(int objectsAdded, int objectsUpdated);

        public class ImportResult
        {
            public ImportResult(int addedObjectCount, int updatedObjectCount)
            {
                AddedObjectCount = addedObjectCount;
                UpdatedObjectCount = updatedObjectCount;
            }

            public int AddedObjectCount { get; }
            public int UpdatedObjectCount { get; }
        }

        public abstract ImportResult Import(SqlDumpReader sqlDumpReader, ImportProgressReporter progressReporter, double progressUpdateInterval,
            SqlDumpReader.ParsedTableDefinition parsedTableDefinition);
    }

    public abstract class Importer<T> : Importer where T : LibgenObject, new()
    {
        private readonly BitArray existingLibgenIds;
        private readonly TableDefinition<T> tableDefinition;
        private readonly List<T> currentBatchObjectsToInsert;
        private readonly List<T> currentBatchObjectsToUpdate;

        protected Importer(LocalDatabase localDatabase, BitArray existingLibgenIds, TableDefinition<T> tableDefinition)
        {
            LocalDatabase = localDatabase;
            this.existingLibgenIds = existingLibgenIds;
            IsUpdateMode = existingLibgenIds != null && existingLibgenIds.Length > 0;
            this.tableDefinition = tableDefinition;
            currentBatchObjectsToInsert = new List<T>(DATABASE_TRANSACTION_BATCH);
            currentBatchObjectsToUpdate = new List<T>(DATABASE_TRANSACTION_BATCH);
        }

        protected LocalDatabase LocalDatabase { get; }
        protected bool IsUpdateMode { get; }

        public override ImportResult Import(SqlDumpReader sqlDumpReader, ImportProgressReporter progressReporter, double progressUpdateInterval,
            SqlDumpReader.ParsedTableDefinition parsedTableDefinition)
        {
            List<Action<T, string>> sortedColumnSetters =
                tableDefinition.GetSortedColumnSetters(parsedTableDefinition.Columns.Select(column => column.ColumnName));
            return Import(sqlDumpReader.ParseImportObjects(sortedColumnSetters), progressReporter, progressUpdateInterval);
        }

        public ImportResult Import(IEnumerable<T> importingObjects, ImportProgressReporter progressReporter, double progressUpdateInterval)
        {
            DateTime lastProgressUpdateDateTime = DateTime.Now;
            int addedObjectCount = 0;
            int updatedObjectCount = 0;
            currentBatchObjectsToInsert.Clear();
            currentBatchObjectsToUpdate.Clear();
            progressReporter(addedObjectCount, updatedObjectCount);
            foreach (T importingObject in importingObjects)
            {
                if (importingObject.LibgenId == 282536)
                {
                    System.Diagnostics.Debugger.Break();
                }
                if (!IsUpdateMode || existingLibgenIds.Length <= importingObject.LibgenId || !existingLibgenIds[importingObject.LibgenId])
                {
                    currentBatchObjectsToInsert.Add(importingObject);
                }
                else if (IsNewObject(importingObject))
                {
                    int? existingObjectId = GetExistingObjectIdByLibgenId(importingObject.LibgenId);
                    if (existingObjectId.HasValue)
                    {
                        importingObject.Id = existingObjectId.Value;
                        currentBatchObjectsToUpdate.Add(importingObject);
                    }
                    else
                    {
                        currentBatchObjectsToInsert.Add(importingObject);
                    }
                }
                if (currentBatchObjectsToInsert.Count + currentBatchObjectsToUpdate.Count == DATABASE_TRANSACTION_BATCH)
                {
                    if (currentBatchObjectsToInsert.Any())
                    {
                        InsertBatch(currentBatchObjectsToInsert);
                        addedObjectCount += currentBatchObjectsToInsert.Count;
                        currentBatchObjectsToInsert.Clear();
                    }
                    if (currentBatchObjectsToUpdate.Any())
                    {
                        UpdateBatch(currentBatchObjectsToUpdate);
                        updatedObjectCount += currentBatchObjectsToUpdate.Count;
                        currentBatchObjectsToUpdate.Clear();
                    }
                    DateTime now = DateTime.Now;
                    if ((now - lastProgressUpdateDateTime).TotalSeconds > progressUpdateInterval)
                    {
                        progressReporter(addedObjectCount, updatedObjectCount);
                        lastProgressUpdateDateTime = now;
                    }
                }
            }
            if (currentBatchObjectsToInsert.Any())
            {
                InsertBatch(currentBatchObjectsToInsert);
                addedObjectCount += currentBatchObjectsToInsert.Count;
            }
            if (currentBatchObjectsToUpdate.Any())
            {
                UpdateBatch(currentBatchObjectsToUpdate);
                updatedObjectCount += currentBatchObjectsToUpdate.Count;
            }
            progressReporter(addedObjectCount, updatedObjectCount);
            return new ImportResult(addedObjectCount, updatedObjectCount);
        }

        protected abstract void InsertBatch(List<T> objectBatch);
        protected abstract void UpdateBatch(List<T> objectBatch);
        protected abstract bool IsNewObject(T importingObject);
        protected abstract int? GetExistingObjectIdByLibgenId(int libgenId);
    }
}
