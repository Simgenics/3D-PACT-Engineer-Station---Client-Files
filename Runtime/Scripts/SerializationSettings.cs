using System;
using Newtonsoft.Json;

namespace LMS.API
{
	[Serializable]
	public class SerializationSettings
	{
		public DefaultValueHandling defaultValueHandling = DefaultValueHandling.Include;
		public DateFormatHandling dateFormatHandling = DateFormatHandling.IsoDateFormat;
		public DateParseHandling dateParseHandling = DateParseHandling.DateTime;
		public MissingMemberHandling missingMemberHandling = MissingMemberHandling.Ignore;
		public NullValueHandling nullValueHandling = NullValueHandling.Ignore;
		public ReferenceLoopHandling referenceLoopHandling = ReferenceLoopHandling.Error;
		public StringEscapeHandling stringEscapeHandling = StringEscapeHandling.Default;
		public TypeNameHandling typeNameHandling = TypeNameHandling.None;
	}
}
