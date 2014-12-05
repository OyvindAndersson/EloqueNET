#EloqueNET# 
*A C# library that is extremely influenced by the fantastic Eloquent ORM in Laravel*

Current Support
-------------------------
######QueryBuilder######
Select, insert, wheres, joins, unions, having, groupby, orderby, limit, offset ++
Base and MySql grammar

Usage
-------------------------
######QueryBuilder - Builder.cs######

**Create connection type**
```C#
DB db = new MySqlDB(server, user, pw, database);
```

**Nested wheres (Using a callback delegate taking a Builder instance)**
```C#
ResultSet results = db.Table("users")
  .Where("name", Is.Like, "%doe")
  .OrWhere( q => q.Where("age", Is.LessThan, 25).Where("age", Is.GreaterThan, 18) )
  .Limit(4)
  .Get(new string[] { "id", "name", "age" });
```

**Retrieving a single row from a table**
```C#
  ResultSet result = db.Table("users").Where("name", Is.EqualTo, "John").First();
```

**Inserting a row**
```C#
  int result = db.Table("users").Insert(new Column("name", "John"), new Column("title", "Mr."));
```

```C#
  ColumnList columns = new ColumnList();
  // Add columns...

  int result = db.Table("users").Insert(columns);
```

And a lot more, as well as a lot more to come.

TODO
-----------------------
######Finish implementation for Builder######
* [ ] Cache functionality for queries
* [ ] Eager / lazy loading

######Later on######
* [ ] Schemabuilder
* [ ] Migrations
* [ ] Model relationships
* [ ] Different grammar types (besides base SQL grammar, and MySQL, which is already in
  
... and a lot more.

Summary
-----------------------
Codebase needs a proper optimization and needs to be cleaned from redundancies. 
Some classes are "need-quick-results" based, and need a proper revision.

Thinks are working as they should, so the cleanup will come inbetween, but in large scale when most
query features are done and sorted.

Regards,
Oyvind
