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

         DeleteEntries(client);
         CreateEntities(client);
         QueryEntities(client);
      }

      // ReSharper disable once UnusedMember.Local
      private static void DeleteEntries(GraphClient client)
      {
         var query = client.Cypher.Match("(n)")
                                  .OptionalMatch("(n)-[r]-()")
                                  .Delete("n, r");
         query.ExecuteWithoutResults();

         if (client.CheckIndexExists("idx_artists", IndexFor.Node))
            client.DeleteIndex("idx_artists", IndexFor.Node);
      }

      // ReSharper disable once UnusedMember.Local
      private static void QueryEntities(GraphClient client)
      {
         var query = client.Cypher
                           .Start(new { n = Node.ByIndexLookup("idx_artists", "id", 1) })
                           .Match("n-[r1:COVERED]->e")
                           .Match("e<-[r2:WROTE]-x")
                           .ReturnDistinct((n, x) => new
                           {
                              CoveringArtist = n.As<Artist>().Name,
                              Composer = x.As<Artist>().Name,
                           });

         foreach (var result in query.Results)
            Console.WriteLine(result);
      }

      // ReSharper disable once UnusedMember.Local
      private static void CreateEntities(GraphClient client)
      {
         // Create index
         client.CreateIndex(
            "idx_artists",
            new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact },
            IndexFor.Node);

         // Create entities
         var jimiHendrix = client.Create(new Artist { Name = "Jimi Hendrix" }, null, new[] { new IndexEntry("idx_artists") { { "id", 1 } } });
         var bobDylan = client.Create(new Artist { Name = "Bob Dylan" }, null, new[] { new IndexEntry("idx_artists") { { "id", 2 } } });
         var theBeetles = client.Create(new Artist { Name = "The Beetles" }, null, new[] { new IndexEntry("idx_artists") { { "id", 3 } } });
         var chuckBerry = client.Create(new Artist { Name = "Chuck Berry" }, null, new[] { new IndexEntry("idx_artists") { { "id", 4} } });

         var voodooChile = client.Create(new Track {Name = "Voodoo Chile"});
         var allAlongTheWatchtower = client.Create(new Track {Name = "All Along the Watchtower"});
         var dayTripper = client.Create(new Track {Name = "Day Tripper"});
         var sergentPepper = client.Create(new Track {Name = "Sergent Pepper's Lonely Hearts Club Band"});
         var johnnyBGoode = client.Create(new Track {Name = "Johnny B. Goode"});

         // Create relationships
         client.CreateRelationship(jimiHendrix, new WroteRelationship(voodooChile));
         client.CreateRelationship(bobDylan, new WroteRelationship(allAlongTheWatchtower));
         client.CreateRelationship(theBeetles, new WroteRelationship(dayTripper));
         client.CreateRelationship(theBeetles, new WroteRelationship(sergentPepper));
         client.CreateRelationship(chuckBerry, new WroteRelationship(johnnyBGoode));

         client.CreateRelationship(jimiHendrix, new CoveredRelationship(allAlongTheWatchtower));
         client.CreateRelationship(jimiHendrix, new CoveredRelationship(dayTripper));
         client.CreateRelationship(jimiHendrix, new CoveredRelationship(sergentPepper));
         client.CreateRelationship(jimiHendrix, new CoveredRelationship(johnnyBGoode));
      }
   }

   public class Artist
   {
      public string Name { get; set; }
   }
   
   public class Track
   {
      public string Name { get; set; }
   }

   public class WroteRelationship :
      Relationship, IRelationshipAllowingSourceNode<Artist>, IRelationshipAllowingTargetNode<Track>
   {
      private const string TypeKey = "WROTE";

      public WroteRelationship(NodeReference targetNode)
         : base(targetNode)
      { }

      public override string RelationshipTypeKey
      {
         get { return TypeKey; }
      }
   }

   public class CoveredRelationship :
      Relationship, IRelationshipAllowingSourceNode<Artist>, IRelationshipAllowingTargetNode<Track>
   {
      private const string TypeKey = "COVERED";

      public CoveredRelationship(NodeReference targetNode)
         : base(targetNode)
      { }

      public override string RelationshipTypeKey
      {
         get { return TypeKey; }
      }
   }
}
