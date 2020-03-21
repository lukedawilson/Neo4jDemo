# Intorduction to Neo4j

An internal interactive session given as part of Trayport's employee tech talk series.

## Download link

http://www.neo4j.org/download_thanks?edition=community&release=2.0.3&platform=windows&packaging=exe&architecture=x64

## Documentation (some of this is out-of-date)

https://github.com/Readify/Neo4jClient/wiki

https://github.com/Readify/Neo4jClient/wiki/cypher

https://github.com/Readify/Neo4jClient/wiki/indexes

## .NET SDK

https://www.nuget.org/packages/Neo4jClient

## Examples

### Connect to the database

```c#
var client = new GraphClient(new Uri("http://localhost:7474/db/data"));
client.Connect();
```

### Create an index

```c#
client.CreateIndex(
   "index_name",
   new IndexConfiguration { Provider = IndexProvider.lucene, Type = IndexType.exact },
   IndexFor.Node);
```

### Create and insert a node on the given index

```c#
var nodeRef = client.Create(
   new FooNode { BarProperty = "baz value" },
   null,
   new[] { new IndexEntry("index_name") { { "key", VALUE } } });
```

### Create a relationship between two nodes

```c#
public class MyRelationship :
   Relationship,
   IRelationshipAllowingSourceNode<FooNode>,
   IRelationshipAllowingTargetNode<BarNode>
{
   private const string TypeKey = "MY_RELATIONSHIP_TYPE";

   public MyRelationship(NodeReference targetNode, MyRelationshipData data)
      : base(targetNode, data)
   { }

   public override string RelationshipTypeKey
   {
      get { return TypeKey; }
   }
}
```
