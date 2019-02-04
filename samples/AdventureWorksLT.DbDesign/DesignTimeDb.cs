﻿#if DbDesign
using DevZest.Data;
using System.IO;

namespace DevZest.Samples.AdventureWorksLT
{
    public sealed class DesignTimeDb : DesignTimeDb<Db>
    {
        public override Db Create(string projectPath)
        {
            var dbFolder = Path.Combine(projectPath, @"LocalDb");
            string attachDbFilename = Path.Combine(dbFolder, "AdventureWorksLT.Design.mdf");
            var connectionString = string.Format(@"Data Source=(localdb)\MSSQLLocalDB;AttachDbFilename=""{0}"";Integrated Security=True", attachDbFilename);
            return new Db(connectionString);
        }
    }
}
#endif