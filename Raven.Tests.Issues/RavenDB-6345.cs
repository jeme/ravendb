﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client.Indexes;
using Raven.Tests.Common;
using Xunit;

namespace Raven.Tests.Issues
{
    public class RavenDB_6345: RavenTest
    {
        [Fact]
        void NotOperatorShouldBeInvokedOnTheProperAstNode()
        {
            using (var store = NewDocumentStore())
            {
                store.ExecuteIndex(new SomeClassIndex());
                using (var session = store.OpenSession())
                {
                    session.Store(new SomeClass {Culture = "EU",CatalogId = "Catalog/Test",ModelId = 4});
                    session.SaveChanges();
                    WaitForIndexing(store);
                    var query = session.Advanced.DocumentQuery<SomeClass>("SomeClassIndex").WhereEquals("Culture","EU").AndAlso().Not.WhereEquals("ModelId",4).AndAlso().WhereEquals("CatalogId", "Catalog/Test");
                    Assert.Empty(query.ToList());
                }
            }
        }

        public class SomeClassIndex : AbstractIndexCreationTask<SomeClass>
        {
            public SomeClassIndex()
            {
                Map = docs => from doc in docs select new {doc.Culture,doc.ModelId,doc.CatalogId};
            }
        }
        public class SomeClass
        {
            public string Culture { get; set; }
            public int ModelId { get; set; }
            public string CatalogId { get; set; }
        }
    }
}
