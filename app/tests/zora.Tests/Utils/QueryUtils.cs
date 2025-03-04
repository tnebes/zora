#region

using zora.Core.DTOs.Requests;

#endregion

namespace zora.Tests.Utils;

public static class QueryUtils
{
    public static class QueryParamUtils
    {
        public static QueryParamsDto GetValidQueryParams()
        {
            return new QueryParamsDto
            {
                Page = 1,
                PageSize = 50,
                SearchTerm = "",
                SortColumn = "asc",
                SortDirection = "Id"
            };
        }

        public static QueryParamsDto GetLargePageSizeQueryParams()
        {
            return new QueryParamsDto
            {
                Page = 1,
                PageSize = 10000,
                SearchTerm = "",
                SortColumn = "asc",
                SortDirection = "Id"
            };
        }

        public static QueryParamsDto GetInvalidQueryParams()
        {
            return new QueryParamsDto
            {
                Page = 0,
                PageSize = 0,
                SearchTerm = "",
                SortColumn = "invalid",
                SortDirection = "invalid"
            };
        }
    }

    public static class DynamicQueryParamsUtils
    {
        public static DynamicQueryUserParamsDto GetValidDynamicQueryUserParams()
        {
            return new DynamicQueryUserParamsDto
            {
                Page = 1,
                PageSize = 50,
                Email = "john.doe@example.com,jane.doe@example.com"
            };
        }
    }
}
