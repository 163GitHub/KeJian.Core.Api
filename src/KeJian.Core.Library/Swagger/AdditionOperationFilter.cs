﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KeJian.Core.Library.Dto;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace KeJian.Core.Library.Swagger
{
    public class AdditionOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (context == null) return;

            var actualReturnType = context.MethodInfo.ReturnType.Name == "Task`1"
                ? context.MethodInfo.ReturnType.GenericTypeArguments.FirstOrDefault()
                : context.MethodInfo.ReturnType;

            if (actualReturnType == null || actualReturnType.Name == "ApiResult`1" ||
                actualReturnType.Name == "ApiResult") return;

            var wrapApiResultReturnType = actualReturnType == typeof(void) || actualReturnType == typeof(Task)
                ? typeof(ApiResult)
                : typeof(ApiResult<>).MakeGenericType(actualReturnType);

            operation?.Responses?.Remove("200");
            operation?.Responses?.Add("200",
                new OpenApiResponse
                {
                    Description = "Success",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = context.SchemaGenerator.GenerateSchema(wrapApiResultReturnType,
                                    context.SchemaRepository)
                            }
                        }
                    }
                });
        }
    }
}