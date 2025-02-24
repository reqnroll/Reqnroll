using FluentAssertions;
using NSubstitute;
using Reqnroll.Analytics.UserId;
using Xunit;

namespace Reqnroll.GeneratorTests.MSBuildTask
{
    public class FileUserIdStoreTests
    {
        private const string UserId = "491ed5c0-9f25-4c27-941a-19b17cc81c87";
        
        IFileService fileServiceStub;
        IDirectoryService directoryServiceStub;
        FileUserIdStore sut;

        public FileUserIdStoreTests()
        {
            fileServiceStub = Substitute.For<IFileService>();
            directoryServiceStub = Substitute.For<IDirectoryService>();
            sut = new FileUserIdStore(fileServiceStub, directoryServiceStub);
        }

        private void GivenUserIdStringInFile(string userIdString)
        {
            fileServiceStub.ReadAllText(Arg.Any<string>()).Returns(userIdString);
        }

        private void GivenFileExists()
        {
            fileServiceStub.Exists(Arg.Any<string>()).Returns(true);
        }

        private void GivenFileDoesNotExists()
        {
            fileServiceStub.Exists(Arg.Any<string>()).Returns(false);
        }

        [Fact]
        public void Should_GetUserIdFromFile_WhenFileExists()
        {
            GivenFileExists();
            GivenUserIdStringInFile(UserId);

            string userId = sut.GetUserId();

            userId.Should().Be(UserId);
        }

        [Fact]
        public void Should_GenerateNewUserId_WhenFileDoesNotExists()
        {
            GivenFileDoesNotExists();

            string userId = sut.GetUserId();

            userId.Should().NotBeEmpty();
        }

        [Fact]
        public void Should_PersistNewlyGeneratedUserId_WhenNoUserIdExists()
        {
            GivenFileDoesNotExists();

            string userId = sut.GetUserId();

            userId.Should().NotBeEmpty();
            fileServiceStub.Received(1).WriteAllText(FileUserIdStore.UserIdFilePath, userId); // TODO NSub
        }
    }
}
