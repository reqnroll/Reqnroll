namespace Reqnroll.RuntimeTests.AssistTests.TableHelperExtensionMethods
{
#if NET5_0
    public class RecordSupport
    {
        public record RecordType(string Property);

        [Fact]
        public void Works_With_Records()
        {
            var table = new Table("Property");
            table.AddRow("Row1");

            var record = table.CreateSet<RecordType>();
            record.Count().Should().Be(1);
        }


    }
#endif
}
