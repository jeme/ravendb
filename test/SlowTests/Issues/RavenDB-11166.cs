﻿using System;
using System.Threading.Tasks;
using FastTests;
using Raven.Client.Documents.Subscriptions;
using Sparrow.Server;
using Xunit;

namespace SlowTests.Issues
{
    public class RavenDB_11166 : RavenTestBase
    {
        private class Dog
        {
#pragma warning disable 414
            public string Name;
#pragma warning restore 414
            public string Owner;
        }

        private class Person
        {
#pragma warning disable 414
            public string Name;
#pragma warning restore 414
        }

        [Fact]
        public async Task CanUseSubscriptionWithIncludes()
        {
            using (var store = GetDocumentStore())
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new Person
                    {
                        Name = "Arava"
                    }, "people/1");
                    session.Store(new Dog
                    {
                        Name = "Oscar",
                        Owner = "people/1"
                    });
                    session.SaveChanges();
                }
                var id = store.Subscriptions.Create(new SubscriptionCreationOptions
                {
                    Query = @"from Dogs include Owner"
                });

                using (var sub = store.Subscriptions.GetSubscriptionWorker<Dog>(id))
                {
                    var mre = new AsyncManualResetEvent();
                    var r = sub.Run(batch =>
                    {
                        Assert.NotEmpty(batch.Items);
                        using (var s = batch.OpenSession())
                        {
                            foreach (var item in batch.Items)
                            {
                                s.Load<Person>(item.Result.Owner);
                                var dog = s.Load<Dog>(item.Id);
                                Assert.Same(dog, item.Result);
                            }
                            Assert.Equal(0, s.Advanced.NumberOfRequests);
                        }
                        mre.Set();
                    });
                    Assert.True(await mre.WaitAsync(TimeSpan.FromSeconds(60)));
                    await sub.DisposeAsync();
                    await r;// no error
                }

            }
        }

        [Fact]
        public async Task CanUseSubscriptionWithIncludesViaJavaScript()
        {
            using (var store = GetDocumentStore())
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new Person
                    {
                        Name = "Arava"
                    }, "people/1");
                    session.Store(new Dog
                    {
                        Name = "Oscar",
                        Owner = "people/1"
                    });
                    session.SaveChanges();
                }
                var id = store.Subscriptions.Create(new SubscriptionCreationOptions
                {
                    Query = @"declare function f(d) { 
    include(d.Owner);
    return d;
}
from Dogs as dog
select f(dog)
"
                });

                using (var sub = store.Subscriptions.GetSubscriptionWorker<Dog>(id))
                {
                    var mre = new AsyncManualResetEvent();
                    var r = sub.Run(batch =>
                    {
                        Assert.NotEmpty(batch.Items);
                        using (var s = batch.OpenSession())
                        {
                            foreach (var item in batch.Items)
                            {
                                s.Load<Person>(item.Result.Owner);
                                var dog = s.Load<Dog>(item.Id);
                                Assert.Same(dog, item.Result);
                            }
                            Assert.Equal(0, s.Advanced.NumberOfRequests);
                        }
                        mre.Set();
                    });
                    Assert.True(await mre.WaitAsync(TimeSpan.FromSeconds(60)));
                    await sub.DisposeAsync();
                    await r;// no error
                }

            }
        }
    }
}
