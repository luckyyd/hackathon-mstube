using Microsoft.VisualStudio.TestTools.UnitTesting;
using mstube.Controllers;
using mstube.Item;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            long user_id = 30001;
            var result = await controller.Candidates(user_id);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod()]
        public void ListTopicTest()
        {
            var controller = new ApiController();
            var result = controller.ListTopic();
            System.Diagnostics.Debug.WriteLine(result);
            Assert.IsNotNull(result);
        }

        [TestMethod()]
        public void SearchNoneTopicTest()
        {
            var controller = new ApiController();
            string topic = "NoneTopic";
            var result = controller.SearchTopic(topic);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod()]
        public void SearchExistTopicTest()
        {
            var controller = new ApiController();
            var listResult = controller.ListTopic();
            List<Item.Topic> topics = JsonConvert.DeserializeObject<List<Item.Topic>>(JsonConvert.SerializeObject(listResult.Data));
            Assert.IsNotNull(topics);
            string topic = topics[0].topic;
            var searchResult = controller.SearchTopic(topic);
            Assert.IsInstanceOfType(searchResult, typeof(JsonResult));
            List<Item.Item> items = JsonConvert.DeserializeObject<List<Item.Item>>(JsonConvert.SerializeObject(searchResult.Data));
            Assert.IsNotNull(items);
        }

        [TestMethod()]
        public void PreferenceTest()
        {
            //Assert.Fail();
        }

        [TestMethod()]
        public void UpdateDBTest()
        {
            //Assert.Fail();
        }
    }
}