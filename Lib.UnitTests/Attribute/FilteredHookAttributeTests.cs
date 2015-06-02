using System.Linq;
using Gauge.CSharp.Lib.Attribute;
using NUnit.Framework;

namespace Gauge.CSharp.Lib.UnitTests.Attribute
{
    [TestFixture]
    public class FilteredHookAttributeTests
    {
        [Test]
        public void ShouldCreateAttributeWithNoParameters()
        {
            var filteredHookAttribute = new FilteredHookAttribute();
            Assert.IsNotNull(filteredHookAttribute);
        }

        [Test]
        public void ShouldCreateAttributeWithOneTag()
        {
            var filterTag = "foo";
            var filteredHookAttribute = new FilteredHookAttribute(filterTag);
            Assert.IsTrue(filteredHookAttribute.FilterTags.Contains(filterTag));
        }

        [Test]
        public void ShouldCreateAttributeWithMultipleTags()
        {
            var filterTags = new[] {"foo", "bar"};
            var filteredHookAttribute = new FilteredHookAttribute(filterTags);

            foreach (var filterTag in filterTags)
            {
                Assert.IsTrue(filteredHookAttribute.FilterTags.Contains(filterTag));
            }
        }

        [Test]
        public void MultipleTagsAttributeShouldHaveDefaultAggregation()
        {
            var filterTags = new[] {"foo", "bar"};
            var filteredHookAttribute = new FilteredHookAttribute(filterTags);
            Assert.AreEqual(TagAggregation.And, filteredHookAttribute.TagAggregation);
        }

        [Test]
        public void MultipleTagsAttributeShouldTakeAggregation()
        {
            var filterTags = new[] {"foo", "bar"};
            var filteredHookAttribute = new FilteredHookAttribute(filterTags, TagAggregation.Or);
            Assert.AreEqual(TagAggregation.Or, filteredHookAttribute.TagAggregation);
        }
    }
}
