using System;

namespace EasyExtensions.Analyzers.Example
{
	public class ExampleService(TimeProvider timeProvider)
	{
		public DateTimeOffset GetCurrentTime()
		{
			return timeProvider.GetUtcNow();
		}
	}
}
