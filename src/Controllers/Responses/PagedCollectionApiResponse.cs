using System;
using System.Collections.Generic;

namespace Package.WebApi.Controllers.Responses
{
    public class PagedCollectionApiResponse<T>
            where T : class
    {
        public PagedCollectionApiResponse(IEnumerable<T> data)
        {
            Items = data;
        }

        public IEnumerable<T> Items { get; }
        public Uri NextPage { get; set; }
        public Uri PreviousPage { get; set; }
    }
}