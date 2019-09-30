using ASC.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Microsoft.Extensions.Options;
using ASC.Web.Configuration;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ASC.Tests.TestUtilities;
using ASC.Utilities;

namespace ASC.Tests
{
    public class HomeControllerTests
    {

        private readonly Mock<IOptions<ApplicationSettings>> _optionsMock;
        private readonly Mock<HttpContext> _mockHttpContext;

        public HomeControllerTests()
        {
            // Create an instance of Mock IOptions
            _optionsMock = new Mock<IOptions<ApplicationSettings>>();
            _mockHttpContext = new Mock<HttpContext>();
            // Set FakeSession to HttpContext Session.
            _mockHttpContext.Setup(p => p.Session).Returns(new FakeSession());

            // Set IOptions<> Values property to return ApplicationSettings object
            _optionsMock.Setup(ap => ap.Value).Returns(new ApplicationSettings { ApplicationTitle = "ASC" });
        }     

        /// <summary>
        /// The Theory attribute, on the other hand, tests the same test for multiple variant conditions by
        /// passing different data to the test method as arguments.
        /// </summary>
        [Fact]
        public void HomeController_Index_View_Test()
        {
            // Home controller instantiated with Mock IOptions<> object
            // passamos o objeto para o construtor da classe
            var controller = new HomeController(_optionsMock.Object);

            controller.ControllerContext.HttpContext = _mockHttpContext.Object;

            // Assert.IsType(typeof(ViewResult), controller.Index());
            Assert.IsType<ViewResult>(controller.Index());
        }

        [Fact]
        public void HomeController_Index_NoModel_Test()
        {

            // Home controller instantiated with Mock IOptions<> object
            var controller = new HomeController(_optionsMock.Object);
            controller.ControllerContext.HttpContext = _mockHttpContext.Object;

            Assert.Null((controller.Index() as ViewResult).ViewData.Model);
        }

        [Fact]
        public void HomeController_Index_Validation_Test()
        {
            var controller = new HomeController(_optionsMock.Object);
            controller.ControllerContext.HttpContext = _mockHttpContext.Object;
            // Assert ModelState Error Count to 0
            Assert.Equal(0, (controller.Index() as ViewResult).ViewData.ModelState.ErrorCount);
        }

        [Fact]
        public void HomeController_Index_Session_Test()
        {
            var controller = new HomeController(_optionsMock.Object);
            controller.ControllerContext.HttpContext = _mockHttpContext.Object;

            controller.Index();

            // Session value with key "Test" should not be null.
            Assert.NotNull(controller.HttpContext.Session.GetSession<ApplicationSettings>("Test"));

        }
    }
}
