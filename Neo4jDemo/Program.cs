using System;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jDemo.Annotations;

namespace Neo4jDemo
{
   [UsedImplicitly]
   static class Program
   {
      static void Main()
      {
         var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
         client.Connect();

         DeleteEntries(client, 17, 18, 19, 20);
         CreateEntities(client);
         QueryEntities(client);
      }

      // ReSharper disable once UnusedMember.Local
      private static void DeleteEntries(GraphClient client, params int[] ids)
      {
         foreach (var id in ids)
            client.Delete(new NodeReference(id), DeleteMode.NodeAndRelationships);

         client.DeleteIndex("idx_People", IndexFor.Node);
      }

      // ReSharper disable once UnusedMember.Local
      private static void QueryEntities(GraphClient client)
      {
         var query = client.Cypher
                           .Start(new { n = Node.ByIndexLookup("idx_People", "initials", "RS") })
                           .Match("n-[r:HATES]->e")
                           .Return((n, e, r) => new
                           {
                              Subject = n.As<Person>().Name,
                              Object = e.As<Person>().Name,
                              r.As<HatesData>().Reason
                           });

         foreach (var result in query.Results)
            Console.WriteLine(result);
      }

      // ReSharper disable once UnusedMember.Local
      private static void CreateEntities(GraphClient client)
      {
         // Create index
         client.CreateIndex(
            "idx_People",
            new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact },
            IndexFor.Node);

         // Create entities
         var refA = client.Create(new Person { Name = "John" }, null, new[] { new IndexEntry("idx_People") { { "initials", "JL" } } });
         var refB = client.Create(new Person { Name = "Paul" }, null, new[] { new IndexEntry("idx_People") { { "initials", "PM" } } });
         var refC = client.Create(new Person { Name = "George" }, null, new[] { new IndexEntry("idx_People") { { "initials", "GH" } } });
         var refD = client.Create(new Person { Name = "Ringo" }, null, new[] { new IndexEntry("idx_People") { { "initials", "RS" } } });

         // Create relationships
         client.CreateRelationship(refA, new KnowsRelationship(refB));
         client.CreateRelationship(refB, new KnowsRelationship(refC));
         client.CreateRelationship(refB, new HatesRelationship(refD, new HatesData("Crazy guy")));
         client.CreateRelationship(refC, new HatesRelationship(refD, new HatesData("Don't know why...")));
         client.CreateRelationship(refD, new KnowsRelationship(refA));
      }
   }

   public class Person
   {
      public string Name { get; set; }
   }

   public class HatesData
   {
      public string Reason { [UsedImplicitly] get; set; }

      public HatesData()
      { }

      public HatesData(string reason)
      {
         Reason = reason;
      }
   }

   public class KnowsRelationship :
      Relationship, IRelationshipAllowingSourceNode<Person>, IRelationshipAllowingTargetNode<Person>
   {
      private const string TypeKey = "KNOWS";

      public KnowsRelationship(NodeReference targetNode)
         : base(targetNode)
      { }

      public override string RelationshipTypeKey
      {
         get { return TypeKey; }
      }
   }

   public class HatesRelationship :
      Relationship<HatesData>, IRelationshipAllowingSourceNode<Person>, IRelationshipAllowingTargetNode<Person>
   {
      private const string TypeKey = "HATES";

      public HatesRelationship(NodeReference targetNode, HatesData data)
         : base(targetNode, data)
      { }

      public override string RelationshipTypeKey
      {
         get { return TypeKey; }
      }
   }
}
