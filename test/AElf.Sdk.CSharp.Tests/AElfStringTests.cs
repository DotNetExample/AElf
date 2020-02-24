using AElf.Types;
using Shouldly;
using Xunit;

namespace AElf.Sdk.CSharp.Tests
{
    public class AElfStringTests
    {
        [Fact]
        public void Concat_ArrayString_Test()
        {
            var array = new[] {"test1", "test2", "test3", "test4"};
            var concatResult = AElfString.Concat(array);
            var emptyInfo = string.Empty;
            concatResult.ShouldBe(string.Join(emptyInfo, array));
        }

        [Fact]
        public void Concat_ArrayObj_Test()
        {
            var info = new object[] {1, 2, 3, 4};
            var concatResult = AElfString.Concat(info);
            concatResult.ShouldBe("1234");
        }

        [Fact]
        public void Concat_StringParameters_Test()
        {
            var info1 = "test1";
            var info2 = "test2";
            var info3 = "test3";

            var result1 = AElfString.Concat(info1, info2);
            result1.ShouldBe("test1test2");

            var result2 = AElfString.Concat(info1, info2, info3);
            result2.ShouldBe("test1test2test3");
        }

        [Fact]
        public void Concat_ObjectParameters_Test()
        {
            object info1 = 24;
            object info2 = "test";
            object info3 = Hash.Empty;
            object info4 = new[] {2, 4, 6};

            var result1 = AElfString.Concat(info1, info2);
            result1.ShouldBe("24test");

            var result2 = AElfString.Concat(info1, info2, info3);
            result2.ShouldBe("24test" + info3);

            var result3 = AElfString.Concat(info1, info2, info3, info4);
            result3.ShouldBe("24test" + info3 + info4);
        }
    }
}