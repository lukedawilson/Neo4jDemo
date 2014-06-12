using System;
using Neo4jClient;
using Neo4jDemo.Annotations;

namespace Neo4jDemo
{
   [UsedImplicitly]
   class Program
   {
      static void Main()
      {
         var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
         client.Connect();

         var newNode = client.Create(new {name = "Jimi Hendrix", type = "artist"});
         Console.WriteLine(newNode.Id);

         Console.ReadKey();

         client.Delete(newNode, DeleteMode.NodeAndRelationships);
      }
   }
}
