namespace Graph;

using GraphQL;
using GraphQL.Execution;

public class CustomErrorInfoProvider : ErrorInfoProvider
{
	private readonly bool isDevelopment;

	public CustomErrorInfoProvider(bool isDevelopment)
	{
		this.isDevelopment = isDevelopment;
	}

	public override ErrorInfo GetInfo(ExecutionError executionError)
	{
		var info = base.GetInfo(executionError);
		if (isDevelopment)
		{
			var details = new List<object>();
			var inner = executionError.InnerException;
			while (inner != null)
			{
				var innerDetail = new Dictionary<string, object>
				{
					["message"] = String.Format("{0}: {1}", inner.GetType().Name, inner.Message),
				};
				if (!String.IsNullOrEmpty(inner.StackTrace))
				{
					innerDetail["stack"] = inner.StackTrace.Split("\n").Select(x => x.Trim()).ToList();
				}
				details.Add(innerDetail);
				inner = inner.InnerException;
			}

			if (details.Count > 0)
			{
				info.Extensions ??= new Dictionary<string, object?>();
				info.Extensions["details"] = details;
			}
		}
		return info;
	}
}
