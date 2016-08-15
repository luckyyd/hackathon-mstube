using Microsoft.VisualStudio.TestTools.UnitTesting;
using mstube.Controllers;
using mstube.Item;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace mstube.Controllers.Tests
{
    [TestClass()]
    public class ApiControllerTests
    {
        [TestMethod()]
        public void IndexTest()
        {
            var controller = new ApiController();
            Assert.IsNotNull(controller.Index());
        }

        [TestMethod()]
        public void UserIdTest()
        {
            var controller = new ApiController();
            string uuid = "testUUID";
            var result = controller.UserId(uuid);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            string str = JsonConvert.SerializeObject(result.Data);
            Assert.IsInstanceOfType(str, typeof(string));
            Assert.IsNotNull(str);
        }

        [TestMethod()]
        public async Task CandidatesTest()
        {
            var controller = new ApiController();
            var result = await controller.Candidates(9);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod()]
        public async Task RandomCandidatesTest()
        {
            var controller = new ApiController();
            var result = await controller.Candidates(new Random().Next(50000, 60000));
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod()]
        public void ListTopicTest()
        {
            var controller = new ApiController();
            var result = controller.ListTopic();
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            Assert.IsNotNull(result);
            List<Item.Topic> topics = JsonConvert.DeserializeObject<List<Item.Topic>>(JsonConvert.SerializeObject(result.Data));
            if (topics.Count > 0)
            {
                Assert.IsNotNull(topics[0].topic);
            }
        }

        [TestMethod()]
        public void SearchNoneTopicTest()
        {
            var controller = new ApiController();
            string topic = "NoneTopic";
            var result = controller.SearchTopic(topic);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            List<Item.Item> items = JsonConvert.DeserializeObject<List<Item.Item>>(JsonConvert.SerializeObject(result.Data));
            Assert.AreEqual(items.Count, 0);
        }

        [TestMethod()]
        public void SearchExistTopicTest()
        {
            var controller = new ApiController();
            var listResult = controller.ListTopic();
            List<Item.Topic> topics = JsonConvert.DeserializeObject<List<Item.Topic>>(JsonConvert.SerializeObject(listResult.Data));
            Assert.IsNotNull(topics);
            int counts = topics.Count;
            string topic = topics[new Random().Next(counts)].topic;

            var searchResult = controller.SearchTopic(topic);
            Assert.IsInstanceOfType(searchResult, typeof(JsonResult));
            List<Item.Item> items = JsonConvert.DeserializeObject<List<Item.Item>>(JsonConvert.SerializeObject(searchResult.Data));
            Assert.AreNotEqual(items.Count, 0);
            Assert.IsNotNull(items[new Random().Next(items.Count)].item_id);
        }
        [TestMethod()]
        public void SearchAzureTopicTest()
        {
            var controller = new ApiController();
            string topic = "Azure";
            var result = controller.SearchTopic(topic);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            List<Item.Item> items = JsonConvert.DeserializeObject<List<Item.Item>>(JsonConvert.SerializeObject(result.Data));
            Assert.AreNotEqual(items.Count, 0);
        }
        [TestMethod()]
        public void SearchAzureTitleTest()
        {
            var controller = new ApiController();
            string title = "Azure";
            var result = controller.SearchTopic(title);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            List<Item.Item> items = JsonConvert.DeserializeObject<List<Item.Item>>(JsonConvert.SerializeObject(result.Data));
            Assert.AreNotEqual(items.Count, 0);
        }
    }
}