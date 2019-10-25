
This is a Mongorepository that can be used with Azure's DocumentDB (MongoDb Interface) as well as a normal MongoDb server.

# Usage for MongoDb 
```
<configuration>
  <connectionStrings>
    <add name="MongoServerSettings" connectionString="mongodb://[username]:[password]@[server ip]:[port]/[DatabaseName]" />
  </connectionStrings>
</configuration>
```
 

# Usage for DocumentDb using MongoDb Api
```
<configuration>
  <connectionStrings>
    <add name="MongoServerSettings" connectionString="[ConnectionStringFoundInAzurePortal]/[DatabaseName]" />
  </connectionStrings>
</configuration>
```
# Passing connectionstring as argument
```
new MongoRepository<Person>("mongodb://azureUser:I74av6H11qw....thwoZCsC8w==@bla.documents.azure.com:10250/?ssl=true/testdb");
```