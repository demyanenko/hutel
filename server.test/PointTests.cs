using System;
using System.Collections.Generic;
using System.Linq;
using hutel.Logic;
using hutel.Models;
using Xunit;
using static hutel.Tests.Constants;
using static hutel.Tests.Helpers;

namespace hutel.Tests
{
    public class PointTests
    {
        public static Tag CompleteTag
        {
            get
            {
                return new Tag
                {
                    Id = "completeTag",
                    Fields = new List<BaseTagField>
                    {
                        CreateIntField(),
                        CreateFloatField(),
                        CreateStringField(),
                        CreateDateField(),
                        CreateTimeField(),
                        CreateEnumField()
                    }.ToDictionary(field => field.Name, field => field)
                };
            }
        }
        public static Tag EmptyTag
        {
            get
            {
                return new Tag
                {
                    Id = "emptyTag",
                    Fields = new Dictionary<string, BaseTagField>()
                };
            }
        }
        public static Dictionary<string, Tag> AllTags
        {
            get
            {
                return new[] { EmptyTag, CompleteTag }.ToDictionary(tag => tag.Id, tag => tag);
            }
        }

        [Fact]
        public void FromJsonValid()
        {
            var extraFields = new Dictionary<string, Object>
            {
                { IntFieldName, (Int64)0 },
                { FloatFieldName, (Double)0.0 },
                { StringFieldName, "" },
                { DateFieldName, "2000-01-10" },
                { TimeFieldName, "10:09:08" },
                { EnumFieldName, EnumValueA }
            };
            var pointJson = new PointWithIdJson
            {
                Id = SampleGuid,
                TagId = CompleteTag.Id,
                Date = SampleDate,
                Extra = extraFields
            };
            var point = Point.FromJson(pointJson, AllTags);
            Assert.Equal(point.Id, pointJson.Id);
            Assert.Equal(point.TagId, CompleteTag.Id);
            Assert.Equal(point.Date.DateTime, new DateTime(2000, 10, 1));
            var pointJsonExtraList = pointJson.Extra.Keys.ToList();
            pointJsonExtraList.Sort();
            var pointExtraList = point.Extra.Keys.ToList();
            pointExtraList.Sort();
            Assert.Equal(pointJsonExtraList, pointExtraList);
        }

        [Fact]
        public void FromJsonUnknownTag()
        {
            var pointJson = new PointWithIdJson
            {
                Id = SampleGuid,
                TagId = "unknownTag",
                Date = SampleDate,
                Extra = new Dictionary<string, Object>()
            };
            Assert.Throws<PointValidationException>(() => Point.FromJson(pointJson, AllTags));
        }

        [Fact]
        public void FromJsonUnknownProperty()
        {
            var pointJson = new PointWithIdJson
            {
                Id = SampleGuid,
                TagId = EmptyTag.Id,
                Date = SampleDate,
                Extra = new Dictionary<string, Object>
                {
                    { "unknownField", "value" }
                }
            };
            Assert.Throws<PointValidationException>(() => Point.FromJson(pointJson, AllTags));
        }

        [Fact]
        public void FromJsonMissingProperty()
        {
            var pointJson = new PointWithIdJson
            {
                Id = SampleGuid,
                TagId = CompleteTag.Id,
                Date = SampleDate,
                Extra = new Dictionary<string, Object>()
            };
            Assert.Throws<PointValidationException>(() => Point.FromJson(pointJson, AllTags));
        }
    }
}