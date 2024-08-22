using System.Reflection;
using FluentAssertions;
using KingTech.Web.SimpleFileServer.Abstract.Sources;
using KingTech.Web.SimpleFileServer.Services;
using KingTech.Web.SimpleFileServer.UnitTests.Helpers;
using Moq;

namespace KingTech.Web.SimpleFileServer.UnitTests
{
    [TestClass]
    public class FileSourceServiceTests
    {
        [TestInitialize]
        public void Initialize()
        {

        }

        [TestMethod]
        public async Task SingleSourceReturnsDirectories_Success()
        {
            //assign
            var fileSourceMock = new Mock<IFileSource>();
            fileSourceMock.Setup(m => m.Name).Returns("TestSource");
            fileSourceMock.Setup(m => m.Enabled).Returns(true);
            fileSourceMock.Setup(m => m.ListDirectories(It.Is<string?>(s => s == null)))
                .ReturnsAsync(new List<string>() { "test1", "test2" });

            var service = CreateService(true, fileSourceMock.Object);

            //act
            var directories = await service.GetDirectories(null);

            //assert
            directories.Should().NotBeNull();
            directories.Should().HaveCount(2);
            directories.Should().BeEquivalentTo(new List<string>() { "test1", "test2" });
        }

        [TestMethod]
        public async Task MultiSourceReturnsSourceNames_Success()
        {
            //assign
            var fileSourceMock1 = new Mock<IFileSource>();
            fileSourceMock1.Setup(m => m.Name).Returns("TestSource1");
            fileSourceMock1.Setup(m => m.Enabled).Returns(true);
            fileSourceMock1.Setup(m => m.ListDirectories(It.Is<string?>(s => s == null)))
                .ReturnsAsync(new List<string>() { "test1", "test2" });

            var fileSourceMock2 = new Mock<IFileSource>();
            fileSourceMock2.Setup(m => m.Name).Returns("TestSource2");
            fileSourceMock2.Setup(m => m.Enabled).Returns(true);
            fileSourceMock2.Setup(m => m.ListDirectories(It.Is<string?>(s => s == null)))
                .ReturnsAsync(new List<string>() { "test3", "test4" });

            var service = CreateService(true, fileSourceMock1.Object, fileSourceMock2.Object);

            //act
            var directories = await service.GetDirectories(null);

            //assert
            directories.Should().NotBeNull();
            directories.Should().HaveCount(2);
            directories.Should().BeEquivalentTo(new List<string>() { "TestSource1", "TestSource2" });
        }

        /// <summary>
        /// Create a new <see cref="IFileSourceService"/> based on the given settings.
        /// </summary>
        /// <param name="prefixSourceName">Whether or not the PrefixSourceName flag is set.</param>
        /// <returns>A new <see cref="IFileSourceService"/> based on the given settings.</returns>
        private FileSourceService CreateService(bool prefixSourceName, params IFileSource[] fileSources)
        {
            var settings = new GeneralSettings()
            {
                PrefixSourceName = prefixSourceName
            };
            return new FileSourceService(TestHelper.CreateLogger<FileSourceService>(), settings, fileSources.ToList());
        }
    }
}