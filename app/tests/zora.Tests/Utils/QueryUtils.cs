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
                Page = -1,
                PageSize = -50,
                SortColumn = "Name",
                SortDirection = "asc",
                SearchTerm = "Admin"
            };
        }

        public static QueryParamsDto GetSearchQueryParams(string searchTerm = "Admin")
        {
            return new QueryParamsDto
            {
                Page = 1,
                PageSize = 10,
                SearchTerm = searchTerm,
                SortColumn = "Name",
                SortDirection = "asc"
            };
        }

        public static QueryParamsDto GetNonAdminSearchQueryParams()
        {
            return new QueryParamsDto
            {
                Page = 0,
                PageSize = 1000,
                SearchTerm = string.Empty,
                SortColumn = "Name",
                SortDirection = "asc"
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
