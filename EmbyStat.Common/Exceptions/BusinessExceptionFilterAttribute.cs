﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Serilog;

namespace EmbyStat.Common.Exceptions
{
	public class BusinessExceptionFilterAttribute : ExceptionFilterAttribute
	{
		public override void OnException(ExceptionContext context)
		{
			ApiError apiError;
			if (context.Exception is BusinessException)
			{
				var ex = context.Exception as BusinessException;
#if !DEBUG
                string stack = null;
#else
				string stack = context.Exception.StackTrace;
#endif
				apiError = new ApiError(ex.Message, stack);
				context.Exception = null;

				context.HttpContext.Response.StatusCode = ex.StatusCode;
				Log.Warning($"{Constants.LogPrefix.ExceptionHandler}\tApplication thrown error: {ex.Message}", ex);
				Log.Warning($"{Constants.LogPrefix.ExceptionHandler}\tFrontend will know what to do with this!");
			}
			else
			{
#if !DEBUG
                var msg = "An unhandled error occurred.";
                string stack = null;
#else
				var msg = context.Exception.GetBaseException().Message;
				string stack = context.Exception.StackTrace;
#endif
			    Log.Error(context.Exception, $"{Constants.LogPrefix.ExceptionHandler}\tUnhandled backend exception");

                apiError = new ApiError(msg, stack);
				context.HttpContext.Response.StatusCode = 500;

				Log.Error(context.Exception, msg);
			}

			// always return a JSON result
			context.Result = new JsonResult(JsonConvert.SerializeObject(apiError));

			base.OnException(context);
		}
	}
}
