using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoRepository;

namespace Tests
{
    [TestClass]
    public class ReadWriteTest
    {
        [TestMethod]
        public void Write()
        {
            var repo = new MongoRepository<PersonEnity>(
                "mongodb://[azureconnectionstring]/[bdname]");

          var result=  repo.Add(new PersonEnity
            {
               
                Age = 20,
                Hobbies = new List<string>() { "cars", "planes", "botes" },
                Name = "Jannie"
            });


        }

        [CollectionName("Persons")]
        public class PersonEnity : IEntity
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public List<string> Hobbies { get; set; }
            public string Id { get; set; }
        }
    }
}
