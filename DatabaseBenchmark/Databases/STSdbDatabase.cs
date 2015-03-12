﻿using STSdb.Data;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DatabaseBenchmark.Benchmarking;
using STS.General.Generators;

namespace DatabaseBenchmark.Databases
{
    public class STSdbDatabase : Database
    {
        private StorageEngine engine;
        private XTable<long, Tick> table;

        public STSdbDatabase()
        {
            SyncRoot = new object();

            DatabaseName = "STSdb";
            DatabaseCollection = "table1";
            Category = "NoSQL\\Key-Value Store";
            Description = "STSdb 3.5.13";
            Website = "http://www.stsdb.com/";
            Color = Color.DeepSkyBlue;
            Requirements = new string[]
            {
                "STSdb.dll"
            };
        }

        public override void Init(int flowCount, long flowRecordCount)
        {
            engine = StorageEngine.FromFile(Path.Combine(DataDirectory, "test.stsdb"));
            table = engine.Scheme.CreateOrOpenXTable<long, Tick>(new Locator(DatabaseCollection));

            engine.Scheme.Commit();
        }

        public override void Write(int flowID, IEnumerable<KeyValuePair<long, Tick>> flow)
        {
            lock (SyncRoot)
            {
                foreach (var kv in flow)
                    table[kv.Key] = kv.Value;

                table.Commit();
            }
        }

        public override IEnumerable<KeyValuePair<long, Tick>> Read()
        {
            foreach (var row in table.Forward())
                yield return new KeyValuePair<long, Tick>(row.Key, row.Record);
        }

        public override void Finish()
        {
            engine.Dispose();
        }
    }
}