using Microsoft.VisualStudio.TestTools.UnitTesting;
using mstube.Controllers;
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
        public void CandidatesTest()
        {
            var controller = new ApiController();
            long user_id = 10001;
            var result = controller.Candidates(user_id);
            Assert.IsInstanceOfType(result, typeof(Task<JsonResult>));
        }

        [TestMethod()]
        public void ListTopicTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SearchTopicTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void PreferenceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void UpdateDBTest()
        {
            Assert.Fail();
        }
    }
}