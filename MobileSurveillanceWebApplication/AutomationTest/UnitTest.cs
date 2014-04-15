using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using MobileSurveillanceWebApplication.Models.DatabaseModel;

namespace MobileSurveillanceWebApplication.AutomationTest
{
    [TestFixture]
    public class UnitTest
    {
        //Testcase check if ViewName of Index is correct
        [Test]        
        public void TestCase1()
        {
            string expected = "Index";
            MobileSurveillanceWebApplication.Controllers.UserController controller = new MobileSurveillanceWebApplication.Controllers.UserController();
            MobileSurveillanceWebApplication.Models.ViewModel.SearchCriteriaViewModel searchUserModel = new Models.ViewModel.SearchCriteriaViewModel();
            var result = controller.Index() as ViewResult;

            Assert.AreEqual(expected, result.ViewName);
        }


        //Testcase check if ViewName of ListInbox is correct
        [Test]        
        public void TestCase2()
        {
            string expected = "ListInbox";
            MobileSurveillanceWebApplication.Controllers.UserController controller = new MobileSurveillanceWebApplication.Controllers.UserController();
            MobileSurveillanceWebApplication.Models.ViewModel.SearchCriteriaViewModel searchUserModel1 = new Models.ViewModel.SearchCriteriaViewModel();
            searchUserModel1.PageCount = 0;
            searchUserModel1.PageNumber = 1;            
            searchUserModel1.SearchKeyword = "";
            var result = controller.ListInbox(searchUserModel1) as ViewResult;

            Assert.AreEqual(expected, result.ViewName);
        }


        //Testcase check if ListFriendTrajectory controller redirect to right action
        [Test]
        public void TestCase3()
        {
            string expected = "ListTrajectory";
            MobileSurveillanceWebApplication.Controllers.UserController controller = new MobileSurveillanceWebApplication.Controllers.UserController();

            var result = (RedirectToRouteResult)controller.ListFriendTrajectory(1);

            Assert.AreEqual(expected, result.RouteValues["action"]);
        }

        //Testcase check if ListFriendTrajectory controller redirect to right action
        [Test]
        public void TestCase4()
        {
            string expected = "ListTrajectory";
            MobileSurveillanceWebApplication.Controllers.UserController controller = new MobileSurveillanceWebApplication.Controllers.UserController();

            var result = (RedirectToRouteResult)controller.MakeFriendRequest(1);

            Assert.AreEqual(expected, result.RouteValues["action"]);
        }

        //Testcase check if ListFriendTrajectory controller redirect to right action
        [Test]
        public void TestCase5()
        {
            var expected = new
            {
                countInbox = 1,
                countFriends = 23
            };
            MobileSurveillanceWebApplication.Controllers.UserController controller = new Controllers.UserController();
            JsonResult actual = controller.GetCountItems() as JsonResult;                                    

            Assert.AreEqual(expected, actual.Data);
        }
    }
}